using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Shared.Interaction;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Prototypes;

namespace Content.Server.Cargo.Systems;

public sealed class EconomicConsoleSystem : SharedEconomicConsoleSystem
{
    [Dependency] private readonly DynamicPricingSystem _dynamicPricing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EconomicConsoleComponent, ActivateInWorldEvent>(OnActivate);
    }

    private void OnActivate(Entity<EconomicConsoleComponent> ent, ref ActivateInWorldEvent args)
    {
        args.Handled = true;
        
        var user = args.User;

        _uiSystem.OpenUi(ent.Owner, EconomicConsoleUiKey.Key, user);
        SendBuiState(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<EconomicConsoleComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.AutoUpdateEnabled)
                continue;

            component.LastUpdateTimer += frameTime;
            
            if (component.LastUpdateTimer >= component.RefreshInterval)
            {
                component.LastUpdateTimer = 0f;
                
                if (_uiSystem.TryGetOpenUi(uid, EconomicConsoleUiKey.Key, out _))
                {
                    SendBuiState(uid);
                }
            }
        }
    }

    private string GetLocalizedName(string prototypeId)
    {
        if (_prototype.TryIndex<EntityPrototype>(prototypeId, out var proto))
        {
            var key = proto.Name;
            if (!string.IsNullOrEmpty(key))
                return Loc.GetString(key);
        }
        return prototypeId;
    }

    private void SendBuiState(EntityUid consoleUid)
    {
        var prices = _dynamicPricing.GetAllPricesFull();
        var itemPrices = new Dictionary<string, ItemPriceData>();

        foreach (var (itemId, (basePrice, multiplier, currentPrice)) in prices)
        {
            var localizedName = GetLocalizedName(itemId);
            itemPrices[itemId] = new ItemPriceData(itemId, localizedName, basePrice, currentPrice, multiplier);
        }

        var shuttlePricesRaw = _dynamicPricing.GetAllShuttlePrices();
        var shuttlePrices = new Dictionary<string, ShuttlePriceInfoData>();

        foreach (var (name, info) in shuttlePricesRaw)
        {
            var priceChange = info.BasePrice > 0 
                ? (float)(info.CurrentPrice - info.BasePrice) / info.BasePrice 
                : 0f;
            shuttlePrices[name] = new ShuttlePriceInfoData(name, info.BasePrice, info.CurrentPrice, priceChange);
        }

        var state = new EconomicConsoleBuiState(itemPrices, shuttlePrices);
        _uiSystem.SetUiState(consoleUid, EconomicConsoleUiKey.Key, state);
    }
}
