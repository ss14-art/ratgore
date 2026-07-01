using Robust.Shared.GameStates;

namespace Content.Shared._Rat.CrewMonitoring;

/// <summary>
/// Added to an entity by a faction tracker implant.
/// The crew monitoring console queries for this component to find trackable entities.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class FactionTrackerComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Faction = "";
}
