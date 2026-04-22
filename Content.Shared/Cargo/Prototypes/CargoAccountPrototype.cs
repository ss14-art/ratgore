using Robust.Shared.Prototypes;

namespace Content.Shared.Cargo.Prototypes;

[Prototype("cargoAccount")]
public sealed partial class CargoAccountPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name { get; private set; } = string.Empty;

    [DataField]
    public int DefaultPoints = 0;
}
