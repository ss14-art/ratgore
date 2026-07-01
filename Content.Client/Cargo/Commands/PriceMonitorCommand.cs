using Content.Client.Cargo.Systems;
using Robust.Shared.Console;

namespace Content.Client.Cargo.Commands;

public sealed class PriceMonitorCommand : IConsoleCommand
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public string Command => "pricemonitor";
    public string Help => "Open the economic price monitor window";
    public string Description => "Open the economic price monitor window";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var priceSystem = _entitySystemManager.GetEntitySystem<PriceMonitorSystem>();
        priceSystem.OpenPriceMonitor();
    }
}
