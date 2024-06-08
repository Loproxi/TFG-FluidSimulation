using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidInitializer : MonoBehaviour
{
    [Header("Particle Init Settings")]
    public Vector2[] positions;
    public int numParticles = 100;
    public float particleScale = 0.25f;

    [Header("Domain Bounds")]
    public Vector2 minBounds = new Vector2(0,0);
    public Vector2 maxBounds = new Vector2(10,10);

    public void InitializeParticles()
    {
        positions = new Vector2[numParticles];
        GetPositionInBounds();
    }
    void GetPositionInBounds()
    {
        int numRows = Mathf.CeilToInt(Mathf.Sqrt(numParticles));
        int numCols = Mathf.CeilToInt((float)numParticles / numRows);

        // Calculate the space available for each particle
        float xSpacing = (maxBounds.x - minBounds.x) / numCols;
        float ySpacing = (maxBounds.y - minBounds.y) / numRows;

        int particleIndex = 0;

        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                if (particleIndex >= numParticles) return; // Si hemos calculado todas las posiciones, salir

                // Calculate the position of the particle within the bounds
                float xPos = minBounds.x + col * xSpacing + xSpacing / 2;
                float yPos = minBounds.y + row * ySpacing + ySpacing / 2;
                Vector2 spawnPosition = new Vector2(xPos, yPos);

                positions[particleIndex] = spawnPosition;
                particleIndex++;
            }
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

    private void OnDrawGizmos()
    {
        DrawBoundsQuad();
    }

    private void DrawBoundsQuad()
    {
        //Bottom
        Gizmos.DrawLine(new Vector3(minBounds.x, minBounds.y), new Vector3(maxBounds.x, minBounds.y));
        //Left
        Gizmos.DrawLine(new Vector3(minBounds.x, minBounds.y), new Vector3(minBounds.x, maxBounds.y));
        //Right
        Gizmos.DrawLine(new Vector3(maxBounds.x, minBounds.y), new Vector3(maxBounds.x, maxBounds.y));
        //Up
        Gizmos.DrawLine(new Vector3(minBounds.x, maxBounds.y), new Vector3(maxBounds.x , maxBounds.y));

    }
}
