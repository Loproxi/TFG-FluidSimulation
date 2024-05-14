using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidParticle : MonoBehaviour
{

    public Vector2 position { get; private set; }
    public Vector2 velocity { get; private set; }
    public float mass { get; private set; }
    public float density { get; private set; }

    public void InitializeParticle(Vector2 position, Vector2 velocity, float mass)
    {
        this.position = position;
        this.velocity = velocity;
        this.mass = mass;
    }

    public void UpdatePosition(Vector2 newPos)
    {
        position = newPos;
        gameObject.transform.position = newPos;
    }

    public void UpdateVelocity(Vector2 newVel)
    {
        velocity = newVel;
    }

    public void UpdateDensity(float newDensity)
    {
        density = newDensity;
    }

}
