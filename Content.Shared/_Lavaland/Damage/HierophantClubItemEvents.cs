using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.Damage;

[Serializable, NetSerializable]
public sealed partial class HierophantClubActivateCrossEvent : WorldTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class HierophantClubPlaceMarkerEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class HierophantClubTeleportToMarkerEvent : InstantActionEvent;
