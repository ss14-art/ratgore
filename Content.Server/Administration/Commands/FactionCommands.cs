using System.Linq;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class FactionCreateCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public override string Command => "factioncreate";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 2),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            return;
        }

        var name = args[0].Trim();
        if (!bool.TryParse(args[1].Trim(), out var isWhitelisted))
        {
            shell.WriteError(Loc.GetString("rat-faction-admin-invalid-boolean", ("value", args[1])));
            shell.WriteLine(Help);
            return;
        }

        var description = args.Length > 2 ? string.Join(' ', args[2..]) : string.Empty;

        await _db.CreateFaction(name, description, isWhitelisted);
        shell.WriteLine(Loc.GetString("rat-faction-admin-created", ("name", name), ("whitelisted", isWhitelisted)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 2)
        {
            return CompletionResult.FromHintOptions(
                new[] { "true", "false" },
                "true/false");
        }

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class FactionDeleteCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public override string Command => "factiondelete";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 1),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            return;
        }

        if (!int.TryParse(args[0].Trim(), out var factionId))
        {
            shell.WriteError(Loc.GetString("rat-faction-admin-invalid-id", ("id", args[0])));
            return;
        }

        var success = await _db.DeleteFactionById(factionId);
        if (!success)
        {
            shell.WriteError(Loc.GetString("rat-faction-admin-delete-failed", ("id", factionId)));
            return;
        }

        shell.WriteLine(Loc.GetString("rat-faction-admin-deleted", ("id", factionId)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class FactionListCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public override string Command => "factionlist";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var factions = await _db.GetAllFactions();
        if (factions.Count == 0)
        {
            shell.WriteLine(Loc.GetString("rat-faction-admin-no-subfactions"));
            return;
        }

        shell.WriteLine(Loc.GetString("rat-faction-admin-list-columns"));
        shell.WriteLine(new string('-', 100));
        foreach (var faction in factions)
        {
            var whitelisted = faction.IsWhitelisted ? Loc.GetString("rat-faction-admin-yes") : Loc.GetString("rat-faction-admin-no");
            var desc = faction.Description.Length > 40 ? faction.Description[..37] + "..." : faction.Description;
            shell.WriteLine($"{faction.Id,-5} | {faction.Name,-30} | {whitelisted,-12} | {desc}");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class FactionSetManagerCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    public override string Command => "factionsetmanager";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 2),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            shell.WriteLine(Loc.GetString("rat-faction-admin-use-factionlist"));
            return;
        }

        var playerName = args[0].Trim();
        
        if (!int.TryParse(args[1].Trim(), out var factionId))
        {
            shell.WriteError(Loc.GetString("rat-faction-admin-invalid-id", ("id", args[1])));
            shell.WriteLine(Loc.GetString("rat-faction-admin-use-factionlist"));
            return;
        }

        var data = await _playerLocator.LookupIdByNameAsync(playerName);
        if (data == null)
        {
            shell.WriteError(Loc.GetString("rat-faction-admin-player-not-found", ("playerName", playerName)));
            return;
        }

        var success = await _db.AddFactionManagerById(data.UserId, factionId);
        if (!success)
        {
            shell.WriteError(Loc.GetString("rat-faction-admin-faction-not-found", ("factionId", factionId)));
            return;
        }

        shell.WriteLine(Loc.GetString("rat-faction-admin-set-manager", ("playerName", playerName), ("factionId", factionId)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                _players.Sessions.Select(s => s.Name),
                "Player name");
        }

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class FactionRemoveManagerCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    public override string Command => "factionremovemanager";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 2),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            shell.WriteLine(Loc.GetString("rat-faction-admin-use-factionlist"));
            return;
        }

        var playerName = args[0].Trim();
        
        if (!int.TryParse(args[1].Trim(), out var factionId))
        {
            shell.WriteError(Loc.GetString("rat-faction-admin-invalid-id", ("id", args[1])));
            shell.WriteLine(Loc.GetString("rat-faction-admin-use-factionlist"));
            return;
        }

        var data = await _playerLocator.LookupIdByNameAsync(playerName);
        if (data == null)
        {
            shell.WriteError(Loc.GetString("rat-faction-admin-player-not-found", ("playerName", playerName)));
            return;
        }

        var success = await _db.RemoveFactionManagerById(data.UserId, factionId);
        if (!success)
        {
            shell.WriteError(Loc.GetString("rat-faction-admin-remove-failed", ("factionId", factionId)));
            return;
        }

        shell.WriteLine(Loc.GetString("rat-faction-admin-remove-manager", ("playerName", playerName), ("factionId", factionId)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                _players.Sessions.Select(s => s.Name),
                "Player name");
        }

        return CompletionResult.Empty;
    }
}
