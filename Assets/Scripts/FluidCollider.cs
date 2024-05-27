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
    public ColliderType type;

    public FluidCircle(Vector2 center,float radius,ColliderType type)
    {
        this.radius = radius;
        this.center = center;
        this.type = type;
    }

}

public class FluidCollider : MonoBehaviour,IFluidCollider
{

    public ColliderType Type => ColliderType.OTHER;
    public int gridResolution = 1;

    public void ResolveCollision(ref FluidParticle particle, float particleRadius, float collisionDamping)
    {
        Vector2 particlePosition = particle.position;
        Vector2 particleVelocity = particle.velocity;

        List<FluidCircle> list = new List<FluidCircle>();

        list = CreateCirclesFromSprite(gridResolution);

        foreach (var circle in list)
        {
            
            //Check each circle in the complex mesh

        }
    }


    public List<FluidCircle> CreateCirclesFromSprite(int gridResolution)
    {

        List<FluidCircle> circles = new List<FluidCircle>();
        SpriteRenderer sprite = gameObject.GetComponent<SpriteRenderer>();

        if (sprite == null)
        {
            Debug.LogError("SpriteRenderer component not found on the provided GameObject.");
            return circles;
        }

        Bounds bounds = sprite.bounds;
        Vector2 min = bounds.min;
        Vector2 max = bounds.max;


        return circles;
    }
}
