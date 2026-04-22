using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Robust.Shared.Serialization;

namespace Content.Shared.Body.Part;

[ByRefEvent]
public readonly record struct BodyPartAddedEvent(string Slot, Entity<BodyPartComponent> Part);

[ByRefEvent]
public readonly record struct BodyPartRemovedEvent(string Slot, Entity<BodyPartComponent> Part);

[Serializable, NetSerializable]
public sealed partial class TryChangePartDamageEvent : EntityEventArgs
{
    public NetEntity Entity;
    public DamageSpecifier Damage;
    public NetEntity? Origin;
    public TargetBodyPart? TargetPart;
    public bool IgnoreResistances;
    public bool CanSever;
    public bool CanEvade;
    public float stoppingPower = 0f;
    public float HullrotArmorPen = 0f;
    public float PartMultiplier = 1.0f;
    public bool Evaded;
    public bool Handled;

    public TryChangePartDamageEvent(NetEntity entity, DamageSpecifier damage, NetEntity? origin = null, TargetBodyPart? targetPart = null, bool ignoreResistances = false, bool canSever = false, bool canEvade = false, float partMultiplier = 1.0f, float stoppingPower = 0f, float HullrotArmorPen = 0f)
    {
        Entity = entity;
        Damage = damage;
        Origin = origin;
        TargetPart = targetPart;
        IgnoreResistances = ignoreResistances;
        CanSever = canSever;
        CanEvade = canEvade;
        PartMultiplier = partMultiplier;
        this.stoppingPower = stoppingPower;
        this.HullrotArmorPen = HullrotArmorPen;
    }
}
