using Robust.Shared.GameStates;

namespace Content.Shared._Rat.CrewMonitoring;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class RatCrewMonitorComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? Faction;
}
