using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Chemistry.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TankLeakComponent : Component
{
    [DataField, AutoNetworkedField]
    public int LeakCount;

    [DataField]
    public FixedPoint2 LeakRatePerHole = FixedPoint2.New(5);

    [DataField]
    public int MaxLeaks = 5;

    [DataField]
    public float LeakAccumulator;

    [DataField]
    public string Solution = "tank";
}
