using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public struct SPHParticles
{
    public Sprite mesh;
    public Vector3 position;
    public Vector3 velocity;
    public float density;
    public float pressure;
}

public struct BoundLimits
{
    public float ymin;
    public float ymax;
    public float xmin;
    public float xmax;
}

public class SimulationArea : MonoBehaviour
{

    public float particleScale = 1.0f;
    public float gravity = -9.8f;
    public GameObject circle;
    public int NumParticles = 10;
    private GameObject[,] _particles;
    private Vector3[,] _velocities;
    [SerializeField] Vector2 limits;

    private int NumTotalOfParticles
    {  get { return NumParticles * NumParticles; } }

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
        _particles = new GameObject[NumParticles, NumParticles];
        _velocities = new Vector3[NumParticles, NumParticles];

        for (int i = 0; i < NumParticles; ++i)
        {
            for (int j = 0; j < NumParticles; ++j)
            {

                _particles[i, j] = Instantiate(circle);
                _particles[i, j].transform.localScale = new Vector3(particleScale, particleScale, particleScale);
                Vector3 position = new Vector2(i * particleScale, j * particleScale);
                _particles[i, j].transform.position = position;

            }
        }
    }

    private void ApplyForcesOnParticles()
    {
        for (int i = 0; i < NumParticles; ++i)
        {
            for (int j = 0; j < NumParticles; ++j)
            {

                Vector3 particlePos = _particles[i, j].transform.position;

                if (particlePos.y <= limits.y)
                {
                    _velocities[i, j] = new Vector3(0.0f, 0.0f);
                    _particles[i, j].transform.position.Set(particlePos.x, limits.y, particlePos.z);
                }
                else
                {

                    _velocities[i, j].y += gravity * Time.deltaTime;
                    _particles[i, j].transform.position += _velocities[i, j] * Time.deltaTime;

                }
                
            }
        }
    }
}
