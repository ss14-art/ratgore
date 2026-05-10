using Content.Shared.Clothing.Loadouts.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Roles;

[Serializable, NetSerializable]
public sealed class RoleLoadout
{
    public string? EntityName;
    public Dictionary<string, List<SelectedLoadout>> SelectedLoadouts = new();

    public RoleLoadout(string? entityName = null)
    {
        EntityName = entityName;
    }
}

[Serializable, NetSerializable]
public sealed class SelectedLoadout
{
    public ProtoId<LoadoutPrototype> Prototype;

    public SelectedLoadout(ProtoId<LoadoutPrototype> prototype)
    {
        Prototype = prototype;
    }
}
