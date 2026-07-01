using Content.Server.Administration;
using Content.Server.Cargo.Systems;
using Content.Shared.Administration;
using Content.Shared.Cargo;
using Robust.Shared.Console;

namespace Content.Server.Cargo.Commands;

[AdminCommand(AdminFlags.Fun)]
public sealed class SetMarketTrendCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "setmarkettrend";
    public override string Description => Loc.GetString("cmd-setmarkettrend-desc");
    public override string Help => Loc.GetString("cmd-setmarkettrend-help", ("command", Command));

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
            shell.WriteLine(Help);
            return;
        }

        var goodId = args[0];
        
        if (!Enum.TryParse<MarketTrendDirection>(args[1], true, out var direction))
        {
            shell.WriteLine(Loc.GetString("cmd-setmarkettrend-invalid-direction"));
            shell.WriteLine(Loc.GetString("cmd-setmarkettrend-valid-directions"));
            return;
        }

        var strength = 1.0f;
        if (args.Length > 2 && !float.TryParse(args[2], out strength))
        {
            shell.WriteLine(Loc.GetString("cmd-setmarkettrend-invalid-strength"));
            return;
        }

        var duration = 600f;
        if (args.Length > 3 && !float.TryParse(args[3], out duration))
        {
            shell.WriteLine(Loc.GetString("cmd-setmarkettrend-invalid-duration"));
            return;
        }

        shell.WriteLine(Loc.GetString("cmd-setmarkettrend-success", 
            ("goodId", goodId), 
            ("direction", direction),
            ("strength", strength),
            ("duration", duration)));
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class CrashMarketCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "crashmarket";
    public override string Description => Loc.GetString("cmd-crashmarket-desc");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var dynamicPricing = _entitySystemManager.GetEntitySystem<DynamicPricingSystem>();
        shell.WriteLine(Loc.GetString("cmd-crashmarket-executed"));
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class VolatileMarketCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "volatilemarket";
    public override string Description => Loc.GetString("cmd-volatilemarket-desc");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        shell.WriteLine(Loc.GetString("cmd-volatilemarket-executed"));
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class StabilizeMarketCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "stabilizemarket";
    public override string Description => Loc.GetString("cmd-stabilizemarket-desc");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var dynamicPricing = _entitySystemManager.GetEntitySystem<DynamicPricingSystem>();
        dynamicPricing.ResetAllPrices();
        shell.WriteLine(Loc.GetString("cmd-stabilizemarket-executed"));
    }
}
