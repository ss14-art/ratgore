using Robust.Shared.Serialization;

namespace Content.Shared.Actions.Events;

[Serializable, NetSerializable]
public sealed partial class DarkSwapActionEvent : InstantActionEvent
{
    [DataField]
    public bool CheckInsulation;
}
