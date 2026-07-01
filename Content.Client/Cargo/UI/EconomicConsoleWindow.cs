using System.Linq;
using System.Numerics;
using Content.Shared.Cargo;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client.Cargo.UI;

public sealed class EconomicConsoleWindow : DefaultWindow
{
    private readonly Label _statusLabel;
    private readonly LineEdit _searchBox;
    private readonly ScrollContainer _contentScroll;
    private readonly BoxContainer _contentList;
    
    private Dictionary<string, ItemPriceData> _currentPrices = new();
    private Dictionary<string, ShuttlePriceInfoData> _shuttlePrices = new();
    private string _searchFilter = string.Empty;

    public EconomicConsoleWindow(EconomicConsoleBoundUserInterface bui)
    {
        Title = Loc.GetString("economic-console-title");
        MinSize = new Vector2(550, 450);

        var vbox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Margin = new Thickness(4),
        };

        _statusLabel = new Label
        {
            Text = Loc.GetString("economic-console-loading"),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 4),
        };
        vbox.AddChild(_statusLabel);

        var searchContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Margin = new Thickness(0, 4),
        };

        var searchLabel = new Label
        {
            Text = Loc.GetString("economic-console-search") + ":",
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
        };

        _searchBox = new LineEdit
        {
            HorizontalExpand = true,
            PlaceHolder = Loc.GetString("economic-console-search-placeholder"),
        };
        _searchBox.OnTextChanged += OnSearchChanged;

        searchContainer.AddChild(searchLabel);
        searchContainer.AddChild(_searchBox);
        vbox.AddChild(searchContainer);

        _contentScroll = new ScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        _contentList = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };

        _contentScroll.AddChild(_contentList);
        vbox.AddChild(_contentScroll);

        Contents.AddChild(vbox);
    }

    private void OnSearchChanged(LineEdit.LineEditEventArgs args)
    {
        _searchFilter = args.Text.ToLowerInvariant();
        RefreshContentList();
    }

    public void UpdateState(Dictionary<string, ItemPriceData> prices, Dictionary<string, ShuttlePriceInfoData> shuttlePrices)
    {
        _currentPrices = prices;
        _shuttlePrices = shuttlePrices;
        _statusLabel.Text = Loc.GetString("economic-console-status-count", ("count", prices.Count + shuttlePrices.Count));
        RefreshContentList();
    }

    private void RefreshContentList()
    {
        _contentList.RemoveAllChildren();

        if (_shuttlePrices.Count > 0)
        {
            var shuttleHeader = new Label
            {
                Text = Loc.GetString("economic-console-shuttles"),
                StyleClasses = { "LabelHeading" },
                Margin = new Thickness(0, 8, 0, 4),
            };
            _contentList.AddChild(shuttleHeader);

            foreach (var (name, shuttle) in _shuttlePrices)
            {
                if (!string.IsNullOrEmpty(_searchFilter) && !name.ToLowerInvariant().Contains(_searchFilter))
                    continue;

                var row = CreateShuttleRow(shuttle);
                _contentList.AddChild(row);
            }
        }

        var itemHeader = new Label
        {
            Text = Loc.GetString("economic-console-items"),
            StyleClasses = { "LabelHeading" },
            Margin = new Thickness(0, 12, 0, 4),
        };
        _contentList.AddChild(itemHeader);

        var filteredItems = _currentPrices
            .Where(p => string.IsNullOrEmpty(_searchFilter) || 
                p.Key.ToLowerInvariant().Contains(_searchFilter) ||
                (!string.IsNullOrEmpty(p.Value.LocalizedName) && p.Value.LocalizedName.ToLowerInvariant().Contains(_searchFilter)))
            .OrderBy(p => p.Key)
            .ToList();

        if (filteredItems.Count == 0 && _shuttlePrices.Count == 0)
        {
            var noResults = new Label
            {
                Text = Loc.GetString("economic-console-no-results"),
                HorizontalAlignment = HAlignment.Center,
                VerticalAlignment = VAlignment.Center,
                Margin = new Thickness(0, 20),
            };
            _contentList.AddChild(noResults);
            return;
        }

        var displayCount = Math.Min(filteredItems.Count, 50);
        
        for (var i = 0; i < displayCount; i++)
        {
            var (itemId, price) = filteredItems[i];
            var row = CreateItemRow(price);
            _contentList.AddChild(row);
        }

        if (filteredItems.Count > displayCount)
        {
            var moreLabel = new Label
            {
                Text = Loc.GetString("economic-console-more-items", ("count", filteredItems.Count - displayCount)),
                HorizontalAlignment = HAlignment.Center,
                Margin = new Thickness(0, 8),
            };
            _contentList.AddChild(moreLabel);
        }
    }

    private Control CreateShuttleRow(ShuttlePriceInfoData shuttle)
    {
        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Margin = new Thickness(4, 2),
        };

        var nameLabel = new Label
        {
            Text = TruncateString(shuttle.Name, 30),
            HorizontalExpand = true,
            MinWidth = 180,
        };

        var baseLabel = new Label
        {
            Text = $"{shuttle.BasePrice:N0}cr",
            MinWidth = 80,
            HorizontalAlignment = HAlignment.Right,
        };

        var currentLabel = new Label
        {
            Text = $"{shuttle.CurrentPrice:N0}cr",
            MinWidth = 80,
            HorizontalAlignment = HAlignment.Right,
        };

        var changeText = shuttle.PriceChange >= 0 ? "▲" : "▼";
        var changePercent = Math.Abs(shuttle.PriceChange * 100);
        var changeLabel = new Label
        {
            Text = $"{changeText}{changePercent:F0}%",
            MinWidth = 60,
            HorizontalAlignment = HAlignment.Right,
        };

        container.AddChild(nameLabel);
        container.AddChild(baseLabel);
        container.AddChild(currentLabel);
        container.AddChild(changeLabel);

        return container;
    }

    private Control CreateItemRow(ItemPriceData price)
    {
        var container = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Margin = new Thickness(0, 2),
        };

        var displayName = !string.IsNullOrEmpty(price.LocalizedName) ? price.LocalizedName : price.ItemId;
        var nameLabel = new Label
        {
            Text = TruncateString(displayName, 30),
            HorizontalExpand = true,
            MinWidth = 180,
        };

        var baseLabel = new Label
        {
            Text = $"{price.BasePrice:F0}cr",
            MinWidth = 70,
            HorizontalAlignment = HAlignment.Right,
        };

        var currentLabel = new Label
        {
            Text = $"{price.CurrentPrice:F0}cr",
            MinWidth = 70,
            HorizontalAlignment = HAlignment.Right,
        };

        var changeText = price.Multiplier >= 1.0f ? "▲" : "▼";
        var changePercent = Math.Abs((price.Multiplier - 1.0f) * 100);
        var changeLabel = new Label
        {
            Text = $"{changeText}{changePercent:F0}%",
            MinWidth = 60,
            HorizontalAlignment = HAlignment.Right,
        };

        container.AddChild(nameLabel);
        container.AddChild(baseLabel);
        container.AddChild(currentLabel);
        container.AddChild(changeLabel);

        return container;
    }

    private static string TruncateString(string str, int maxLength)
    {
        return str.Length <= maxLength ? str : str[..(maxLength - 2)] + "..";
    }

    public void UpdateStatus(string text)
    {
        _statusLabel.Text = text;
    }
}
