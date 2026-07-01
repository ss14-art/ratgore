using System.Linq;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class FactionWhitelistAddCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    public override string Command => "factionwhitelistadd";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 2),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            shell.WriteLine("Usage: factionwhitelistadd <player> <faction_id>");
            shell.WriteLine("Use 'factionlist' to see subfaction IDs");
            return;
        }

        var playerName = args[0].Trim();
        
        if (!int.TryParse(args[1].Trim(), out var factionId))
        {
            shell.WriteError($"Invalid subfaction ID: {args[1]}. Must be a number.");
            shell.WriteLine("Use 'factionlist' to see subfaction IDs");
            return;
        }
        
        var data = await _playerLocator.LookupIdByNameAsync(playerName);
        if (data == null)
        {
            shell.WriteError($"Player '{playerName}' not found.");
            return;
        }

        var success = await _db.AddFactionWhitelistById(data.UserId, factionId);
        if (!success)
        {
            shell.WriteError($"Failed to whitelist player '{playerName}' for subfaction ID {factionId}. Subfaction may not exist or player is already whitelisted.");
            return;
        }

        shell.WriteLine($"Whitelisted '{playerName}' for subfaction ID {factionId}.");
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
public sealed class FactionWhitelistRemoveCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    public override string Command => "factionwhitelistremove";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 2),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            shell.WriteLine("Usage: factionwhitelistremove <player> <faction_id>");
            shell.WriteLine("Use 'factionlist' to see subfaction IDs");
            return;
        }

        var playerName = args[0].Trim();
        
        if (!int.TryParse(args[1].Trim(), out var factionId))
        {
            shell.WriteError($"Invalid subfaction ID: {args[1]}. Must be a number.");
            shell.WriteLine("Use 'factionlist' to see subfaction IDs");
            return;
        }
        
        var data = await _playerLocator.LookupIdByNameAsync(playerName);
        if (data == null)
        {
            shell.WriteError($"Player '{playerName}' not found.");
            return;
        }

        var success = await _db.RemoveFactionWhitelistById(data.UserId, factionId);
        if (!success)
        {
            shell.WriteError($"Failed to remove whitelist for player '{playerName}' from subfaction ID {factionId}. Player may not be whitelisted.");
            return;
        }

        shell.WriteLine($"Removed whitelist for '{playerName}' from subfaction ID {factionId}.");
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
public sealed class FactionWhitelistGetCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    public override string Command => "factionwhitelistget";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 2),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            shell.WriteLine("Usage: factionwhitelistget <player> <faction_id>");
            shell.WriteLine("Use 'factionlist' to see subfaction IDs");
            return;
        }

        var playerName = args[0].Trim();
        
        if (!int.TryParse(args[1].Trim(), out var factionId))
        {
            shell.WriteError($"Invalid subfaction ID: {args[1]}. Must be a number.");
            shell.WriteLine("Use 'factionlist' to see subfaction IDs");
            return;
        }
        
        var data = await _playerLocator.LookupIdByNameAsync(playerName);
        if (data == null)
        {
            shell.WriteError($"Player '{playerName}' not found.");
            return;
        }

        var isWhitelisted = await _db.IsFactionWhitelistedById(data.UserId, factionId);
        shell.WriteLine($"Player '{playerName}' is {(isWhitelisted ? "" : "NOT ")}whitelisted for subfaction ID {factionId}.");
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
