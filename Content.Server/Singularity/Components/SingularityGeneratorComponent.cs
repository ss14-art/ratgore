using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

using Content.Server.Singularity.EntitySystems;

namespace Content.Server.Singularity.Components;

[RegisterComponent]
public sealed partial class SingularityGeneratorComponent : Component
{
    /// <summary>
    /// The amount of power this generator has accumulated.
    /// If you want to set this use <see  cref="SingularityGeneratorSystem.SetPower"/>
    /// </summary>
    [DataField("power")]
    [Access(friends:typeof(SingularityGeneratorSystem))]
    public float Power = 0;

    /// <summary>
    /// The power threshold at which this generator will spawn a singularity.
    /// If you want to set this use <see  cref="SingularityGeneratorSystem.SetThreshold"/>
    /// </summary>
    [DataField("threshold")]
    [Access(friends:typeof(SingularityGeneratorSystem))]
    public float Threshold = 16;

    /// <summary>
    ///     The prototype ID used to spawn a singularity.
    /// </summary>
    [DataField("spawnId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? SpawnPrototype = "Singularity";

    /// <summary>
    ///     Whether or not the failsafe is disabled.
    /// </summary>
    [DataField("failsafeDisabled")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool FailsafeDisabled = false;

    /// <summary>
    ///     The time at which the failsafe will next be able to trigger.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextFailsafe = TimeSpan.Zero;

    /// <summary>
    ///     The cooldown between failsafe triggers.
    /// </summary>
    [DataField("failsafeCooldown")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan FailsafeCooldown = TimeSpan.FromSeconds(10);

    /// <summary>
    ///     The distance at which the failsafe will trigger.
    /// </summary>
    [DataField("failsafeDistance")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float FailsafeDistance = 10f;

    /// <summary>
    ///     The collision mask used to check for containment fields.
    /// </summary>
    [DataField("collisionMask")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int CollisionMask = 0;
}
