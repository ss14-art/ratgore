using System.Linq;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Cargo.UI;

public sealed partial class StockMarketUiFragment : BoxContainer
{
    private readonly Label _statusLabel;
    private readonly Label _portfolioLabel;
    private readonly ScrollContainer _stockScroll;
    private readonly BoxContainer _stockList;

    public Action<string, int>? OnBuyPressed;
    public Action<string, int>? OnSellPressed;

    public StockMarketUiFragment()
    {
        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;
        VerticalExpand = true;
        Margin = new Thickness(4);

        var titleLabel = new Label
        {
            Text = Loc.GetString("stock-market-app-title"),
            StyleClasses = { "LabelHeading" },
            HorizontalAlignment = HAlignment.Center,
        };
        AddChild(titleLabel);

        _statusLabel = new Label
        {
            Text = Loc.GetString("economic-console-loading"),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 4),
        };
        AddChild(_statusLabel);

        _portfolioLabel = new Label
        {
            Text = Loc.GetString("stock-market-portfolio") + ": 0",
            HorizontalAlignment = HAlignment.Center,
            StyleClasses = { "LabelHeading" },
            Margin = new Thickness(0, 4),
        };
        AddChild(_portfolioLabel);

        _stockScroll = new ScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        _stockList = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };

        _stockScroll.AddChild(_stockList);
        AddChild(_stockScroll);
    }

    public void UpdateState(Content.Shared.Cargo.Cartridges.StockMarketUiState state)
    {
        _stockList.RemoveAllChildren();

        if (state.Prices.Count == 0)
        {
            _statusLabel.Text = Loc.GetString("economic-console-no-data");
            _portfolioLabel.Text = Loc.GetString("stock-market-portfolio") + ": 0";
            return;
        }

        var totalShares = 0;
        var totalValue = 0.0;
        foreach (var (id, shares) in state.Portfolio)
        {
            totalShares += shares;
            if (state.Prices.TryGetValue(id, out var price))
                totalValue += shares * price.CurrentPrice;
        }

        _statusLabel.Text = Loc.GetString("stock-market-available") + $": {state.Prices.Count}";
        _portfolioLabel.Text = Loc.GetString("stock-market-portfolio") + $": {totalShares} | {totalValue:F0}cr";

        var stocksToShow = state.Prices
            .OrderByDescending(p => Math.Abs(p.Value.PriceChange))
            .Take(20)
            .ToList();

        foreach (var (id, price) in stocksToShow)
        {
            var row = CreateStockRow(id, price, state.Portfolio.GetValueOrDefault(id, 0));
            _stockList.AddChild(row);
        }
    }

    private Control CreateStockRow(string id, Content.Shared.Cargo.Cartridges.StockPriceData price, int owned)
    {
        var container = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            Margin = new Thickness(0, 2),
        };

        var infoRow = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
        };

        var displayName = Loc.GetString(id);
        var nameLabel = new Label
        {
            Text = TruncateString(displayName, 20),
            HorizontalExpand = true,
            MinWidth = 130,
            StyleClasses = { "LabelHeading" },
        };

        var changeText = price.PriceChange >= 0 ? "▲" : "▼";
        var priceLabel = new Label
        {
            Text = $"{price.CurrentPrice:F0}cr {changeText}{Math.Abs(price.PriceChange * 100):F0}%",
            MinWidth = 100,
            HorizontalAlignment = HAlignment.Right,
        };

        infoRow.AddChild(nameLabel);
        infoRow.AddChild(priceLabel);
        container.AddChild(infoRow);

        var actionRow = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            HorizontalAlignment = HAlignment.Right,
        };

        var ownedLabel = new Label
        {
            Text = Loc.GetString("stock-market-owned") + $": {owned}",
            MinWidth = 80,
            VerticalAlignment = VAlignment.Center,
        };

        var sellButton = new Button
        {
            Text = Loc.GetString("stock-market-sell-btn"),
            MinWidth = 50,
            Disabled = owned <= 0,
        };
        sellButton.OnPressed += _ => OnSellPressed?.Invoke(id, 1);

        var buyButton = new Button
        {
            Text = Loc.GetString("stock-market-buy-btn"),
            MinWidth = 50,
        };
        buyButton.OnPressed += _ => OnBuyPressed?.Invoke(id, 1);

        actionRow.AddChild(ownedLabel);
        actionRow.AddChild(sellButton);
        actionRow.AddChild(buyButton);
        container.AddChild(actionRow);

        var separator = new Control
        {
            MinHeight = 1,
            Margin = new Thickness(0, 4),
        };
        container.AddChild(separator);

        return container;
    }

    private static string TruncateString(string str, int maxLength)
    {
        return str.Length <= maxLength ? str : str[..(maxLength - 2)] + "..";
    }
}
