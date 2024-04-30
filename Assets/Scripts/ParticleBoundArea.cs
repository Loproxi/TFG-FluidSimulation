using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBoundArea : MonoBehaviour
{
    [Header("Particles Buffer Related")]
    public Vector2[] _position;
    public Vector2[] _velocity;
    public int NumParticles = 10;
    public float particleScale = 1.0f;

    [Header("Bound Related")]
    [SerializeField] public Vector2 boundInit;
    public float width, height = 10;

    private int NumTotalOfParticles
    { get { return NumParticles * NumParticles; } }

    public void SpawnParticles()
    {
        _position = new Vector2[NumTotalOfParticles];
        _velocity = new Vector2[NumTotalOfParticles];

        for (int i = 0; i < NumTotalOfParticles; i++)
        {

            _position[i] = RandomPosInBounds(particleScale / 2);
            _velocity[i] = Vector2.zero;

        }
    }

    private Vector2 RandomPosInBounds(float particleRadius)
    {

        //Taking into account the particle radius in bounds
        float minX = boundInit.x + particleRadius;
        float maxX = boundInit.x + width - particleRadius;
        float minY = boundInit.y + particleRadius;
        float maxY = boundInit.y + height - particleRadius;

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);

        return new Vector2(x, y);

    }

    public bool IsParticleInsideBounds(Vector3 particlePos)
    {

        //Taking into account the particle radius in limits
        float minX = boundInit.x;
        float maxX = boundInit.x + width;
        float minY = boundInit.y;
        float maxY = boundInit.y + height;

        bool insideBounds = particlePos.x >= minX && particlePos.y >= minY && particlePos.x <= maxX && particlePos.y <= maxY;

        return insideBounds;

    }

    private void OnDrawGizmos()
    {

        //DrawParticles();

        DrawBoundsQuad();

    }

    private void DrawParticles()
    {
        for (int i = 0; i < NumParticles; i++)
        {
            for (int j = 0; j < NumParticles; j++)
            {

                //TODO: Find a way to save the random positions that the particles will spawn without doing inside draw particles

                Vector3 position = new Vector2(i * particleScale, j * particleScale);

                Gizmos.DrawWireSphere(position, particleScale / 2);

            }
        }
    }

    private void DrawBoundsQuad()
    {
        //Bottom
        Gizmos.DrawLine(new Vector3(boundInit.x, boundInit.y), new Vector3(boundInit.x + width, boundInit.y));
        //Left
        Gizmos.DrawLine(new Vector3(boundInit.x, boundInit.y), new Vector3(boundInit.x, boundInit.y + height));
        //Right
        Gizmos.DrawLine(new Vector3(boundInit.x + width, boundInit.y), new Vector3(boundInit.x + width, boundInit.y + height));
        //Up
        Gizmos.DrawLine(new Vector3(boundInit.x, boundInit.y + height), new Vector3(boundInit.x + width, boundInit.y + height));

    }
}
