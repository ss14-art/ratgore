using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Buckle.Events;

[Serializable, NetSerializable]
public sealed partial class BuckleDoAfterEvent : DoAfterEvent
{
    [DataField]
    public NetEntity? Target;

    [DataField]
    public NetEntity? Used;

    public BuckleDoAfterEvent()
    {
    }

    public BuckleDoAfterEvent(NetEntity? target, NetEntity? used)
    {
        Target = target;
        Used = used;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class UnbuckleAlertEvent : EntityEventArgs
{
    public bool Handled;
}
