using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidInitializer : MonoBehaviour
{
    [Header("Particle Init Settings")]
    public Vector2[] positions;
    public int numParticles = 100;
    public float particleScale = 1.0f;

    [Header("Domain Bounds")]
    public Vector2 minBounds = new Vector2(0,0);
    public Vector2 maxBounds = new Vector2(10,10);

    private void Start()
    {
        InitializeParticles();
    }

    void InitializeParticles()
    {
        positions = new Vector2[numParticles];

        for (int i = 0; i < numParticles; i++)
        {
            positions[i] = GetRandomPosition();
        }
    }

    Vector2 GetRandomPosition()
    {
        float particleRadius = particleScale/2;

        float x = Random.Range(minBounds.x + particleRadius, maxBounds.x - particleRadius);
        float y = Random.Range(minBounds.y + particleRadius, maxBounds.y - particleRadius);

        return new Vector2(x, y);
    }

    public bool IsParticleInsideBounds(Vector3 particlePos)
    {
        float particleRadius = particleScale / 10;
        //Taking into account the particle radius in limits
        float minX = minBounds.x + particleRadius;
        float maxX = maxBounds.x - particleRadius;
        float minY = minBounds.y + particleRadius;
        float maxY = maxBounds.y - particleRadius;

        bool insideBounds = particlePos.x >= minX && particlePos.y >= minY && particlePos.x <= maxX && particlePos.y <= maxY;

        return insideBounds;

    }
}
