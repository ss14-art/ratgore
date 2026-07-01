using System.Linq;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared._Rat.Factions;
using Content.Shared.GameTicking;
using Robust.Server.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Player;
using Robust.Shared.Enums;

namespace Content.Server._Rat.Factions;

public sealed class RatFactionSystem : EntitySystem
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
        SubscribeNetworkEvent<RequestFactionListMessage>(OnRequestFactionList);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _playerManager.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.Connected)
        {
            await SendFactionList(e.Session);
        }
    }

    private async void OnRequestFactionList(RequestFactionListMessage msg, EntitySessionEventArgs args)
    {
        await SendFactionList(args.SenderSession);
    }

    private async Task SendFactionList(ICommonSession session)
    {
        var factions = await _db.GetAllFactions();
        var whitelists = await _db.GetPlayerFactionWhitelists(session.UserId);

        var availableFactions = factions
            .Where(f => !f.IsWhitelisted || whitelists.Contains(f.Name))
            .Select(f => new RatFactionInfo
            {
                Name = f.Name,
                Description = f.Description,
                IsWhitelisted = f.IsWhitelisted,
            })
            .ToList();

        var message = new FactionListMessage
        {
            Factions = availableFactions
        };

        RaiseNetworkEvent(message, session);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        var profile = ev.Profile;
        var entity = ev.Mob;

        if (string.IsNullOrEmpty(profile.Subfaction))
            return;

        var comp = EnsureComp<RatFactionComponent>(entity);
        comp.SubfactionName = profile.Subfaction;
        Dirty(entity, comp);
    }
}
