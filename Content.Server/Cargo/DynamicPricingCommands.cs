using Content.Server.Administration;
using Content.Server.Cargo.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;
using System.Linq;

namespace Content.Server.Cargo;

[AdminCommand(AdminFlags.Debug)]
public sealed class ViewPricesCommand : IConsoleCommand
{
    public string Command => "viewprices";
    public string Description => Loc.GetString("cmd-viewprices-desc");
    public string Help => Loc.GetString("cmd-viewprices-help", ("command", Command));

    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var dynamicPricing = _entitySystemManager.GetEntitySystem<DynamicPricingSystem>();
        var prices = dynamicPricing.GetAllPrices();

        if (args.Length > 0)
        {
            var goodId = args[0];
            if (prices.TryGetValue(goodId, out var multiplier))
            {
                shell.WriteLine($"{goodId}: Multiplier = {multiplier:F2} ({(multiplier - 1.0f) * 100f:+0.0;-0.0}%)");
            }
            else
            {
                shell.WriteLine(Loc.GetString("cmd-viewprices-good-not-found", ("goodId", goodId)));
                shell.WriteLine(Loc.GetString("cmd-viewprices-available-goods"));
                foreach (var (id, mult) in prices)
                {
                    shell.WriteLine($"  - {id}");
                }
            }
        }
        else
        {
            shell.WriteLine(Loc.GetString("cmd-viewprices-header"));
            shell.WriteLine($"{Loc.GetString("cmd-viewprices-column-good-id"),-20} {Loc.GetString("cmd-viewprices-column-multiplier"),12} {Loc.GetString("cmd-viewprices-column-change"),10}");
            shell.WriteLine(new string('-', 45));

            foreach (var (goodId, multiplier) in prices.OrderBy(x => x.Key))
            {
                var change = (multiplier - 1.0f) * 100f;
                shell.WriteLine($"{goodId,-20} {multiplier,12:F2} {change,9:+0.0;-0.0}%");
            }

            shell.WriteLine(new string('-', 45));
            shell.WriteLine(Loc.GetString("cmd-viewprices-total", ("count", prices.Count)));
        }
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class ForceUpdatePricesCommand : IConsoleCommand
{
    public string Command => "forceupdateprices";
    public string Description => Loc.GetString("cmd-forceupdateprices-desc");
    public string Help => Loc.GetString("cmd-forceupdateprices-help", ("command", Command));

    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var dynamicPricing = _entitySystemManager.GetEntitySystem<DynamicPricingSystem>();
        dynamicPricing.ForceUpdatePrices();
        shell.WriteLine(Loc.GetString("cmd-forceupdateprices-success"));
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class ResetPricesCommand : IConsoleCommand
{
    public string Command => "resetprices";
    public string Description => Loc.GetString("cmd-resetprices-desc");
    public string Help => Loc.GetString("cmd-resetprices-help", ("command", Command));

    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var dynamicPricing = _entitySystemManager.GetEntitySystem<DynamicPricingSystem>();
        dynamicPricing.ResetAllPrices();
        shell.WriteLine(Loc.GetString("cmd-resetprices-success"));
    }
}

[AdminCommand(AdminFlags.Debug)]
public sealed class ViewShuttlePricesCommand : IConsoleCommand
{
    public string Command => "viewshuttleprices";
    public string Description => Loc.GetString("cmd-viewshuttleprices-desc");
    public string Help => Loc.GetString("cmd-viewshuttleprices-help", ("command", Command));

    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var shuttlePricing = _entitySystemManager.GetEntitySystem<ShuttlePricingSystem>();
        
        shell.WriteLine(Loc.GetString("cmd-viewshuttleprices-header"));
        shell.WriteLine($"{Loc.GetString("cmd-viewshuttleprices-column-entity"),-15} {Loc.GetString("cmd-viewshuttleprices-column-base"),12} {Loc.GetString("cmd-viewshuttleprices-column-current"),15} {Loc.GetString("cmd-viewshuttleprices-column-change"),10}");
        shell.WriteLine(new string('-', 55));

        var query = _entityManager.EntityQueryEnumerator<Content.Server.Cargo.Components.ShuttlePricingComponent>();
        int count = 0;

        while (query.MoveNext(out var uid, out var component))
        {
            var change = component.CurrentPrice - component.BasePrice;
            var percentChange = (change / (float)component.BasePrice) * 100f;
            
            shell.WriteLine($"{uid,-15} {component.BasePrice,12:N0} {component.CurrentPrice,15:N0} {percentChange,9:+0.0;-0.0}%");
            count++;
        }

        shell.WriteLine(new string('-', 55));
        shell.WriteLine(Loc.GetString("cmd-viewshuttleprices-total", ("count", count)));

        if (count == 0)
        {
            shell.WriteLine(Loc.GetString("cmd-viewshuttleprices-none"));
        }
    }
}
