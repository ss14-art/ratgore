using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.Cargo.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PlayerStockPortfolioComponent : Component
{
    [ViewVariables]
    [DataField]
    public Dictionary<string, int> OwnedShares = new();

    [ViewVariables]
    [DataField]
    public double TotalInvested;

    [ViewVariables]
    [DataField, AutoNetworkedField]
    public double CurrentValue;
}
