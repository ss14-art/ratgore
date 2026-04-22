using Robust.Shared.Prototypes;

namespace Content.Shared.Cargo.Prototypes;

[Prototype("cargoMarket")]
public sealed partial class CargoMarketPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name { get; private set; } = string.Empty;
}
