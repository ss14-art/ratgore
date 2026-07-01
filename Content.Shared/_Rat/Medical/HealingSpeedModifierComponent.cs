using Robust.Shared.GameStates;

namespace Content.Shared._Rat.Medical;

[RegisterComponent, NetworkedComponent]
public sealed partial class HealingSpeedModifierComponent : Component
{
    [DataField]
    public float SpeedModifier = 1.5f;
}
