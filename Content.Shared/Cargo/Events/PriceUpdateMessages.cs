using Robust.Shared.Serialization;

namespace Content.Shared.Cargo.Events;

[Serializable, NetSerializable]
public sealed class PriceUpdateMessage : EntityEventArgs
{
    public Dictionary<string, GoodsPriceEntry> GoodsPrices = new();
    public List<ShuttlePriceEntry> ShuttlePrices = new();
}

[Serializable, NetSerializable]
public record struct GoodsPriceEntry(
    string GoodId,
    double BasePrice,
    double CurrentPrice,
    float Multiplier
);

[Serializable, NetSerializable]
public record struct ShuttlePriceEntry(
    string ShuttleName,
    int BasePrice,
    int CurrentPrice,
    float PercentChange
);

[Serializable, NetSerializable]
public sealed class RequestPriceUpdateMessage : EntityEventArgs
{
}
