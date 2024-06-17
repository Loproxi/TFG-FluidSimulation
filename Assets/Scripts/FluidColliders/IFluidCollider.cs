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
    FluidColliderData GetColliderData();
}
