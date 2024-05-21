using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using static UnityEngine.ParticleSystem;
using System.Reflection;

public class FluidSimulation : MonoBehaviour
{
    [Header("Particle Related")]
    public FluidParticle fluidParticle;
    private FluidParticle[] _particles;
    private FluidInitializer _fluidInitializer;

    private SP_Tile tile;
    private float deltaTime = 0.0f;
    public Vector2[] velocities;

    [Header("SPH Related")]
    public float smoothDensityRadius = 1.0f;
    public float restDensity = 1.0f;
    public float gasConstant = 2.0f;
    public float collisionDamping = 1.0f;
    private CompactHashing compactHashing;

    private void Start()
    {
        InitializeSimulation();
    }

    private void FixedUpdate()
    {
        deltaTime = Time.fixedDeltaTime;
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
        velocities = new Vector2[_fluidInitializer.numParticles];

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
        UpdateSpatialHashing();

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
    }

    private void UpdateSpatialHashing()
    {
        Parallel.For(0, _fluidInitializer.numParticles, particleId =>
        {

            FillSpatialHashing(particleId);

        });

        SortSpatialHashing();

        Parallel.For(0, _fluidInitializer.numParticles, it =>
        {

            FillSpatialIndices(it);

        });
    }

    private void FillSpatialHashing(int particleId)
    {
        compactHashing.ClearSpatialHashingLists(particleId);

        Vector2 cell = compactHashing.GetCellFromPosition(_particles[particleId].position);
        uint key = compactHashing.GetKeyFromHashedCell(compactHashing.HashingCell(cell));

        compactHashing.spatialHashingInfo[particleId] = new uint2((uint)particleId, key);
    }

    private void FillSpatialIndices(int it)
    {
        int numParticles = _fluidInitializer.numParticles;
        int nextIt = it + 1;
        uint key = compactHashing.spatialHashingInfo[it].y;
        uint nextKey = uint.MaxValue;
        if(nextIt != numParticles)
        {
            nextKey = compactHashing.spatialHashingInfo[nextIt].y;
        }

        if (key != nextKey || it == _fluidInitializer.numParticles - 1)
        {
            compactHashing.spatialHashingIndices[key] = (uint)it;
        }

    }

    private void SortSpatialHashing()
    {
        //Delegate that points to the compare Y Coord function
        Comparison<uint2> sortByY = CompareYCoord;

        Array.Sort(compactHashing.spatialHashingInfo, sortByY);
    }

    private int CompareYCoord(uint2 a, uint2 b)
    {
        return a.y.CompareTo(b.y);
    }

    void ComputeDensity(int particleIndex)
    {
        FluidParticle particle = _particles[particleIndex];
        //Compute radius * radius to avoid computing square
        float radius2 = smoothDensityRadius * smoothDensityRadius;
        float density = 0.0f;
        float nearDens = 0.0f;

        Vector2[] nearCells = compactHashing.SelectSurroundingCells(particle.position);

        for (uint i = 0; i < nearCells.Length; i++)
        {
            uint key = compactHashing.GetKeyFromHashedCell(compactHashing.HashingCell(nearCells[i]));
            uint index = compactHashing.spatialHashingIndices[key];

            while(index < compactHashing.spatialHashingInfo.Length && compactHashing.spatialHashingInfo[index].y == key)
            {

                uint neighbourIndex = compactHashing.spatialHashingInfo[index].x;

                Vector2 centerToNeighbour = particle.position - _particles[neighbourIndex].position;
                float sqrDistFromCenterToNeighbour = Vector2.Dot(centerToNeighbour, centerToNeighbour);

                if (sqrDistFromCenterToNeighbour <= radius2)
                {
                    float dist = Mathf.Sqrt(sqrDistFromCenterToNeighbour);
                    float influence = Tools.Ver_2_SmoothDensityKernel(smoothDensityRadius, dist);
                    float nearInfluence = Tools.Ver_1_SmoothNearDensityKernel(smoothDensityRadius, dist);

                    density += particle.mass * influence;
                    nearDens += particle.mass * nearInfluence;
                }

                index++;
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

        for (uint i = 0; i < nearCells.Length; i++)
        {
            uint key = compactHashing.GetKeyFromHashedCell(compactHashing.HashingCell(nearCells[i]));
            uint index = compactHashing.spatialHashingIndices[key];

            while (index < compactHashing.spatialHashingInfo.Length && compactHashing.spatialHashingInfo[index].y == key)
            {

                uint neighbourIndex = compactHashing.spatialHashingInfo[index].x;

                if (particleIndex == neighbourIndex)
                {
                    index++;
                    continue;
                }
                Vector2 particleToOther = (_particles[neighbourIndex].position - particle.position);
                float sqrDistFromCenterToNeighbour = Vector2.Dot(particleToOther,particleToOther);
                if (sqrDistFromCenterToNeighbour <= radius2)
                {
                    //Compute Density of those
                    float dist = Mathf.Sqrt(sqrDistFromCenterToNeighbour);
                    Vector2 dir = dist > 0 ? particleToOther / dist : Vector2.up;
                    float slope = Tools.Derivative_Ver_2_SmoothDensityKernel(smoothDensityRadius, dist);
                    float nearSlope = Tools.Derivative_Ver_3_SmoothNearDensityKernel(smoothDensityRadius, dist);
                    float pressureBetweenParticles = (ConvertDensityIntoPressure(_particles[particleIndex].density) + ConvertDensityIntoPressure(_particles[neighbourIndex].density)) * 0.5f;
                    float nearPressureBetweenParticles = (ConvertDensityIntoPressure(_particles[particleIndex].nearDensity) + ConvertDensityIntoPressure(_particles[neighbourIndex].nearDensity)) * 0.5f;

                    if (_particles[neighbourIndex].density < float.Epsilon)
                    {
                        index++;
                        continue;
                    }
                    pressure += dir * slope * pressureBetweenParticles / _particles[neighbourIndex].density;
                    pressure += dir * nearSlope * nearPressureBetweenParticles / _particles[neighbourIndex].nearDensity;
                }

                index++;
            }
        }

        Vector2 pressureAcceleration = Vector2.zero;

        if (_particles[particleIndex].density > float.Epsilon)
        {
            pressureAcceleration = pressure / _particles[particleIndex].density;
        }

        _particles[particleIndex].ModifyVelocity(pressureAcceleration * deltaTime);
        velocities[particleIndex] = pressureAcceleration * deltaTime;

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

        _particles[particleIndex].UpdatePosition(new Vector2(
           Mathf.Clamp(particlePosition.x, _fluidInitializer.minBounds.x + particleSize.x, _fluidInitializer.maxBounds.x - particleSize.x),
           Mathf.Clamp(particlePosition.y, _fluidInitializer.minBounds.y + particleSize.y, _fluidInitializer.maxBounds.y - particleSize.y)));

        // Check collisions with the boundaries
        if (particlePosition.x < (_fluidInitializer.minBounds.x + particleSize.x) || particlePosition.x > (_fluidInitializer.maxBounds.x - particleSize.x))
        {
            _particles[particleIndex].UpdateVelocity(particleVelocity.x * -1 * collisionDamping, particleVelocity.y); // Invert X velocity
        }

        if (particlePosition.y < (_fluidInitializer.minBounds.y + particleSize.y) || particlePosition.y > (_fluidInitializer.maxBounds.y - particleSize.y))
        {
            _particles[particleIndex].UpdateVelocity(particleVelocity.x, particleVelocity.y * -1 * collisionDamping); // Invert Y velocity
        }        
    }

    void ApplyForces()
    {
        // Compute Acceleration -> velocity -> position -> resolve collisions with bounds

        //Vector2 gravity = new Vector2(0, -9.81f);

        Parallel.For(0, _fluidInitializer.numParticles, particleId =>
        {
            ComputePressureForce(particleId);
            //_particles[particleId].ModifyVelocity(gravity);
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

