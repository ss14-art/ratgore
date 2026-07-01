using Content.Shared.CartridgeLoader;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Cargo.Cartridges;

[Serializable, NetSerializable]
public sealed class StockMarketRequestPricesMsg : CartridgeMessageEvent
{
}

[Serializable, NetSerializable]
public sealed class StockMarketPricesResponseMsg : EntityEventArgs
{
    public Dictionary<string, StockPriceData> Prices { get; init; } = new();
    public Dictionary<string, int> Portfolio { get; init; } = new();
}

[Serializable, NetSerializable]
public sealed class StockMarketBuyMsg : CartridgeMessageEvent
{
    public string CompanyId { get; init; } = string.Empty;
    public int Amount { get; init; } = 1;
}

[Serializable, NetSerializable]
public sealed class StockMarketSellMsg : CartridgeMessageEvent
{
    public string CompanyId { get; init; } = string.Empty;
    public int Amount { get; init; } = 1;
}

[Serializable, NetSerializable]
public sealed class StockMarketTransactionMsg : EntityEventArgs
{
    public bool Success { get; init; }
    public string CompanyId { get; init; } = string.Empty;
    public int Amount { get; init; }
    public double TotalCost { get; init; }
    public bool IsBuy { get; init; }
}

[Serializable, NetSerializable]
public sealed class StockMarketErrorMsg : EntityEventArgs
{
    public string Message { get; init; } = string.Empty;
}

[Serializable, NetSerializable]
public record struct StockPriceData(
    string CompanyId,
    double BasePrice,
    double CurrentPrice,
    float Multiplier,
    float PriceChange = 0f
);
