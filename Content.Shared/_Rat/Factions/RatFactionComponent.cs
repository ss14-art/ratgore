using Robust.Shared.GameStates;

namespace Content.Shared._Rat.Factions;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RatFactionComponent : Component
{
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public string? SubfactionName;
}
