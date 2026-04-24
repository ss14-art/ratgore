using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Shared.Roles;

[Prototype]
public sealed partial class RoleLoadoutPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public List<string> Groups = new();

    [DataField]
    public bool CanCustomizeName;

    [DataField]
    public ProtoId<LocalizedDatasetPrototype> NameDataset;
}
