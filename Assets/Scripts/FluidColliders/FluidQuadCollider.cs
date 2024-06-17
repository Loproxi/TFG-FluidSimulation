using UnityEngine;

public class FluidQuadCollider : MonoBehaviour, IFluidCollider
{
    public Vector2 center { get; private set; }
    public Vector2 size { get; private set; }
    public Vector3 rotation { get; private set; }

    public FluidQuadCollider(Vector2 center, Vector2 size, Vector3 rotation)
    {
        this.center = center;
        this.size = size;
        this.rotation = rotation;
    }

    public ColliderType Type => ColliderType.QUAD;

    private void Start()
    {
        center = new Vector2(transform.position.x, transform.position.y);
        size = new Vector2(transform.localScale.x, transform.localScale.y);
        rotation = transform.rotation.eulerAngles;
    }
    private void Update()
    {
        center = new Vector2(transform.position.x, transform.position.y);
        size = new Vector2(transform.localScale.x, transform.localScale.y);
        rotation = transform.rotation.eulerAngles;
    }

    public void ResolveCollision(ref FluidParticle particle, float particleRadius, float collisionDamping)
    {
        Quaternion rotationQuat = Quaternion.Euler(rotation);
        Quaternion invRotationQuat = Quaternion.Euler(-rotation);

        // Since the axis in local space are aligned with the quad we pass the particles to quad local space
        // Apply the inv of the rotation to the particles so they are in the same local space
        Vector2 localParticlePosition = invRotationQuat * (particle.position - center);
        Vector2 localParticleVelocity = invRotationQuat * particle.velocity;

        Vector2 halfSize = size / 2;
        Vector2 minSizeLocal = -halfSize;
        Vector2 maxSizeLocal = halfSize;

        // Check if the particle is inside the rotated quad in local space
        if (localParticlePosition.x > minSizeLocal.x - particleRadius && localParticlePosition.x < maxSizeLocal.x + particleRadius &&
            localParticlePosition.y > minSizeLocal.y - particleRadius && localParticlePosition.y < maxSizeLocal.y + particleRadius)
        {
            Vector2 newVelocity = localParticleVelocity;
            Vector2 newPosition = localParticlePosition;

            Vector4 outDirs = Vector4.zero;
            outDirs.x = localParticlePosition.x - minSizeLocal.x; // LEFT
            outDirs.y = maxSizeLocal.x - localParticlePosition.x; // RIGHT
            outDirs.z = localParticlePosition.y - minSizeLocal.y; // BOTTOM
            outDirs.w = maxSizeLocal.y - localParticlePosition.y; // TOP

            MoveParticleOutOfQuad(particleRadius, collisionDamping, localParticleVelocity, ref newVelocity, ref newPosition, outDirs);

            //Apply the rotation and Convert it to World Space
            Vector3 newVelRotated = rotationQuat * newVelocity;
            Vector3 newPosRotated = rotationQuat * newPosition;
            particle.UpdateVelocity(newVelRotated.x, newVelRotated.y);
            particle.UpdatePosition(center + new Vector2(newPosRotated.x, newPosRotated.y));
        }
    }

    private void MoveParticleOutOfQuad(float particleRadius, float collisionDamping, Vector2 localParticleVelocity, ref Vector2 newVelocity, ref Vector2 newPosition, Vector4 outDirs)
    {
        Vector2 halfSize = size / 2;
        Vector2 minSize = -halfSize;
        Vector2 maxSize = halfSize;

        //Get which side of the quad is the one closer from the particle in order to eject it out of it

        float outDist = Mathf.Min(outDirs.x, outDirs.y, outDirs.z, outDirs.w);

        if (outDist == outDirs.x)
        {
            newPosition.x = minSize.x - particleRadius;
            newVelocity.x = Mathf.Abs(localParticleVelocity.x) * collisionDamping;
        }
        else if (outDist == outDirs.y)
        {
            newPosition.x = maxSize.x + particleRadius;
            newVelocity.x = -Mathf.Abs(localParticleVelocity.x) * collisionDamping;
        }
        else if (outDist == outDirs.z)
        {
            newPosition.y = minSize.y - particleRadius;
            newVelocity.y = Mathf.Abs(localParticleVelocity.y) * collisionDamping;
        }
        else if (outDist == outDirs.w)
        {
            newPosition.y = maxSize.y + particleRadius;
            newVelocity.y = -Mathf.Abs(localParticleVelocity.y) * collisionDamping;
        }
    }

    public FluidColliderData GetColliderData()
    {
        FluidColliderData toReturn = new FluidColliderData();

        toReturn.size = size;
        toReturn.type = (int)Type;
        toReturn.center = center;
        toReturn.rotation = rotation;

        return toReturn;
    }

}
