using Robust.Shared.Serialization;

namespace Content.Shared.Cargo;

[Serializable, NetSerializable]
public sealed class EconomicConsoleBuiState : BoundUserInterfaceState
{
    public Dictionary<string, ItemPriceData> ItemPrices;
    public Dictionary<string, ShuttlePriceInfoData> ShuttlePrices;

    public EconomicConsoleBuiState(Dictionary<string, ItemPriceData> itemPrices, Dictionary<string, ShuttlePriceInfoData> shuttlePrices)
    {
        ItemPrices = itemPrices;
        ShuttlePrices = shuttlePrices;
    }
}

[Serializable, NetSerializable]
public record struct ItemPriceData(
    string ItemId,
    string LocalizedName,
    double BasePrice,
    double CurrentPrice,
    float Multiplier
);

[Serializable, NetSerializable]
public record struct ShuttlePriceInfoData(
    string Name,
    int BasePrice,
    int CurrentPrice,
    float PriceChange
);
