using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.PointCannons;

[RegisterComponent]
public sealed partial class CannonFireCooldownComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextFire = TimeSpan.Zero;

    [DataField]
    public float FireCooldown = 0.15f;
}
