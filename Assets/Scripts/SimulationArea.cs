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

public class SimulationArea : MonoBehaviour
{
    [Header("Particle Related")]
    public float particleScale = 1.0f;
    public float gravity = 9.8f;
    public float collisionDamping = 0.6f;
    public GameObject circle;
    public int NumParticles = 10;
    private GameObject[,] _particles;
    private Vector3[,] _velocities;

    [Header("Bound Related")]
    [SerializeField] Vector2 limits;
    public float width,height = 10;

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

                float particleRadius = particleScale / 2;

                if (IsParticleInsideBounds(particlePos, particleRadius))
                {

                    // Inside Limits
                    _velocities[i, j] += Vector3.down * gravity * Time.deltaTime;
                    
                }
                else
                {

                    //Out of limits
                    _velocities[i, j] *= -1 * collisionDamping;

                    //Assure that our particles are set back to the limits
                    Vector3 clampedPos = particlePos;
                    clampedPos.x = Mathf.Clamp(clampedPos.x, limits.x + particleRadius, limits.x + width - particleRadius);
                    clampedPos.y = Mathf.Clamp(clampedPos.y, limits.y + particleRadius, limits.y + height - particleRadius);
                    _particles[i, j].transform.position = clampedPos;

                }

                _particles[i, j].transform.position += _velocities[i, j] * Time.deltaTime;
            }
        }
    }

    private bool IsParticleInsideBounds(Vector3 particlePos,float particleRadius)
    {

        //Taking into account the particle radius in limits
        float minX = limits.x + particleRadius;
        float maxX = limits.x + width - particleRadius;
        float minY = limits.y + particleRadius;
        float maxY = limits.y + height - particleRadius;

        bool insideBounds = particlePos.x >= minX && particlePos.y >= minY && particlePos.x <= maxX && particlePos.y <= maxY;

        return insideBounds;

    }

    private void OnDrawGizmos()
    {      

        for (int i = 0; i < NumParticles; i++)
        {
            for (int j = 0; j < NumParticles; j++)
            {

                Vector3 position = new Vector2(i * particleScale, j * particleScale);

                Gizmos.DrawWireSphere(position, particleScale/2);

            }
        }

        DrawBoundsQuad();
    }

    private void DrawBoundsQuad()
    {
        //Bottom
        Gizmos.DrawLine(new Vector3(limits.x, limits.y), new Vector3(limits.x + width, limits.y));
        //Left
        Gizmos.DrawLine(new Vector3(limits.x, limits.y), new Vector3(limits.x, limits.y + height));
        //Right
        Gizmos.DrawLine(new Vector3(limits.x + width, limits.y), new Vector3(limits.x + width, limits.y + height));
        //Up
        Gizmos.DrawLine(new Vector3(limits.x, limits.y + height), new Vector3(limits.x + width, limits.y + height));
    }
}
