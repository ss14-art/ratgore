using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Client.UserInterface.Systems.Chat;
using System.Linq;

namespace Content.Client._Rat.Factions.Commands;

[AnyCommand]
public sealed class SelectFactionCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "selectfaction";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var system = _entitySystemManager.GetEntitySystem<RatFactionSystem>();

        if (args.Length == 0)
        {
            var factions = system.GetAvailableFactions();

            if (factions.Count == 0)
            {
                shell.WriteLine(Loc.GetString("rat-faction-command-no-factions"));
                return;
            }

            shell.WriteLine(Loc.GetString("rat-faction-command-available"));
            shell.WriteLine(Loc.GetString("rat-faction-command-none"));
            foreach (var faction in factions)
            {
                var wl = faction.IsWhitelisted ? " [WL]" : "";
                shell.WriteLine($"  {faction.Name}{wl} - {faction.Description}");
            }
            shell.WriteLine(Loc.GetString("rat-faction-command-usage"));
            return;
        }

        var factionName = string.Join(" ", args);

        if (factionName.ToLower() == "none")
        {
            system.SelectFaction("");
            shell.WriteLine(Loc.GetString("rat-faction-command-reset"));
            return;
        }

        var selected = system.GetAvailableFactions().FirstOrDefault(f =>
            f.Name.Equals(factionName, System.StringComparison.OrdinalIgnoreCase));

        if (selected == null)
        {
            shell.WriteLine(Loc.GetString("rat-faction-command-not-found", ("factionName", factionName)));
            return;
        }

        system.SelectFaction(selected.Name);
        shell.WriteLine(Loc.GetString("rat-faction-command-selected", ("factionName", selected.Name)));
    }
}
