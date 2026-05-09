using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Shared._Rat.SpaceEvents;

[Serializable, NetSerializable]
public sealed class EmpZoneActivatedEvent : EntityEventArgs
{
    public Vector2 Center;
    public float Radius;

    public EmpZoneActivatedEvent(Vector2 center, float radius)
    {
        Center = center;
        Radius = radius;
    }
}

[Serializable, NetSerializable]
public sealed class EmpZoneDeactivatedEvent : EntityEventArgs
{
}