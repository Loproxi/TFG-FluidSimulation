using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FluidSimulation : MonoBehaviour
{
    [Header("Particle Related")]
    public GameObject fluidParticle;
    private FluidParticle[] _particles;
    private FluidInitializer _fluidInitializer;

    private SP_Tile tile;

    [Header("SPH Related")]
    public float smoothDensityRadius = 1.0f;
    private CompactHashing compactHashing;

    private void Start()
    {
        InitializeSimulation();
    }

    private void FixedUpdate()
    {
        
    }

    #region InitSimulation

    void InitializeSimulation()
    {

        _fluidInitializer = gameObject.GetComponent<FluidInitializer>();
        compactHashing = new CompactHashing(_fluidInitializer.numParticles, tile.width, tile.height);

        if(_fluidInitializer != null )
        {
            SpawnParticles();
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
            Instantiate(fluidParticle, gameObject.transform);
            _particles[i].UpdatePosition(_fluidInitializer.positions[i]);
            _particles[i].name = $"Particle: {i}";
        }
    }

    #endregion

    void UpdateSimulation()
    {
        //First Compute Density
        Parallel.For(0, _fluidInitializer.numParticles, particleId =>
        {
            ComputeDensity(particleId);
        });
        
        //Second Apply the forces (Pressure & Viscosity)
        ApplyForces();

    }

    void ComputeDensity(int particleIndex)
    {
        FluidParticle particle = _particles[particleIndex];
        Vector2 centerCell = compactHashing.GetCellFromPosition(particle.position);
        //Compute radius * radius to avoid computing square
        float radius2 = smoothDensityRadius * smoothDensityRadius;
        float density = 0.0f;

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

                    float sqrDistFromCenterToNeighbour = (particle.position - _particles[neighbourIndex].position).sqrMagnitude;
                    if (sqrDistFromCenterToNeighbour > radius2) continue;               

                    Debug.Log($"ParticleIndex: {particleIndex} has this NeighbourIndex {neighbourIndex} in radius");

                    //Compute Density of those
                    float dist = Mathf.Sqrt(sqrDistFromCenterToNeighbour);
                    float influence = Tools.Ver_2_SmoothDensityKernel(smoothDensityRadius, dist);

                    density += particle.mass * influence;                                     
                }
            }
        }
        _particles[particleIndex].UpdateDensity(density);
    }

    void ApplyForces()
    {
        
    }

    void ComputePressureForce()
    {

    }
}

