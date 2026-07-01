using Content.Shared.Cargo.Cartridges;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Cargo.Systems;

public sealed class StockCompanySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly List<StockCompanyData> _companies = new();
    private readonly Dictionary<NetUserId, Dictionary<string, int>> _playerStocks = new();

    private TimeSpan _lastUpdate;
    private const float UpdateInterval = 600f;
    private bool _initialized;

    public override void Initialize()
    {
        base.Initialize();
        _lastUpdate = _timing.CurTime;
    }

    private void InitializeCompanies()
    {
        if (_initialized)
            return;
        _initialized = true;

        _companies.Clear();
        _companies.Add(new StockCompanyData("stock-company-tpsh", 500f));
        _companies.Add(new StockCompanyData("stock-company-kvst", 400f));
        _companies.Add(new StockCompanyData("stock-company-bms", 350f));
        _companies.Add(new StockCompanyData("stock-company-lrnp", 250f));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_initialized)
            InitializeCompanies();

        var curTime = _timing.CurTime;
        if ((curTime - _lastUpdate).TotalSeconds < UpdateInterval)
            return;

        _lastUpdate = curTime;

        foreach (var company in _companies)
        {
            float change;
            if (_random.Prob(0.15f))
                change = _random.NextFloat(-0.15f, 0.15f);
            else
                change = _random.NextFloat(-0.03f, 0.03f);

            company.PriceMultiplier += change;
            company.PriceMultiplier = Math.Clamp(company.PriceMultiplier, 0.1f, 1.9f);
            company.PriceMultiplier += (1.0f - company.PriceMultiplier) * 0.005f;
            company.CurrentPrice = company.BasePrice * company.PriceMultiplier;
            company.PriceChange = company.PriceMultiplier - 1.0f;
        }
    }

    public List<StockCompanyData> GetCompanies()
    {
        if (!_initialized)
            InitializeCompanies();
        return _companies;
    }

    public StockCompanyData? GetCompany(string id)
    {
        if (!_initialized)
            InitializeCompanies();
        return _companies.Find(c => c.Id == id);
    }

    public Dictionary<string, int> GetPlayerStocks(NetUserId playerId)
    {
        if (_playerStocks.TryGetValue(playerId, out var stocks))
            return new Dictionary<string, int>(stocks);
        return new Dictionary<string, int>();
    }

    public void AddPlayerStock(NetUserId playerId, string companyId, int amount)
    {
        if (amount <= 0)
            return;

        if (!_playerStocks.TryGetValue(playerId, out var stocks))
        {
            stocks = new Dictionary<string, int>();
            _playerStocks[playerId] = stocks;
        }

        if (!stocks.TryGetValue(companyId, out var current))
            current = 0;

        stocks[companyId] = current + amount;
    }

    public bool TryRemovePlayerStock(NetUserId playerId, string companyId, int amount)
    {
        if (amount <= 0)
            return false;

        if (!_playerStocks.TryGetValue(playerId, out var stocks))
            return false;

        if (!stocks.TryGetValue(companyId, out var current) || current < amount)
            return false;

        stocks[companyId] = current - amount;
        if (stocks[companyId] <= 0)
            stocks.Remove(companyId);

        return true;
    }

    public double GetPortfolioValue(NetUserId playerId)
    {
        if (!_playerStocks.TryGetValue(playerId, out var stocks))
            return 0;

        double total = 0;
        foreach (var (companyId, shares) in stocks)
        {
            var company = GetCompany(companyId);
            if (company != null)
                total += shares * company.CurrentPrice;
        }
        return total;
    }
}

public sealed class StockCompanyData
{
    public string Id;
    public float BasePrice;
    public float CurrentPrice;
    public float PriceMultiplier = 1.0f;
    public float PriceChange;

    public StockCompanyData(string id, float basePrice)
    {
        Id = id;
        BasePrice = basePrice;
        CurrentPrice = basePrice;
    }
}
