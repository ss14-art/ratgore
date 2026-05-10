using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Cargo.Components;

[RegisterComponent]
public sealed partial class CargoSlipComponent : Component
{
    [DataField]
    public ProtoId<CargoProductPrototype>? Product;

    [DataField]
    public string Requester = string.Empty;

    [DataField]
    public string Reason = string.Empty;

    [DataField]
    public int OrderQuantity;

    [DataField]
    public ProtoId<CargoAccountPrototype>? Account;
}
