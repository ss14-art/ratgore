using Robust.Shared.Serialization;

namespace Content.Shared._Rat.Factions;

[Serializable, NetSerializable]
public sealed class FactionListMessage : EntityEventArgs
{
    public List<RatFactionInfo> Factions { get; set; } = new();
}

[Serializable, NetSerializable]
public sealed class RequestFactionListMessage : EntityEventArgs
{
}
