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

    private int NumTotalOfParticles
    {  get { return NumParticles * NumParticles; } }

    void Start()
    {
        _particles = new GameObject[NumParticles,NumParticles];
        _velocities = new Vector3[NumParticles, NumParticles];

        for (int i = 0; i < NumParticles; ++i)
        {
            for (int j = 0; j < NumParticles; ++j)
            {

                _particles[i,j] = Instantiate(circle);
                _particles[i,j].transform.localScale = new Vector3(particleScale,particleScale,particleScale);
                Vector3 position = new Vector2(i * particleScale, j * particleScale);
                _particles[i, j].transform.position = position;

            }
        }
    }

    void Update()
    {
        for (int i = 0; i < NumParticles; ++i)
        {
            for (int j = 0; j < NumParticles; ++j)
            {
                _velocities[i, j].y += gravity * Time.deltaTime;
                _particles[i, j].transform.position += _velocities[i, j] * Time.deltaTime;
            }
        }
    }
}
