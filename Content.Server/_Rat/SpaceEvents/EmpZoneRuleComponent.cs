using System.Numerics;
using Content.Server._Rat.SpaceEvents;

namespace Content.Server._Rat.SpaceEvents.Components;

[RegisterComponent, Access(typeof(EmpZoneRule))]
public sealed partial class EmpZoneRuleComponent : Component
{
    [DataField]
    public Vector2 ZoneCenter = Vector2.Zero;

    [DataField]
    public float ZoneRadius = 500f;

    [DataField]
    public TimeSpan NextEmpTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan EmpInterval = TimeSpan.FromMinutes(0.1);

    [DataField]
    public bool ZoneActive = false;

    [DataField]
    public TimeSpan ZoneAppearTime = TimeSpan.Zero;
  
    [DataField]
    public TimeSpan ZoneAppearDelay = TimeSpan.FromMinutes(5);

    [DataField]
    public float EmpRange = 20f;

    [DataField]
    public float EmpEnergyConsumption = 100000f;

    [DataField]
    public float EmpDisableDuration = 10f;

    [DataField]
    public float MinX = -2000f;

    [DataField]
    public float MinY = -2000f;

    [DataField]
    public float MaxX = 2000f;

    [DataField]
    public float MaxY = 2000f;
}