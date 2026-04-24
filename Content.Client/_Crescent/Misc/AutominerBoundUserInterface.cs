using Content.Shared._Crescent.Misc;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Crescent.Misc;

[UsedImplicitly]
public sealed class AutominerBoundUserInterface : BoundUserInterface
{
    private AutominerWindow? _window;

    public AutominerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<AutominerWindow>();
        _window.OnStartButtonPressed += () => SendMessage(new AutominerStartMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not AutominerBoundUserInterfaceState msg) return;
        _window?.UpdateState(msg);
    }
}