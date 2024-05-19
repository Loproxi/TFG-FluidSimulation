using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidParticle : MonoBehaviour
{

    public Vector2 position { get; private set; }
    public Vector2 velocity { get; private set; }
    public float mass { get; private set; }
    public float density { get; private set; }
    public float nearDensity { get; private set; }

    public void InitializeParticle(Vector2 position, Vector2 velocity, float mass)
    {
        this.position = position;
        this.velocity = velocity;
        this.mass = mass;
        gameObject.transform.position = position;
    }

    public void UpdatePosition(Vector2 newPos)
    {
        position = newPos;
        gameObject.transform.position = newPos;
    }

    public void ModifyVelocity(Vector2 newVel)
    {
        velocity += newVel;
    }

    public void UpdateVelocity(float newVelX,float newVelY)
    {
        velocity = new Vector2(newVelX,newVelY);
    }

    public void UpdateDensity(float newDensity)
    {
        density = newDensity;
    }

    public void UpdateNearDensity(float newnearDensity)
    {
        nearDensity = newnearDensity;
    }

    public void MoveParticle(Vector2 velocity)
    {
        position += velocity;
        gameObject.transform.position += new Vector3(velocity.x, velocity.y);
    }

}
