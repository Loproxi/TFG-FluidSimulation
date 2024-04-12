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

    public float particleScale = 1.0f;
    public float gravity = -9.8f;
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

                if(particlePos.x >= limits.x && particlePos.y >= limits.y && particlePos.x <= limits.x + width && particlePos.y <= limits.y + height)
                {
                    // Particle inside limits

                    _velocities[i, j].y += gravity * Time.deltaTime;
                    _velocities[i, j].x += 2.0f * Time.deltaTime;
                    _particles[i, j].transform.position += _velocities[i, j] * Time.deltaTime;
                }
                else
                {
                    // out of limits

                    _velocities[i, j] = Vector3.zero;
                    _particles[i, j].transform.position = new Vector3(
                        Mathf.Clamp(particlePos.x, limits.x, limits.x + width),
                        Mathf.Clamp(particlePos.y, limits.y, limits.y + height),
                        particlePos.z
                    );

                }
                
            }
        }
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
        Gizmos.DrawLine(new Vector3(limits.x, limits.y), new Vector3(limits.x + width, limits.y));
        Gizmos.DrawLine(new Vector3(limits.x, limits.y), new Vector3(limits.x, limits.y + height));
        Gizmos.DrawLine(new Vector3(limits.x + width, limits.y), new Vector3(limits.x + width, limits.y + height));
        Gizmos.DrawLine(new Vector3(limits.x, limits.y + height), new Vector3(limits.x + width, limits.y + height));
    }
}
