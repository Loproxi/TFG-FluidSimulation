using UnityEngine;

public enum ColliderType
{
    CIRCLE,
    QUAD,
    OTHER
}

public interface IFluidCollider
{
    ColliderType Type { get; }
    void ResolveCollision(ref FluidParticle particle, float particleRadius, float collisionDamping);
}
