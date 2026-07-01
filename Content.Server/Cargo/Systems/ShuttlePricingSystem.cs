using Content.Server.Cargo.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Cargo.Systems;

public sealed class ShuttlePricingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ShuttlePricingComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (curTime < component.NextUpdate)
                continue;

            component.NextUpdate = curTime + component.UpdateInterval;
            UpdateShuttlePrice(uid, component);
        }
    }

    private void UpdateShuttlePrice(EntityUid uid, ShuttlePricingComponent component)
    {
        component.PriceHistory.Add(component.CurrentPrice);
        if (component.PriceHistory.Count > 5)
            component.PriceHistory.RemoveAt(0);

        var trend = CalculateTrend(component.PriceHistory);
        var randomChange = _random.NextFloat(-0.15f, 0.15f);
        
        var totalChange = trend * 0.3f + randomChange * 0.7f;
        
        var newPrice = (int)(component.CurrentPrice * (1 + totalChange));
        
        component.CurrentPrice = Math.Clamp(newPrice, component.MinPrice, component.MaxPrice);

        if (component.PriceHistory.Count >= 2)
        {
            var percentChange = ((component.CurrentPrice - component.PriceHistory[^2]) / (float)component.PriceHistory[^2]) * 100f;
            if (Math.Abs(percentChange) > 5f)
            {
                Log.Info($"[ShuttlePricing] {Loc.GetString("rat-economy-log-shuttle-price-change", ("shuttleUid", uid), ("percentChange", percentChange), ("currentPrice", component.CurrentPrice))}");
            }
        }
    }

    private float CalculateTrend(List<int> priceHistory)
    {
        if (priceHistory.Count < 2)
            return 0f;

        var recent = priceHistory[^1];
        var previous = priceHistory[^2];
        
        if (previous == 0)
            return 0f;

        return (recent - previous) / (float)previous;
    }

    public int GetShuttlePrice(EntityUid uid)
    {
        if (TryComp<ShuttlePricingComponent>(uid, out var component))
            return component.CurrentPrice;
        
        return 0;
    }

    public bool SetShuttlePrice(EntityUid uid, int newPrice)
    {
        if (!TryComp<ShuttlePricingComponent>(uid, out var component))
            return false;

        component.CurrentPrice = Math.Clamp(newPrice, component.MinPrice, component.MaxPrice);
        
        component.PriceHistory.Add(component.CurrentPrice);
        if (component.PriceHistory.Count > 5)
            component.PriceHistory.RemoveAt(0);

        return true;
    }

    public void ResetShuttlePrice(EntityUid uid)
    {
        if (!TryComp<ShuttlePricingComponent>(uid, out var component))
            return;

        component.CurrentPrice = component.BasePrice;
        component.PriceHistory.Clear();
    }
}
