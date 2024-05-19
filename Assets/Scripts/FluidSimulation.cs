using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FluidSimulation : MonoBehaviour
{
    [Header("Particle Related")]
    public FluidParticle fluidParticle;
    private FluidParticle[] _particles;
    private FluidInitializer _fluidInitializer;

    private SP_Tile tile;
    private float deltaTime = 0.0f;

    [Header("SPH Related")]
    public float smoothDensityRadius = 1.0f;
    public float restDensity = 1.0f;
    public float gasConstant = 2.0f;
    public float collisionDamping = 1.0f;
    public float timestep = 0.0001f;
    private CompactHashing compactHashing;

    private void Start()
    {
        InitializeSimulation();
    }

    private void Update()
    {
        deltaTime = Time.deltaTime;
        UpdateSimulation();
    }

    #region InitSimulation

    void InitializeSimulation()
    {

        _fluidInitializer = gameObject.GetComponent<FluidInitializer>();
        
        if(_fluidInitializer != null )
        {
            _fluidInitializer.InitializeParticles();
            SpawnParticles();
            tile = new SP_Tile();
            compactHashing = new CompactHashing(_fluidInitializer.numParticles, tile.width, tile.height);

        }
        else
        {
            Debug.Log("This simulation doesn't have the fluid initializer component");
        }
    }

    void SpawnParticles()
    {
        _particles = new FluidParticle[_fluidInitializer.numParticles];

        for (int i = 0; i < _fluidInitializer.numParticles; i++)
        {
            _particles[i] = Instantiate(fluidParticle, gameObject.transform);
            _particles[i].InitializeParticle(_fluidInitializer.positions[i],Vector2.zero,1.0f);
            _particles[i].name = $"Particle: {i}";
        }
    }

    #endregion

    void UpdateSimulation()
    {
        //First -> Update GridPartitioning && Compute Density
        Parallel.For(0, _fluidInitializer.numParticles, particleId =>
        {
            UpdateSpatialHashing(particleId);
        });

        Parallel.For(0, _fluidInitializer.numParticles, particleId =>
        {
            ComputeDensity(particleId);
        });
        
        //Second Apply the forces (Pressure & Viscosity)
        ApplyForces();

        for (int i = 0; i < _fluidInitializer.numParticles; i++)
        {
            CheckBoundaryCollisions(i);
        }

        compactHashing.ClearSpatialHashingLists();
    }

    private void UpdateSpatialHashing(int particleId)
    {

        Vector2 cell = compactHashing.GetCellFromPosition(_particles[particleId].position);
        uint key = compactHashing.GetKeyFromHashedCell(compactHashing.HashingCell(cell));

        lock (compactHashing.spatialHashingInfo)
        {
            if (compactHashing.spatialHashingInfo.ContainsKey((int)key) == false)
            {
                compactHashing.spatialHashingInfo[(int)key] = new List<int>();
            }

            compactHashing.spatialHashingInfo[(int)key].Add(particleId);
        }
    }

    void ComputeDensity(int particleIndex)
    {
        FluidParticle particle = _particles[particleIndex];
        Vector2 centerCell = compactHashing.GetCellFromPosition(particle.position);
        //Compute radius * radius to avoid computing square
        float radius2 = smoothDensityRadius * smoothDensityRadius;
        float density = 0.0f;
        float nearDens = 0.0f;

        Vector2[] nearCells = compactHashing.SelectSurroundingCells(particle.position);

        for (int i = 1; i < nearCells.Length; i++)
        {
            uint key = compactHashing.GetKeyFromHashedCell(compactHashing.HashingCell(nearCells[i]));

            //TODO: Sometimes if the nearCell Coords are negative the key is negative also and that produces that cellData doesnt work because there are no negative index
            if (compactHashing.spatialHashingInfo.ContainsKey((int)key))
            {

                for (int j = 0; j < compactHashing.spatialHashingInfo[(int)key].Count; j++)
                {
                    int neighbourIndex = compactHashing.spatialHashingInfo[(int)key][j];
                    if (particleIndex == neighbourIndex) continue;

                    Vector2 centerToNeigbour = particle.position - _particles[neighbourIndex].position;
                    float sqrDistFromCenterToNeighbour = Vector2.Dot(centerToNeigbour, centerToNeigbour);
                    if (sqrDistFromCenterToNeighbour > radius2) continue;               

                    Debug.Log($"ParticleIndex: {particleIndex} has this NeighbourIndex {neighbourIndex} in radius");

                    //Compute Density of those
                    float dist = Mathf.Sqrt(sqrDistFromCenterToNeighbour);
                    float nearinfluence = Tools.Ver_1_SmoothNearDensityKernel(smoothDensityRadius, dist);
                    float influence = Tools.Ver_2_SmoothDensityKernel(smoothDensityRadius, dist);

                    density += particle.mass * influence;
                    nearDens += particle.mass * nearinfluence;
                }
            }
        }
        _particles[particleIndex].UpdateDensity(density);
        _particles[particleIndex].UpdateNearDensity(nearDens);
    }
    void ComputePressureForce(int particleIndex)
    {
        
        FluidParticle particle = _particles[particleIndex];
        //Compute radius * radius to avoid computing square
        float radius2 = smoothDensityRadius * smoothDensityRadius;
        Vector2 pressure = Vector2.zero;

        Vector2[] nearCells = compactHashing.SelectSurroundingCells(particle.position);

        for (int i = 1; i < nearCells.Length; i++)
        {
            uint key = compactHashing.GetKeyFromHashedCell(compactHashing.HashingCell(nearCells[i]));

            //TODO: Sometimes if the nearCell Coords are negative the key is negative also and that produces that cellData doesnt work because there are no negative index
            if (compactHashing.spatialHashingInfo.ContainsKey((int)key))
            {

                for (int j = 0; j < compactHashing.spatialHashingInfo[(int)key].Count; j++)
                {
                    int neighbourIndex = compactHashing.spatialHashingInfo[(int)key][j];
                    if (particleIndex == neighbourIndex) continue;

                    Vector2 particleToOther = (_particles[neighbourIndex].position - particle.position);
                    float sqrDistFromCenterToNeighbour = Vector2.Dot(particleToOther,particleToOther);
                    if (sqrDistFromCenterToNeighbour > radius2) continue;

                    //Compute Density of those
                    float dist = Mathf.Sqrt(sqrDistFromCenterToNeighbour);
                    Vector2 dir = dist > 0 ? particleToOther/dist : Vector2.up;
                    float slope = Tools.Derivative_Ver_2_SmoothDensityKernel(smoothDensityRadius, dist);
                    float nearSlope = Tools.Derivative_Ver_3_SmoothNearDensityKernel(smoothDensityRadius, dist);
                    float pressureBetweenParticles = (ConvertDensityIntoPressure(_particles[particleIndex].density) + ConvertDensityIntoPressure(_particles[neighbourIndex].density)) * 0.5f;
                    float nearPressureBetweenParticles = (ConvertDensityIntoPressure(_particles[particleIndex].nearDensity) + ConvertDensityIntoPressure(_particles[neighbourIndex].nearDensity)) * 0.5f;


                    if (_particles[neighbourIndex].density < float.Epsilon) continue;

                    pressure += dir * slope * pressureBetweenParticles / _particles[neighbourIndex].density;
                    pressure += dir * nearSlope * nearPressureBetweenParticles / _particles[neighbourIndex].nearDensity;
                }
            }
        }

        Vector2 pressureAcceleration = Vector2.zero;

        if (_particles[particleIndex].density > float.Epsilon)
        {
            pressureAcceleration = pressure / _particles[particleIndex].density;
        }

        _particles[particleIndex].ModifyVelocity(pressureAcceleration * deltaTime);

    }

    float ConvertDensityIntoPressure(float density)
    {
        float pressure = gasConstant * (density - restDensity);
        return pressure;
    }

    private void CheckBoundaryCollisions(int particleIndex)
    {
        
        Vector2 particlePosition = _particles[particleIndex].position;
        Vector2 particleVelocity = _particles[particleIndex].velocity;

        Vector2 particleSize = Vector2.one * _fluidInitializer.particleScale;

        // Check collisions with the boundaries
        if (particlePosition.x < (_fluidInitializer.minBounds.x + particleSize.x) || particlePosition.x > (_fluidInitializer.maxBounds.x - particleSize.x))
        {
            _particles[particleIndex].UpdateVelocity(particleVelocity.x * -1 * collisionDamping, particleVelocity.y); // Invert X velocity
        }

        if (particlePosition.y < (_fluidInitializer.minBounds.y + particleSize.y) || particlePosition.y > (_fluidInitializer.maxBounds.y - particleSize.y))
        {
            _particles[particleIndex].UpdateVelocity(particleVelocity.x, particleVelocity.y * -1 * collisionDamping); // Invert Y velocity
        }


        // Ensure the particle stays within bounds
        _particles[particleIndex].UpdatePosition( new Vector2(
           Mathf.Clamp(particlePosition.x, _fluidInitializer.minBounds.x + particleSize.x, _fluidInitializer.maxBounds.x - particleSize.x),
           Mathf.Clamp(particlePosition.y, _fluidInitializer.minBounds.y + particleSize.y, _fluidInitializer.maxBounds.y - particleSize.y)));
        
    }
    void ApplyForces()
    {
        // Compute Acceleration -> velocity -> position -> resolve collisions with bounds

        Vector2 gravity = new Vector2(0, -9.81f);

        Parallel.For(0, _fluidInitializer.numParticles, particleId =>
        {
            ComputePressureForce(particleId);
            _particles[particleId].ModifyVelocity(gravity);
        });

        for (int i = 0; i < _fluidInitializer.numParticles; i++)
        {
            _particles[i].MoveParticle(_particles[i].velocity * deltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        //foreach (var particle in _particles)
        //{
        //    Gizmos.DrawWireSphere(particle.position, smoothDensityRadius);
        //}
    }
}

