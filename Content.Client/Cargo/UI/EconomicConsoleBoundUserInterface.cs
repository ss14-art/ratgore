using Content.Shared.Cargo;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;

namespace Content.Client.Cargo.UI;

public sealed class EconomicConsoleBoundUserInterface : BoundUserInterface
{
    private EconomicConsoleWindow? _window;

    public EconomicConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = new EconomicConsoleWindow(this);
        _window.OpenCentered();
        _window.OnClose += Close;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is EconomicConsoleBuiState buiState && _window != null)
        {
            _window.UpdateState(buiState.ItemPrices, buiState.ShuttlePrices);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _window?.Dispose();
    }
}
