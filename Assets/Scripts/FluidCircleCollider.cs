using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;


public class FluidCircleCollider : MonoBehaviour, IFluidCollider
{
    public Vector2 center { get; private set; }
    public float radius { get; private set; }

    public FluidCircleCollider(Vector2 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }
    public void UpdateCenter(Vector2 newCenter)
    {
        center = newCenter;
    }

    public void UpdateRadius(float radius)
    {
        this.radius = radius;
    }

    public ColliderType Type => ColliderType.CIRCLE;

    private void Update()
    {
        center = new Vector2(transform.position.x, transform.position.y);
        radius = transform.localScale.x/2;
    }

    public void ResolveCollision(ref FluidParticle particle, float particleRadius,float collisionDamping)
    {

        Vector2 particlePosition = particle.position;
        Vector2 particleVelocity = particle.velocity;

        Vector2 dir = particlePosition - center;
        float distance = dir.magnitude;
        Vector2 totalDisplacement = Vector2.zero;

        if (distance < radius)
        {
            dir.Normalize();

            Vector2 newPosition = center + dir * (radius + particleRadius);

            Vector2 newVelocity = particleVelocity - 2 * Vector2.Dot(particleVelocity, dir) * dir * collisionDamping;

            particle.UpdatePosition(newPosition);
            particle.UpdateVelocity(newVelocity.x, newVelocity.y);
        }
        else if (distance < radius + particleRadius)
        {
            dir.Normalize();
            Vector2 newPosition = center + dir * (radius + particleRadius);
            Vector2 newVelocity = particleVelocity - 2 * Vector2.Dot(particleVelocity, dir) * dir * collisionDamping;

            particle.UpdatePosition(newPosition);
            particle.UpdateVelocity(newVelocity.x, newVelocity.y);
        }

    }
    public FluidColliderData GetColliderData()
    {
        FluidColliderData toReturn = new FluidColliderData();

        toReturn.radius = radius;
        toReturn.type = (int)Type;
        toReturn.center = center;

        return toReturn;
    }
}
