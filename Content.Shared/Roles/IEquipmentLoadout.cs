using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared.Roles;

public interface IEquipmentLoadout
{
    string GetGear(string slot);
    List<EntProtoId> Inhand { get; }
    Dictionary<string, List<EntProtoId>> Storage { get; }
}
