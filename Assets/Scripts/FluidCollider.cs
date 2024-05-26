using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;


public struct FluidCircle
{
    public float radius;
    public Vector2 center;
    public TYPE_OF_COLLIDER type;

    public FluidCircle(Vector2 center,float radius,TYPE_OF_COLLIDER type)
    {
        this.radius = radius;
        this.center = center;
        this.type = type;
    }

}
public enum TYPE_OF_COLLIDER
{
    CIRCLE,
    QUAD,
    MAX
}
public class FluidCollider : MonoBehaviour
{

    public TYPE_OF_COLLIDER _type;

    public List<FluidCircle> CreateCirclesFromSprite(GameObject mesh, int gridResolution,TYPE_OF_COLLIDER type)
    {

        List<FluidCircle> circles = new List<FluidCircle>();
        SpriteRenderer sprite = mesh.GetComponent<SpriteRenderer>();

        if (sprite == null)
        {
            Debug.LogError("SpriteRenderer component not found on the provided GameObject.");
            return circles;
        }

        Bounds bounds = sprite.bounds;
        Vector2 min = bounds.min;
        Vector2 max = bounds.max;

        float cellWidth = (max.x - min.x) / gridResolution;
        float cellHeight = (max.y - min.y) / gridResolution;

        switch (type)
        {
            case TYPE_OF_COLLIDER.CIRCLE:
                // Create circles based on the grid resolution
                
                float cellRadius = Mathf.Max(cellWidth, cellHeight) / 2;

                circles.Add(new FluidCircle(mesh.transform.position, mesh.transform.localScale.x/2, type));  
                
                break;
            case TYPE_OF_COLLIDER.QUAD:
                for (int i = 0; i < gridResolution; i++)
                {
                    for (int j = 0; j < gridResolution; j++)
                    {
                        float cellMinX = min.x + i * cellWidth;
                        float cellMaxX = cellMinX + cellWidth;
                        float cellMinY = min.y + j * cellHeight;
                        float cellMaxY = cellMinY + cellHeight;

                        Vector2 cellCenter = new Vector2((cellMinX + cellMaxX) / 2, (cellMinY + cellMaxY) / 2);
                        float cellRad = Mathf.Max(cellWidth, cellHeight) / 2;

                        circles.Add(new FluidCircle(cellCenter, cellRad, type));
                    }
                }
                break;
            default:
                Debug.LogError("Unsupported collider type.");
                break;
        }

        return circles;

    }

}
