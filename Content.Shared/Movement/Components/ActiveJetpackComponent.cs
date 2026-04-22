using Robust.Shared.GameStates;
using Robust.Shared.Map;


namespace Content.Shared.Movement.Components;

/// <summary>
/// Added to an enabled jetpack. Tracks gas usage on server / effect spawning on client.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveJetpackComponent : Component
{
    [DataField("effectCooldown")]
    public float EffectCooldown = 0.3f;

    [DataField("targetTime")]
    public TimeSpan TargetTime = TimeSpan.Zero;

    [DataField("lastCoordinates")]
    public EntityCoordinates LastCoordinates;

    [DataField("maxDistance")]
    public float MaxDistance = 0.2f;
}
