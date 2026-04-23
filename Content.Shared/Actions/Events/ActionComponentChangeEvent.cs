using System;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Actions.Events;

/// <summary>
/// Adds / removes the component upon action.
/// </summary>
[Virtual]
[Serializable, NetSerializable]
public partial class ActionComponentChangeEvent : InstantActionEvent
{
    [DataField(required: true)]
    [NonSerialized]
    public ComponentRegistry Components = new();
}

/// <summary>
/// Similar to <see cref="ActionComponentChangeEvent"/> except raises an event to attempt to relay it.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class RelayedActionComponentChangeEvent : ActionComponentChangeEvent
{

}

[ByRefEvent]
public record struct AttemptRelayActionComponentChangeEvent
{
    public EntityUid? Target;
}
