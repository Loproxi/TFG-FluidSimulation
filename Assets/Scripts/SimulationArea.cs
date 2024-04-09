using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SimulationArea : MonoBehaviour
{

    struct SPHParticles
    {
        public Sprite mesh;
        public Vector3 position;
        public Vector3 velocity;
    }

    public GameObject circle;
    public int NumParticles = 10;
    private SPHParticles[] _particles;

    void Start()
    {
        _particles = new SPHParticles[NumParticles];

        for (int i = 0; i < NumParticles; i++)
        {
            _particles[i].mesh = circle.GetComponent<SpriteRenderer>().sprite;
            _particles[i].position = new Vector3(i * 5, 0, 0);
            _particles[i].velocity = Vector3.zero;

            Instantiate(circle, _particles[i].position, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
