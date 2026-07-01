using Content.Client.Cargo.Components;
using Content.Client.Cargo.UI;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Events;
using Robust.Shared.GameObjects;

namespace Content.Client.Cargo.Systems;

public sealed class PriceMonitorSystem : EntitySystem
{
    private PriceMonitorData _monitorData = new();
    private PriceMonitorWindow? _priceWindow;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<PriceUpdateMessage>(OnPriceUpdate);
    }

    private void OnPriceUpdate(PriceUpdateMessage message)
    {
        _monitorData.GoodsPrices.Clear();
        foreach (var (goodId, data) in message.GoodsPrices)
        {
            _monitorData.GoodsPrices[goodId] = new PriceData(
                goodId,
                data.BasePrice,
                data.CurrentPrice,
                data.Multiplier,
                (data.Multiplier - 1.0f) * 100f
            );
        }

        _monitorData.ShuttlePrices.Clear();
        foreach (var shuttleData in message.ShuttlePrices)
        {
            _monitorData.ShuttlePrices[shuttleData.ShuttleName] = new ShuttlePriceData(
                shuttleData.ShuttleName,
                shuttleData.BasePrice,
                shuttleData.CurrentPrice,
                shuttleData.PercentChange
            );
        }

        _priceWindow?.UpdatePrices(_monitorData);
    }

    public void RequestPrices()
    {
        RaiseNetworkEvent(new RequestPriceUpdateMessage());
    }

    public void OpenPriceMonitor()
    {
        if (_priceWindow == null || !_priceWindow.IsOpen)
        {
            _priceWindow = new PriceMonitorWindow(this);
            _priceWindow.Open();
            RequestPrices();
        }
    }

    public void ClosePriceMonitor()
    {
        _priceWindow?.Close();
    }
}
