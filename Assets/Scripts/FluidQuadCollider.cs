using UnityEngine;

public class FluidQuadCollider : MonoBehaviour, IFluidCollider
{
    public Vector2 center { get; private set; }
    public Vector2 size { get; private set; }

    public FluidQuadCollider(Vector2 center, Vector2 size)
    {
        this.center = center;
        this.size = size;
    }

    public ColliderType Type => ColliderType.QUAD;

    private void Update()
    {
        center = new Vector2(transform.position.x, transform.position.y);
        size = new Vector2(transform.localScale.x, transform.localScale.y);
    }

    public void ResolveCollision(ref FluidParticle particle, float particleRadius, float collisionDamping)
    {
        Vector2 particlePosition = particle.position;
        Vector2 particleVelocity = particle.velocity;

        Vector2 halfSize = size / 2;
        Vector2 minSize = center - halfSize;
        Vector2 maxSize = center + halfSize;

        // Check if the particle is inside the square
        if (particlePosition.x > minSize.x - particleRadius && particlePosition.x < maxSize.x + particleRadius &&
            particlePosition.y > minSize.y - particleRadius && particlePosition.y < maxSize.y + particleRadius)
        {
            Vector2 newVelocity = particleVelocity;
            Vector2 newPosition = particlePosition;

            Vector4 outDirs = Vector4.zero;
            outDirs.x = particlePosition.x - minSize.x; // LEFT
            outDirs.y = maxSize.x - particlePosition.x; // RIGHT
            outDirs.z = particlePosition.y - minSize.y; // BOTTOM
            outDirs.w = maxSize.y - particlePosition.y; // UP

            float outDist = Mathf.Min(outDirs.x, outDirs.y, outDirs.z, outDirs.w);

            if (outDist == outDirs.x)
            {
                newPosition.x = minSize.x - particleRadius;
                newVelocity.x = Mathf.Abs(particleVelocity.x) * collisionDamping;
            }
            else if (outDist == outDirs.y)
            {
                newPosition.x = maxSize.x + particleRadius;
                newVelocity.x = -Mathf.Abs(particleVelocity.x) * collisionDamping;
            }
            else if (outDist == outDirs.z)
            {
                newPosition.y = minSize.y - particleRadius;
                newVelocity.y = Mathf.Abs(particleVelocity.y) * collisionDamping;
            }
            else if (outDist == outDirs.w)
            {
                newPosition.y = maxSize.y + particleRadius;
                newVelocity.y = -Mathf.Abs(particleVelocity.y) * collisionDamping;
            }

            particle.UpdateVelocity(newVelocity.x, newVelocity.y);
            particle.UpdatePosition(newPosition);
        }
    }
}
