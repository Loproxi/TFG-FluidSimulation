using System.Collections;
using System.Collections.Generic;
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

    [Header("SPH Related")]
    private float smoothDensityRadius = 15.0f;
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
        ApplyForcesOnParticles();
    }

    private void GenerateParticles()
    {
        particleBoundArea = particleBoundObject.GetComponent<ParticleBoundArea>();
        _particles = new GameObject[NumTotalOfParticles];
        _sphparticles = new SPH_Particles[NumTotalOfParticles];
        _directions = new Vector3[NumTotalOfParticles];

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
        }
    }

    private void ApplyForcesOnParticles()
    {
        
        for (int i = 0; i < particleBoundArea.NumParticles; i++)
        {

            Vector3 particlePos = _particles[i].transform.position;

            float particleRadius = particleBoundArea.particleScale / 2;

            if (particleBoundArea.IsParticleInsideBounds(particlePos, particleRadius))
            {

                // Inside Limits
                _sphparticles[i].velocity += Vector2.down * gravity * Time.deltaTime;

                ComputePressure(particlePos);

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
            
        }
    }

    

    float ComputeDensity(Vector3 posToCompute)
    {

        // TODO: Compute only the with the ones inside of the circle (grid partitioning)

        //Iterate all the particles summing all the masses multiplied by the smoothing Kernel

        float density = 0.0f;
        float mass = 1.0f;

        foreach (var particle in _particles)
        {
            
            float dist = (posToCompute - particle.transform.position).magnitude;
            if (dist < smoothDensityRadius)
            {
                float influence = Tools.Ver_2_SmoothDensityKernel(smoothDensityRadius, dist);
                density += mass * influence;
            }
            else
            {
                continue;
            }

        }

        return density;
    }

    Vector3 ComputePressure(Vector3 posToCompute)
    {
        // TODO: Compute only the with the ones inside of the circle (grid partitioning)

        //Iterate all the particles summing all the masses multiplied by the smoothing Kernel

        Vector3 pressure = Vector3.zero;
        
        float particlePressure = 1.0f;
        float particleMass = 1.0f;

        foreach (var particle in _particles)
        {
            if(particle.transform.position == posToCompute)
            {
                continue;
            }

            float dist = (posToCompute - particle.transform.position).magnitude;
            Vector3 dir = (posToCompute - particle.transform.position) / dist;
            float slope = Tools.Derivative_Ver_2_SmoothDensityKernel(smoothDensityRadius, dist);
            float density = ComputeDensity(particle.transform.position);

            pressure += particlePressure * dir * slope * particleMass / density;

            
        }

        FillDirectionsVec(pressure);

        return pressure;
    }

    void FillDirectionsVec(Vector3 pressure)
    {

        _directions[counter] = pressure;

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
