using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared._Rat.Factions;
using Robust.Shared.Console;
using Content.Server.Database;

namespace Content.Server._Rat.Factions.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class SendFactionsCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public override string Command => "sendfactions";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var factions = await _db.GetAllFactions();

        if (factions.Count == 0)
        {
            shell.WriteLine(Loc.GetString("rat-faction-admin-no-factions"));
            return;
        }

        shell.WriteLine(Loc.GetString("rat-faction-admin-list-header"));
        foreach (var faction in factions)
        {
            var wl = faction.IsWhitelisted ? " [WL]" : "";
            shell.WriteLine($"- {faction.Name}{wl}: {faction.Description}");
        }

        shell.WriteLine(Loc.GetString("rat-faction-admin-total", ("count", factions.Count)));
    }
}
