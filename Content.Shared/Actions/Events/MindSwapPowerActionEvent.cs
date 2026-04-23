using Robust.Shared.Serialization;

namespace Content.Shared.Actions.Events;

[Serializable, NetSerializable]
public sealed partial class MindSwapPowerActionEvent : EntityTargetActionEvent {}

[Serializable, NetSerializable]
public sealed partial class MindSwapPowerReturnActionEvent : InstantActionEvent {}
