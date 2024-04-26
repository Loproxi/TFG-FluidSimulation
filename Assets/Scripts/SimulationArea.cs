using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class SimulationArea : MonoBehaviour
{
    [Header("Particle Related")]
    public float gravity = 9.8f;
    public float collisionDamping = 0.6f;
    public Sprite circle;
    public Color pcolor = Color.blue;

    private GameObject[] _particles;
    private Vector3[] _directions;
    public float[] _densities;

    [Header("SPH Related")]
    public float smoothDensityRadius = 8.2f;
    public float targetDensity;
    public float pressureMultiplier;
    public struct SPH_Particles
    {
        public Vector2 position;
        public Vector2 velocity;
        public float density;
        public float mass;
        public Sprite sprite;
        public Vector2 direction;
    }
    private SPH_Particles[] _sphparticles;

    [Header("Bound Related")]
    [SerializeField] GameObject particleBoundObject;
    private ParticleBoundArea particleBoundArea;

    int counter = 0;

    private int NumTotalOfParticles
    {  get { return particleBoundArea.NumParticles * particleBoundArea.NumParticles; } }

    void Start()
    {
        GenerateParticles();
    }

    void Update()
    {
        ComputeDensity();
        ApplyForcesOnParticles();
    }

    private void GenerateParticles()
    {
        particleBoundArea = particleBoundObject.GetComponent<ParticleBoundArea>();
        _particles = new GameObject[NumTotalOfParticles];
        _sphparticles = new SPH_Particles[NumTotalOfParticles];
        _directions = new Vector3[NumTotalOfParticles];
        _densities = new float[NumTotalOfParticles];

        particleBoundArea.SpawnParticles();

        for (int i = 0; i < NumTotalOfParticles; i++)
        {
            _particles[i] = new GameObject();
            _particles[i].transform.position = particleBoundArea._position[i];
            _particles[i].transform.localScale = new Vector3(particleBoundArea.particleScale, particleBoundArea.particleScale, particleBoundArea.particleScale);
            _particles[i].AddComponent<SpriteRenderer>().sprite = circle;
            _particles[i].GetComponent<SpriteRenderer>().color = pcolor;
            _sphparticles[i].position = particleBoundArea._position[i];
            _sphparticles[i].velocity = particleBoundArea._velocity[i];
            _sphparticles[i].sprite = circle;
            _sphparticles[i].mass = 1.0f;
            _sphparticles[i].density = 0.0f;
            _densities[i] = 0.0f;
        }
    }

    private void ApplyForcesOnParticles()
    {
        
        for (int i = 0; i < NumTotalOfParticles; i++)
        {

            Vector3 particlePos = _particles[i].transform.position;

            float particleRadius = particleBoundArea.particleScale / 2;

            if (particleBoundArea.IsParticleInsideBounds(particlePos, particleRadius))
            {
                // Inside Limits
                _sphparticles[i].velocity += Vector2.down * gravity * Time.deltaTime;

                Vector2 pressure = ComputePressure(i);
                // F = M * A -> A = F/M
                Vector2 pressureAcceleration = _densities[i] == 0 ? Vector2.zero : pressure / _densities[i];

                //Debug.Log("Density Value = " + _densities[i]);

                _sphparticles[i].velocity += pressureAcceleration * Time.deltaTime;

                particleBoundArea._velocity[i] = _sphparticles[i].velocity;

                // Increment the counter
                counter++;

                // Reset the counter if it exceeds the maximum number of particles
                if (counter >= particleBoundArea.NumParticles * particleBoundArea.NumParticles)
                {
                    counter = 0;
                }

            }
            else
            {

                //Out of limits
                _sphparticles[i].velocity *= -1 * collisionDamping;

                //Assure that our particles are set back to the limits
                Vector3 clampedPos = particlePos;
                clampedPos.x = Mathf.Clamp(clampedPos.x, particleBoundArea.boundInit.x + particleRadius, particleBoundArea.boundInit.x + particleBoundArea.width - particleRadius);
                clampedPos.y = Mathf.Clamp(clampedPos.y, particleBoundArea.boundInit.y + particleRadius, particleBoundArea.boundInit.y + particleBoundArea.height - particleRadius);
                _particles[i].transform.position = clampedPos;

            }

            _particles[i].transform.position += new Vector3(_sphparticles[i].velocity.x * Time.deltaTime, _sphparticles[i].velocity.y * Time.deltaTime);
            _sphparticles[i].position = _particles[i].transform.position;


        }
    }

    

    void ComputeDensity()
    {

        // TODO: Compute only the with the ones inside of the circle (grid partitioning)
        //Iterate all the particles summing all the masses multiplied by the smoothing Kernel

        //How many threads do we have?
        int numOfThreads = Environment.ProcessorCount;
        //How many particles each thread should compute
        int particlesPerThread = NumTotalOfParticles/numOfThreads;

        List<Task> tasks = new List<Task>();

        for (int thread = 0; thread < numOfThreads; thread++)
        {
            
            int startParticleId = thread * particlesPerThread;
            //The last thread takes a little bit more or less than the others
            int endParticleId = (thread == numOfThreads - 1) ? NumTotalOfParticles : startParticleId + particlesPerThread;
            tasks.Add(Task.Run(() => ComputeDensityInParallel(startParticleId, endParticleId)));

        }

        Task.WaitAll(tasks.ToArray());

    }

    void ComputeDensityInParallel(int startId,int endId)
    {

        Parallel.For(startId, endId, particleId =>
        {
            float density = 0.0f;
            float mass = 1.0f;

            for (int otherId = 0; otherId < NumTotalOfParticles; otherId++)
            {
                if (particleId == otherId) { continue; }

                Vector2 particleToOther = (_sphparticles[otherId].position - _sphparticles[particleId].position);
                float dist = particleToOther.magnitude;

                float influence = Tools.Ver_2_SmoothDensityKernel(smoothDensityRadius, dist);
                density += mass * influence;
            }
            _densities[particleId] = density;
        });

    }

    Vector2 ComputePressure(int particleId)
    {
        // TODO: Compute only the with the ones inside of the circle (grid partitioning)

        //Iterate all the particles summing all the masses multiplied by the smoothing Kernel

        Vector2 pressure = Vector2.zero;
        
        float particleMass = 1.0f;

        for (int otherId = 0; otherId < NumTotalOfParticles; otherId++)
        {
            if(particleId == otherId) {continue;}

            Vector2 particleToOther = (_sphparticles[otherId].position - _sphparticles[particleId].position);
            float dist = particleToOther.magnitude;
            Vector2 dir = Vector2.zero;
            if (dist == 0)
            {
                dir = new Vector2(UnityEngine.Random.Range(-1,1), UnityEngine.Random.Range(-1, 1));
            }
            else
            {
                dir = particleToOther / dist;
            }    
            float slope = Tools.Derivative_Ver_2_SmoothDensityKernel(smoothDensityRadius, dist);
            float density = _densities[otherId] == 0 ? 0.0001f : _densities[otherId];

            pressure += ConvertDensityIntoPressure(density) * dir * slope * particleMass / density;
           
        }

        FillDirectionsVec(pressure);

        return pressure;
    }

    void FillDirectionsVec(Vector3 pressure)
    {

        _directions[counter] = pressure;

    }

    float ConvertDensityIntoPressure(float density)
    {
        float error = density - targetDensity;
        float pressure = error * pressureMultiplier;
        return pressure;
    }
    private void OnDrawGizmos()
    {

        if (_directions != null && _directions.Length > 0 && Application.isPlaying)
        {

            for (int i = 0; i < NumTotalOfParticles; i++)
            {

                Gizmos.DrawLine(_particles[i].transform.position, _particles[i].transform.position + _directions[i]);
                
            }
        }
    }

}
