using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Actions.Events;

[Serializable, NetSerializable]
public sealed partial class FabricateActionEvent : InstantActionEvent
{
    [DataField(required: true)]
    public EntProtoId Fabrication;
}
