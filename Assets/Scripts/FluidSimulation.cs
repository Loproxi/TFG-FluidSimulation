using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using static UnityEngine.ParticleSystem;


public class FluidSimulation : MonoBehaviour
{
    [Header("Particle Related")]
    public FluidParticle fluidParticle;
    private FluidParticle[] _particles;
    private FluidInitializer _fluidInitializer;

    private SP_Tile tile;
    private float deltaTime = 0.0f;
    private float predictiondeltaTime = 0.0f;
    public Vector2[] velocities;

    public GameObject sphere;

    [Header("SPH Related")]
    [Range(0.0f, 1.0f)]
    public float smoothDensityRadius = 1.0f;
    public float restDensity = 1.0f;
    public float gasConstant = 2.0f;
    [Range(0.0f, 1.0f)]
    public float collisionDamping = 1.0f;
    public float gravity = -9.81f;
    public float viscosity = 0.0f;
    private CompactHashing compactHashing;

    private void Start()
    {
        InitializeSimulation();
    }

    private void FixedUpdate()
    {
        UpdateSimulation();

        for (int i = 0; i < _particles.Length; i++)
        {
            
            SpriteRenderer sprite = _particles[i].GetComponent<SpriteRenderer>();

            if (Mathf.Abs(_particles[i].velocity.x) <= 0.0f && Mathf.Abs(_particles[i].velocity.y) <= 0.0f)
            {
                sprite.color = Color.blue;
            }
            else if(Mathf.Abs(_particles[i].velocity.x) <= 2.0f || Mathf.Abs(_particles[i].velocity.y) <= 1.0f)
            {
                sprite.color = Color.green;
            }
            else if (Mathf.Abs(_particles[i].velocity.x) <= 5.0f || Mathf.Abs(_particles[i].velocity.y) <= 2.0f)
            {
                sprite.color = Color.yellow;
            }
            else
            {
                sprite.color = Color.red;
            }

        }
    }

    #region InitSimulation

    void InitializeSimulation()
    {
        deltaTime = 1 / 60.0f;
        Time.fixedDeltaTime = deltaTime;

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
        UpdateNextPositions();
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
            CheckOtherCollisions(i);
            CheckBoundaryCollisions(i);

        }
    }

    private void UpdateNextPositions()
    {
        predictiondeltaTime = 1 / 120.0f;


        Parallel.For(0, _fluidInitializer.numParticles, particleId =>
        {

            _particles[particleId].ModifyVelocity(new Vector2(0, gravity * deltaTime));

            Vector2 pos = _particles[particleId].position;
            Vector2 vel = _particles[particleId].velocity;

            _particles[particleId].UpdateNextPosition(pos + vel * predictiondeltaTime);

        });
    }

#region SpatialHashing
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

        Vector2 cell = compactHashing.GetCellFromPosition(_particles[particleId].nextPosition);
        uint key = compactHashing.GetKeyFromHashedCell(compactHashing.HashingCell(cell));

        compactHashing.spatialHashingInfo[particleId] = new uint2((uint)particleId, key);
    }

    private void FillSpatialIndices(int it)
    {
        int numParticles = _fluidInitializer.numParticles;
        uint key = compactHashing.spatialHashingInfo[it].y;
        uint beforeKey = uint.MaxValue;

        if ((it-1) >= 0)
        {
            beforeKey = compactHashing.spatialHashingInfo[it - 1].y;
        }

        if (key != beforeKey || it == 0)
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

#endregion

    void ComputeDensity(int particleIndex)
    {
        FluidParticle particle = _particles[particleIndex];
        //Compute radius * radius to avoid computing square
        float radius2 = smoothDensityRadius * smoothDensityRadius;
        float density = 0.0f;
        float nearDens = 0.0f;

        Vector2[] nearCells = compactHashing.SelectSurroundingCells(particle.nextPosition);

        for (uint i = 0; i < nearCells.Length; i++)
        {
            uint key = compactHashing.GetKeyFromHashedCell(compactHashing.HashingCell(nearCells[i]));
            uint index = compactHashing.spatialHashingIndices[key];

            while(index < compactHashing.spatialHashingInfo.Length && compactHashing.spatialHashingInfo[index].y == key)
            {

                uint neighbourIndex = compactHashing.spatialHashingInfo[index].x;

                Vector2 centerToNeighbour = _particles[neighbourIndex].nextPosition - particle.nextPosition;
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
        density = Mathf.Max(density, 0.0001f);
        nearDens = Mathf.Max(nearDens, 0.0001f);
        _particles[particleIndex].UpdateDensity(density);
        _particles[particleIndex].UpdateNearDensity(nearDens);
    }
    void ComputePressureForce(int particleIndex)
    {
        
        FluidParticle particle = _particles[particleIndex];
        //Compute radius * radius to avoid computing square
        float radius2 = smoothDensityRadius * smoothDensityRadius;
        Vector2 pressure = Vector2.zero;

        Vector2[] nearCells = compactHashing.SelectSurroundingCells(particle.nextPosition);

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
                Vector2 particleToOther = (_particles[neighbourIndex].nextPosition - particle.nextPosition);
                float sqrDistFromCenterToNeighbour = Vector2.Dot(particleToOther,particleToOther);
                if (sqrDistFromCenterToNeighbour <= radius2)
                {
                    //Compute Density of those
                    float dist = Mathf.Sqrt(sqrDistFromCenterToNeighbour);
                    Vector2 dir = dist > 0 ? particleToOther / dist : Vector2.up;
                    float slope = Tools.Derivative_Ver_2_SmoothDensityKernel(smoothDensityRadius, dist);
                    //float nearSlope = Tools.Derivative_Ver_1_SmoothNearDensityKernel(smoothDensityRadius, dist);
                    float pressureBetweenParticles = (ConvertDensityIntoPressure(_particles[particleIndex].density) + ConvertDensityIntoPressure(_particles[neighbourIndex].density)) * 0.5f;
                    //float nearPressureBetweenParticles = (_particles[particleIndex].nearDensity + _particles[neighbourIndex].nearDensity) * 0.5f;

                    pressure += particle.mass * dir * slope * pressureBetweenParticles / _particles[neighbourIndex].density;
                    //pressure += dir * nearSlope * nearPressureBetweenParticles / _particles[neighbourIndex].nearDensity;
                }

                index++;
            }
        }

        Vector2 pressureAcceleration = Vector2.zero;

        pressureAcceleration = pressure / _particles[particleIndex].density;

        _particles[particleIndex].ModifyVelocity(pressureAcceleration * deltaTime);
        velocities[particleIndex] = pressureAcceleration * deltaTime;

    }
    float ConvertDensityIntoPressure(float density)
    {
        //If the rest density is achieved particle won't generate pressure
        float pressure = (density - restDensity) * gasConstant;
        return pressure;
    }
    void ComputeViscosityForce(int particleIndex)
    {
        FluidParticle particle = _particles[particleIndex];
        //Compute radius * radius to avoid computing square
        float radius2 = smoothDensityRadius * smoothDensityRadius;
        Vector2 _viscosity = Vector2.zero;

        Vector2[] nearCells = compactHashing.SelectSurroundingCells(particle.nextPosition);

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
                Vector2 particleToOther = (_particles[neighbourIndex].nextPosition - particle.nextPosition);
                float sqrDistFromCenterToNeighbour = Vector2.Dot(particleToOther, particleToOther);
                if (sqrDistFromCenterToNeighbour <= radius2)
                {
                    //Compute Viscosity of those
                    float dist = Mathf.Sqrt(sqrDistFromCenterToNeighbour);
                    float viscInfluence = Tools.Ver_3_SmoothDensityKernel(smoothDensityRadius, dist);

                    _viscosity = particle.mass * (_particles[neighbourIndex].velocity - particle.velocity) / _particles[neighbourIndex].density;
                    _viscosity *= viscInfluence;
                }
                index++;
            }
        }

        _particles[particleIndex].ModifyVelocity(_viscosity * viscosity * deltaTime);

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

    void CheckOtherCollisions(int particleIndex)
    {
        float radius = sphere.transform.localScale.x / 2;
        Vector2 spherePos = sphere.transform.position;
        Vector2 particlePosition = _particles[particleIndex].position;
        Vector2 particleVelocity = _particles[particleIndex].velocity;

        Vector2 dir = particlePosition - spherePos;
        float distance = dir.magnitude;

        if (distance < radius)
        {
            dir.Normalize();

            Vector2 newPosition = spherePos + dir * (radius + _fluidInitializer.particleScale / 2);

            Vector2 newVelocity = particleVelocity - 2 * Vector2.Dot(particleVelocity, dir) * dir * collisionDamping;

            _particles[particleIndex].UpdatePosition(newPosition);
            _particles[particleIndex].UpdateVelocity(newVelocity.x, newVelocity.y);
        }
        else if(distance < radius + _fluidInitializer.particleScale / 2)
        {
            dir.Normalize();
            Vector2 newPosition = spherePos + dir * (radius + _fluidInitializer.particleScale / 2);
            Vector2 newVelocity = particleVelocity - 2 * Vector2.Dot(particleVelocity, dir) * dir * collisionDamping;

            _particles[particleIndex].UpdatePosition(newPosition);
            _particles[particleIndex].UpdateVelocity(newVelocity.x, newVelocity.y);
        }
    }

    void ApplyForces()
    {
        // Compute Acceleration -> velocity -> position -> resolve collisions with bounds

        Parallel.For(0, _fluidInitializer.numParticles, particleId =>
        {
            ComputePressureForce(particleId);

            ComputeViscosityForce(particleId);
        });

        for (int i = 0; i < _fluidInitializer.numParticles; i++)
        {
            _particles[i].MoveParticle(_particles[i].velocity * deltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            foreach (var particle in _particles)
            {
                // Visualiza la densidad como un color
                Gizmos.color = Color.Lerp(Color.blue, Color.red, particle.density / restDensity);
                Gizmos.DrawSphere(particle.position, 0.1f);

                // Visualiza la fuerza de presión como una línea
                Gizmos.color = Color.green;
                Gizmos.DrawLine(particle.position, particle.position + particle.velocity);
            }
        }
    }
}

