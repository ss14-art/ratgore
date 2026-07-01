using Content.Server.Bank;
using Content.Server.Cargo.Systems;
using Content.Server.CartridgeLoader;
using Content.Shared.Cargo.Cartridges;
using Content.Shared.Cargo.Components;
using Content.Shared.CartridgeLoader;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;

namespace Content.Server.Cargo.Cartridges;

public sealed class StockMarketCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly StockCompanySystem _stockCompanies = default!;
    [Dependency] private readonly BankSystem _bank = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StockMarketCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<StockMarketCartridgeComponent, CartridgeMessageEvent>(OnMessage);
    }

    private void OnUiReady(EntityUid uid, StockMarketCartridgeComponent component, CartridgeUiReadyEvent args)
    {
        var playerUid = GetLoaderMob(args.Loader);
        SendUiState(uid, args.Loader, playerUid);
    }

    private void OnMessage(EntityUid uid, StockMarketCartridgeComponent component, CartridgeMessageEvent args)
    {
        if (args is not StockMarketUiMessageEvent msg)
            return;

        HandleUiMessage(uid, args, msg);
    }

    private void HandleUiMessage(EntityUid uid, CartridgeMessageEvent args, StockMarketUiMessageEvent msg)
    {
        var playerUid = args.Actor;
        var loaderUid = GetEntity(args.LoaderUid);

        if (playerUid == default)
            return;

        switch (msg.Action)
        {
            case StockMarketUiAction.RequestPrices:
                SendUiState(uid, loaderUid, playerUid);
                break;

            case StockMarketUiAction.Buy:
                TryBuyStock(playerUid, msg.CompanyId ?? "", msg.Amount);
                SendUiState(uid, loaderUid, playerUid);
                break;

            case StockMarketUiAction.Sell:
                TrySellStock(playerUid, msg.CompanyId ?? "", msg.Amount);
                SendUiState(uid, loaderUid, playerUid);
                break;
        }
    }

    private const int MaxStockAmount = 1000;

    private bool TryBuyStock(EntityUid playerUid, string companyId, int amount)
    {
        if (amount <= 0 || amount > MaxStockAmount)
            return false;

        var company = _stockCompanies.GetCompany(companyId);
        if (company == null)
            return false;

        var cost = (int)Math.Round(company.CurrentPrice * amount);

        if (!_bank.TryBankWithdraw(playerUid, cost))
            return false;

        var portfolio = EnsureComp<PlayerStockPortfolioComponent>(playerUid);
        if (!portfolio.OwnedShares.TryGetValue(companyId, out var current))
            current = 0;
        portfolio.OwnedShares[companyId] = current + amount;
        portfolio.TotalInvested += cost;
        Dirty(playerUid, portfolio);
        return true;
    }

    private bool TrySellStock(EntityUid playerUid, string companyId, int amount)
    {
        if (amount <= 0 || amount > MaxStockAmount)
            return false;

        if (!TryComp<PlayerStockPortfolioComponent>(playerUid, out var portfolio))
            return false;

        if (!portfolio.OwnedShares.TryGetValue(companyId, out var owned) || owned < amount)
            return false;

        var company = _stockCompanies.GetCompany(companyId);
        if (company == null)
            return false;

        var profit = (int)Math.Round(company.CurrentPrice * amount);
        if (!_bank.TryBankDeposit(playerUid, profit))
            return false;

        var newOwned = owned - amount;
        if (newOwned <= 0)
            portfolio.OwnedShares.Remove(companyId);
        else
            portfolio.OwnedShares[companyId] = newOwned;
        Dirty(playerUid, portfolio);
        return true;
    }

    private EntityUid? GetLoaderMob(EntityUid loaderUid)
    {
        var uid = loaderUid;
        for (var i = 0; i < 8; i++)
        {
            var parent = Transform(uid).ParentUid;
            if (!parent.IsValid())
                break;
            if (_playerManager.TryGetSessionByEntity(parent, out _))
                return parent;
            uid = parent;
        }
        return null;
    }

    private void SendUiState(EntityUid cartridgeUid, EntityUid loaderUid, EntityUid? playerUid)
    {
        var priceData = new Dictionary<string, StockPriceData>();
        var portfolio = new Dictionary<string, int>();

        var companies = _stockCompanies.GetCompanies();
        foreach (var company in companies)
        {
            priceData[company.Id] = new StockPriceData(
                company.Id,
                company.BasePrice,
                company.CurrentPrice,
                company.PriceMultiplier,
                company.PriceChange
            );
        }

        if (playerUid != null && TryComp<PlayerStockPortfolioComponent>(playerUid.Value, out var portfolioComp))
            portfolio = new Dictionary<string, int>(portfolioComp.OwnedShares);

        var state = new StockMarketUiState(priceData, portfolio);
        _cartridgeLoader.UpdateCartridgeUiState(loaderUid, state);
    }
}
