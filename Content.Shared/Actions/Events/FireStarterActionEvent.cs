using Robust.Shared.Serialization;

namespace Content.Shared.Actions.Events;

[Serializable, NetSerializable]
public sealed partial class FireStarterActionEvent : InstantActionEvent
{
    /// <summary>
    /// Increases the number of fire stacks when a flammable object is ignited.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float Severity = 0.3f;
}
