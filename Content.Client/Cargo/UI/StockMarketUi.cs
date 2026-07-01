using Content.Client.UserInterface.Fragments;
using Content.Shared.Cargo.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.Cargo.UI;

public sealed partial class StockMarketUi : UIFragment
{
    private StockMarketUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new StockMarketUiFragment();

        _fragment.OnBuyPressed += (companyId, amount) =>
        {
            var ev = new StockMarketUiMessageEvent(StockMarketUiAction.Buy, companyId, amount);
            userInterface.SendMessage(new CartridgeUiMessage(ev));
        };

        _fragment.OnSellPressed += (companyId, amount) =>
        {
            var ev = new StockMarketUiMessageEvent(StockMarketUiAction.Sell, companyId, amount);
            userInterface.SendMessage(new CartridgeUiMessage(ev));
        };

        var requestEv = new StockMarketUiMessageEvent(StockMarketUiAction.RequestPrices, string.Empty, 0);
        userInterface.SendMessage(new CartridgeUiMessage(requestEv));
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is StockMarketUiState cast)
        {
            _fragment?.UpdateState(cast);
        }
    }
}
