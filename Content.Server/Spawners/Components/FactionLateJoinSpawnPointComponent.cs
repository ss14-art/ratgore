using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.Spawners.Components;

[RegisterComponent]
public sealed partial class FactionLateJoinSpawnPointComponent : Component
{
    [DataField("faction_id", required: true)]
    public ProtoId<FactionPrototype> Faction;
}
