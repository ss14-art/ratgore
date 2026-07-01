using System.Linq;
using System.Numerics;
using Content.Server.Chat.Managers;
using Content.Server.SurveillanceCamera;
using Content.Server._Rat.Squad;
using Content.Shared._Rat.Overwatch;
using Content.Shared._Rat.Squad;
using Content.Shared._Crescent.HullrotFaction;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.Implants.Components;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Power;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Maths;

namespace Content.Server._Rat.Overwatch;

/// <summary>
/// Система для управления Overwatch.
/// </summary>
public sealed class OverwatchSystem : EntitySystem
{
    /// <summary>
    /// Интервал инвалидации кэша в секундах.
    /// Кэш сбрасывается при изменении состава фракции.
    /// </summary>
    private const float CacheInvalidationInterval = 2.0f;

    /// <summary>
    /// Интервал обновления UI в секундах.
    /// </summary>
    private const float UpdateInterval = 1.0f;

    /// <summary>
    /// ID слота экипировки для камеры наблюдения.
    /// </summary>
    private const string CameraSlotId = "neck";

    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
    [Dependency] private readonly SurveillanceCameraSystem _cameraSystem = default!;
    [Dependency] private readonly SharedIdCardSystem _idCardSystem = default!;
    [Dependency] private readonly SharedEyeSystem _eyeSystem = default!;
    [Dependency] private readonly ViewSubscriberSystem _viewSubscriberSystem = default!;
    [Dependency] private readonly SquadSystem _squadSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    /// <summary>
    /// Пары наблюдения: watcher -> target.
    /// </summary>
    private readonly Dictionary<EntityUid, EntityUid> _watchingPairs = new();

    /// <summary>
    /// Кэш членов фракции для снижения аллокаций при частых обновлениях.
    /// </summary>
    private readonly Dictionary<string, List<EntityUid>> _factionMembersCache = new();

    /// <summary>
    /// Кэш данных участников для UI.
    /// </summary>
    private readonly Dictionary<EntityUid, OverwatchMemberData> _memberDataCache = new();

    private float _cacheInvalidationTimer;
    private float _updateTimer;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OverwatchConsoleComponent, ActivatableUIOpenAttemptEvent>(OnUIOpenAttempt);
        SubscribeLocalEvent<OverwatchConsoleComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
        SubscribeLocalEvent<OverwatchConsoleComponent, OverwatchRefreshMessage>(OnRefresh);
        SubscribeLocalEvent<OverwatchConsoleComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<OverwatchConsoleComponent, ComponentShutdown>(OnConsoleShutdown);
        SubscribeLocalEvent<RatOverwatchWatchingComponent, MoveInputEvent>(OnWatchingMoveInput);
        SubscribeLocalEvent<RatOverwatchWatchingComponent, ComponentShutdown>(OnWatchingShutdown);
        SubscribeLocalEvent<RatOverwatchCameraComponent, ComponentShutdown>(OnCameraShutdown);
        SubscribeLocalEvent<RatOverwatchCameraComponent, EntityTerminatingEvent>(OnWatchedEntityTerminating);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn); // Обновление UI при спавне игрока для отображения новых членов фракции
        // Инвалидация кэша при изменении состава фракции или отрядов
        SubscribeLocalEvent<HullrotFactionComponent, ComponentInit>(OnFactionComponentInit);
        SubscribeLocalEvent<HullrotFactionComponent, ComponentShutdown>(OnFactionComponentShutdown);
        SubscribeLocalEvent<SquadComponent, ComponentInit>(OnSquadComponentInit);
        SubscribeLocalEvent<SquadComponent, ComponentShutdown>(OnSquadComponentShutdown);

        Subs.BuiEvents<OverwatchConsoleComponent>(OverwatchUiKey.Key, subs =>
        {
            subs.Event<OverwatchViewCameraMessage>(OnViewCamera);
            subs.Event<OverwatchStopWatchingMessage>(OnStopWatching);
            subs.Event<OverwatchSetStatusFilterMessage>(OnSetStatusFilter);
            subs.Event<OverwatchSetSquadFilterMessage>(OnSetSquadFilter);
            subs.Event<OverwatchSetSearchMessage>(OnSetSearch);
            subs.Event<OverwatchCreateSquadMessage>(OnCreateSquad);
            subs.Event<OverwatchDeleteSquadMessage>(OnDeleteSquad);
            subs.Event<OverwatchAssignSquadMessage>(OnAssignSquad);
            subs.Event<OverwatchRemoveSquadMemberMessage>(OnRemoveSquadMember);
            subs.Event<OverwatchSendMessageAnnouncement>(OnSendAnnouncement);
        });
    }

    /// <summary>
    /// Инвалидация кэша при инициализации компонента фракции.
    /// </summary>
    private void OnFactionComponentInit(EntityUid uid, HullrotFactionComponent component, ComponentInit args)
    {
        _factionMembersCache.Clear();
    }

    /// <summary>
    /// Инвалидация кэша при удалении компонента фракции.
    /// </summary>
    private void OnFactionComponentShutdown(EntityUid uid, HullrotFactionComponent component, ComponentShutdown args)
    {
        _factionMembersCache.Clear();
        _memberDataCache.Remove(uid);
    }

    /// <summary>
    /// Инвалидация кэша при инициализации компонента отряда.
    /// </summary>
    private void OnSquadComponentInit(EntityUid uid, SquadComponent component, ComponentInit args)
    {
        _factionMembersCache.Clear();
    }

    /// <summary>
    /// Инвалидация кэша при удалении компонента отряда.
    /// </summary>
    private void OnSquadComponentShutdown(EntityUid uid, SquadComponent component, ComponentShutdown args)
    {
        _factionMembersCache.Clear();
        _memberDataCache.Remove(uid);
    }

    /// <summary>
    /// Обработчик открытия UI консоли — останавливает наблюдение если игрок уже смотрит через камеру.
    /// </summary>
    private void OnBeforeUIOpen(Entity<OverwatchConsoleComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        if (TryComp<RatOverwatchWatchingComponent>(args.User, out var watchingComp) && watchingComp.Watching.HasValue)
        {
            StopWatching(args.User, watchingComp);
        }

        RefreshData(ent);
    }

    /// <summary>
    /// Обработчик попытки открытия UI — проверяет принадлежность к фракции.
    /// </summary>
    private void OnUIOpenAttempt(Entity<OverwatchConsoleComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (args.User is not { Valid: true } user)
            return;
    }

    /// <summary>
    /// Обработчик движения игрока — останавливает наблюдение при попытке движения.
    /// </summary>
    private void OnWatchingMoveInput(Entity<RatOverwatchWatchingComponent> ent, ref MoveInputEvent args)
    {
        if (!args.HasDirectionalMovement)
            return;

        StopWatching(ent.Owner, ent.Comp);
        RemComp<RatOverwatchWatchingComponent>(ent.Owner);
    }

    /// <summary>
    /// Обновляет данные UI консоли Overwatch.
    /// </summary>
    private void RefreshData(Entity<OverwatchConsoleComponent> ent)
    {
        var members = GetFactionMembers(ent.Comp.Faction);
        var memberData = new List<OverwatchMemberData>(members.Count);

        var availableSquads = _squadSystem.GetFactionSquads(ent.Comp.Faction);

        foreach (var member in members)
        {
            if (!_memberDataCache.TryGetValue(member, out var cachedData) ||
                ShouldRefreshData(member, cachedData))
            {
                var newData = CreateMemberData(member);
                _memberDataCache[member] = newData;
                memberData.Add(newData);
            }
            else
            {
                memberData.Add(cachedData);
            }
        }

        _uiSystem.SetUiState(ent.Owner, OverwatchUiKey.Key,
            new OverwatchUpdateState(
                memberData,
                availableSquads.ToDictionary(k => k.Key, v => v.Value.Name),
                ent.Comp.StatusFilter,
                ent.Comp.SquadFilter,
                ent.Comp.SearchQuery,
                GetOverwatchColor(ent.Comp.Faction)
            ));
    }

    /// <summary>
    /// Проверяет необходимость обновления данных участника.
    /// </summary>
    private bool ShouldRefreshData(EntityUid member, OverwatchMemberData cachedData)
    {
        if (!TryComp<ActorComponent>(member, out var actor))
            return cachedData.Status != OverwatchMemberStatus.Dead;

        if (actor.PlayerSession == null)
            return cachedData.Status != OverwatchMemberStatus.SSD;

        if (TryComp<SquadComponent>(member, out var squadComp))
        {
            if (cachedData.SquadId != squadComp.SquadId || cachedData.SquadName != squadComp.SquadName)
                return true;
        }
        else
        {
            // Если компонента нет, а в кэше был отряд — нужно обновить
            if (cachedData.SquadId.HasValue)
                return true;
        }

        var currentCoords = GetMemberCoordinates(member);
        if (cachedData.Coordinates.HasValue != currentCoords.HasValue)
            return true;
        if (cachedData.Coordinates.HasValue && currentCoords.HasValue)
        {
            // Проверяем, изменились ли координаты значительно (порог 0.5f)
            var dx = cachedData.Coordinates.Value.X - currentCoords.Value.X;
            var dy = cachedData.Coordinates.Value.Y - currentCoords.Value.Y;
            if (dx * dx + dy * dy > 0.25f) // 0.5f^2
                return true;
        }

        return false;
    }

    /// <summary>
    /// Создаёт данные участника для отображения в UI.
    /// </summary>
    private OverwatchMemberData CreateMemberData(EntityUid member)
    {
        return new OverwatchMemberData(
            GetNetEntity(member),
            Name(member),
            GetJobTitle(member),
            GetMemberStatus(member),
            true,
            GetSquadId(member),
            GetSquadName(member) ?? "",
            GetMemberCoordinates(member)
        );
    }

    private void OnRefresh(Entity<OverwatchConsoleComponent> ent, ref OverwatchRefreshMessage args)
    {
        RefreshData(ent);
    }

    private void OnSetStatusFilter(Entity<OverwatchConsoleComponent> ent, ref OverwatchSetStatusFilterMessage args)
    {
        ent.Comp.StatusFilter = args.Status;
        RefreshData(ent);
    }

    private void OnSetSquadFilter(Entity<OverwatchConsoleComponent> ent, ref OverwatchSetSquadFilterMessage args)
    {
        ent.Comp.SquadFilter = args.SquadId;
        RefreshData(ent);
    }

    private void OnSetSearch(Entity<OverwatchConsoleComponent> ent, ref OverwatchSetSearchMessage args)
    {
        ent.Comp.SearchQuery = args.SearchQuery;
        RefreshData(ent);
    }

    /// <summary>
    /// Обработчик создания нового отряда.
    /// </summary>
    private void OnCreateSquad(Entity<OverwatchConsoleComponent> ent, ref OverwatchCreateSquadMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        var created = _squadSystem.CreateSquad(ent.Comp.Faction, args.SquadName);
        if (created)
        {
            RefreshData(ent);
        }
    }

    /// <summary>
    /// Обработчик удаления отряда.
    /// </summary>
    private void OnDeleteSquad(Entity<OverwatchConsoleComponent> ent, ref OverwatchDeleteSquadMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        if (_squadSystem.RemoveSquad(ent.Comp.Faction, args.SquadId))
        {
            RefreshData(ent);
        }
    }

    /// <summary>
    /// Обработчик назначения игрока в отряд.
    /// </summary>
    private void OnAssignSquad(Entity<OverwatchConsoleComponent> ent, ref OverwatchAssignSquadMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        var player = GetEntity(args.Player);
        if (!player.Valid)
            return;

        if (!TryComp<HullrotFactionComponent>(player, out var factionComp) ||
            factionComp.Faction != ent.Comp.Faction)
            return;

        if (_squadSystem.AssignToSquad(player, args.SquadId, ent.Comp.Faction))
        {
            RefreshData(ent);
        }
    }

    /// <summary>
    /// Обработчик удаления игрока из отряда.
    /// </summary>
    private void OnRemoveSquadMember(Entity<OverwatchConsoleComponent> ent, ref OverwatchRemoveSquadMemberMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        var player = GetEntity(args.Player);
        if (!player.Valid)
            return;

        if (!TryComp<HullrotFactionComponent>(player, out var factionComp) ||
            factionComp.Faction != ent.Comp.Faction)
            return;

        _squadSystem.RemoveFromSquad(player);
        RefreshData(ent);
    }

    /// <summary>
    /// Обработчик отправки объявления Overwatch.
    /// </summary>
    private void OnSendAnnouncement(Entity<OverwatchConsoleComponent> ent, ref OverwatchSendMessageAnnouncement args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        if (string.IsNullOrEmpty(args.Message))
            return;

        var faction = ent.Comp.Faction;
        var factionMembers = GetFactionMembers(faction);

        var targetName = !args.TargetSquadId.HasValue
            ? Loc.GetString("overwatch-announcement-target-all")
            : GetSquadTargetName(faction, args.TargetSquadId.Value);

        var overwatchTitle = GetOverwatchTitle(faction);
        var color = GetOverwatchColor(faction);

        var recipients = new List<ICommonSession>();

        foreach (var member in factionMembers)
        {
            if (args.TargetSquadId.HasValue)
            {
                var memberSquadId = GetSquadId(member);
                if (memberSquadId != args.TargetSquadId)
                    continue;
            }

            if (TryComp<ActorComponent>(member, out var memberActor) && memberActor.PlayerSession != null)
            {
                recipients.Add(memberActor.PlayerSession);
                RaiseNetworkEvent(new OverwatchAnnouncementEvent(args.Message, targetName, overwatchTitle, color), memberActor.PlayerSession);
            }
        }

        if (recipients.Count == 0)
            return;

        var wrappedMessage = $"{overwatchTitle}: {args.Message}";

        var filter = Robust.Shared.Player.Filter.Empty();
        foreach (var recipient in recipients)
        {
            filter.AddPlayer(recipient);
        }

        _chatManager.ChatMessageToManyFiltered(
            filter,
            ChatChannel.Local,
            args.Message,
            wrappedMessage,
            EntityUid.Invalid,
            false,
            true,
            color
        );
    }

    /// <summary>
    /// Получает название отряда.
    /// </summary>
    private string GetSquadTargetName(string faction, int squadId)
    {
        var squads = _squadSystem.GetFactionSquads(faction);
        return squads.TryGetValue(squadId, out var squadInfo)
            ? squadInfo.Name
            : Loc.GetString("overwatch-announcement-target-squad");
    }

    /// <summary>
    /// Обработчик переключения вида на камеру цели.
    /// </summary>
    private void OnViewCamera(Entity<OverwatchConsoleComponent> ent, ref OverwatchViewCameraMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        var target = GetEntity(args.Target);
        if (!TryComp<HullrotFactionComponent>(target, out var factionComp) ||
            factionComp.Faction != ent.Comp.Faction)
            return;

        if (!TryComp<ActorComponent>(actor, out var actorComp) || actorComp.PlayerSession == null)
            return;

        if (_watchingPairs.TryGetValue(actor, out var currentTarget))
        {
            if (currentTarget == target)
                return;

            if (TryComp<RatOverwatchWatchingComponent>(actor, out var watchingComp))
            {
                StopWatching(actor, watchingComp);
            }
        }

        var cameraCompTarget = EnsureComp<RatOverwatchCameraComponent>(target);
        cameraCompTarget.Watching.Add(actor);
        Dirty(target, cameraCompTarget);

        var watchingCompActor = EnsureComp<RatOverwatchWatchingComponent>(actor);
        watchingCompActor.Watching = target;
        watchingCompActor.Console = ent.Owner;
        watchingCompActor.Camera = null;
        Dirty(actor, watchingCompActor);

        _watchingPairs[actor] = target;

        _eyeSystem.SetTarget(actor, target);
        _viewSubscriberSystem.AddViewSubscriber(target, actorComp.PlayerSession);
    }

    private void OnStopWatching(Entity<OverwatchConsoleComponent> ent, ref OverwatchStopWatchingMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        if (TryComp<RatOverwatchWatchingComponent>(actor, out var watchingComp) && watchingComp.Watching.HasValue)
        {
            StopWatching(actor, watchingComp);
        }
    }

    /// <summary>
    /// Останавливает наблюдение игрока за целью.
    /// </summary>
    private void StopWatching(EntityUid watcher, RatOverwatchWatchingComponent watchingComp)
    {
        if (!watchingComp.Watching.HasValue)
            return;

        var target = watchingComp.Watching.Value;

        if (watchingComp.Camera is { } camera &&
            TryComp<SurveillanceCameraComponent>(camera, out var cameraComp))
        {
            _cameraSystem.RemoveActiveViewer(camera, watcher, component: cameraComp);
        }

        if (TryComp<RatOverwatchCameraComponent>(target, out var cameraCompTarget))
        {
            cameraCompTarget.Watching.Remove(watcher);
            Dirty(target, cameraCompTarget);
        }

        if (TryComp<ActorComponent>(watcher, out var actorComp) && actorComp.PlayerSession != null)
        {
            _viewSubscriberSystem.RemoveViewSubscriber(target, actorComp.PlayerSession);
        }

        _eyeSystem.SetTarget(watcher, null);
        _watchingPairs.Remove(watcher);
        watchingComp.Watching = null;
        watchingComp.Console = null;
        watchingComp.Camera = null;
        Dirty(watcher, watchingComp);
    }

    private void OnConsoleShutdown(Entity<OverwatchConsoleComponent> ent, ref ComponentShutdown args)
    {
        StopWatchingForConsole(ent.Owner);
    }

    /// <summary>
    /// Обработчик удаления компонента наблюдения.
    /// </summary>
    private void OnWatchingShutdown(Entity<RatOverwatchWatchingComponent> ent, ref ComponentShutdown args)
    {
        if (!ent.Comp.Watching.HasValue)
            return;

        var target = ent.Comp.Watching.Value;

        if (ent.Comp.Camera is { } camera &&
            TryComp<SurveillanceCameraComponent>(camera, out var cameraComp))
        {
            _cameraSystem.RemoveActiveViewer(camera, ent.Owner, component: cameraComp);
        }

        if (TryComp<RatOverwatchCameraComponent>(target, out var cameraCompTarget))
        {
            cameraCompTarget.Watching.Remove(ent.Owner);
            Dirty(target, cameraCompTarget);
        }

        if (TryComp<ActorComponent>(ent.Owner, out var actorComp) && actorComp.PlayerSession != null)
        {
            _viewSubscriberSystem.RemoveViewSubscriber(target, actorComp.PlayerSession);
        }

        _eyeSystem.SetTarget(ent.Owner, null);
        _watchingPairs.Remove(ent.Owner);
        ent.Comp.Console = null;
        ent.Comp.Camera = null;
    }

    /// <summary>
    /// Обработчик удаления компонента камеры — очищает всех наблюдателей.
    /// </summary>
    private void OnCameraShutdown(Entity<RatOverwatchCameraComponent> ent, ref ComponentShutdown args)
    {
        foreach (var watcher in ent.Comp.Watching.ToList())
        {
            if (TryComp<RatOverwatchWatchingComponent>(watcher, out var watchingComp))
            {
                StopWatching(watcher, watchingComp);
            }
        }
    }

    /// <summary>
    /// Обработчик удаления наблюдаемой сущности — очищает всех наблюдателей.
    /// </summary>
    private void OnWatchedEntityTerminating(Entity<RatOverwatchCameraComponent> ent, ref EntityTerminatingEvent args)
    {
        foreach (var watcher in ent.Comp.Watching.ToList())
        {
            if (TryComp<RatOverwatchWatchingComponent>(watcher, out var watchingComp))
            {
                StopWatching(watcher, watchingComp);
            }
        }
    }

    /// <summary>
    /// Обработчик отключения питания консоли — закрывает все наблюдения.
    /// </summary>
    private void OnPowerChanged(Entity<OverwatchConsoleComponent> ent, ref PowerChangedEvent args)
    {
        if (args.Powered)
            return;

        StopWatchingForConsole(ent.Owner);

        _uiSystem.CloseUi(ent.Owner, OverwatchUiKey.Key);
    }

    private void StopWatchingForConsole(EntityUid console)
    {
        foreach (var (watcher, _) in _watchingPairs.ToList())
        {
            if (TryComp<RatOverwatchWatchingComponent>(watcher, out var watchingComp) &&
                watchingComp.Console == console)
            {
                StopWatching(watcher, watchingComp);
            }
        }
    }

    /// <summary>
    /// Обработчик завершения спавна игрока — обновляет UI всех открытых консолей.
    /// </summary>
    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        var query = EntityQueryEnumerator<OverwatchConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_uiSystem.IsUiOpen(uid, OverwatchUiKey.Key))
            {
                RefreshData((uid, comp));
            }
        }
    }

    /// <inheritdoc/>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateTimer += frameTime;
        _cacheInvalidationTimer += frameTime;

        if (_updateTimer < UpdateInterval)
            return;

        _updateTimer -= UpdateInterval;

        var hasOpenUi = false;
        var query = EntityQueryEnumerator<OverwatchConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_uiSystem.IsUiOpen(uid, OverwatchUiKey.Key))
            {
                hasOpenUi = true;
                RefreshData((uid, comp));
            }
        }

        if (_cacheInvalidationTimer >= CacheInvalidationInterval && hasOpenUi)
        {
            _cacheInvalidationTimer -= CacheInvalidationInterval;
            _factionMembersCache.Clear();
            _memberDataCache.Clear();
        }

        else if (!hasOpenUi)
            _cacheInvalidationTimer = 0;
    }

    /// <summary>
    /// Получает кэшированный список членов фракции.
    /// </summary>
    private List<EntityUid> GetFactionMembers(string faction)
    {
        if (_factionMembersCache.TryGetValue(faction, out var cached))
            return cached;

        var members = new List<EntityUid>();
        var query = EntityQueryEnumerator<HullrotFactionComponent>();
        while (query.MoveNext(out var uid, out var factionComp))
        {
            if (factionComp.Faction == faction)
                members.Add(uid);
        }

        _factionMembersCache[faction] = members;
        return members;
    }

    /// <summary>
    /// Определяет статус участника (жив/мёртв/SSD).
    /// </summary>
    private OverwatchMemberStatus GetMemberStatus(EntityUid member)
    {
        if (!TryComp<MobStateComponent>(member, out var mobState))
            return OverwatchMemberStatus.Dead;

        if (_mobStateSystem.IsDead(member, mobState))
            return OverwatchMemberStatus.Dead;

        if (!TryComp<ActorComponent>(member, out var actor) || actor.PlayerSession == null)
            return OverwatchMemberStatus.SSD;

        return OverwatchMemberStatus.Alive;
    }



    /// <summary>
    /// Получает должность сущности.
    /// </summary>
    private string GetJobTitle(EntityUid entity)
    {
        if (TryComp<IdCardComponent>(entity, out var idCard) && !string.IsNullOrEmpty(idCard.LocalizedJobTitle))
            return idCard.LocalizedJobTitle;

        if (_idCardSystem.TryFindIdCard(entity, out var foundIdCard) && !string.IsNullOrEmpty(foundIdCard.Comp.LocalizedJobTitle))
            return foundIdCard.Comp.LocalizedJobTitle;

        return Loc.GetString("overwatch-job-title-unknown");
    }

    /// <summary>
    /// Получает ID отряда сущности.
    /// </summary>
    private int? GetSquadId(EntityUid entity)
    {
        if (TryComp<SquadComponent>(entity, out var squadComp))
            return squadComp.SquadId;

        return null;
    }

    /// <summary>
    /// Получает название отряда сущности.
    /// </summary>
    private string GetSquadName(EntityUid entity)
    {
        if (TryComp<SquadComponent>(entity, out var squadComp) && !string.IsNullOrEmpty(squadComp.SquadName))
            return squadComp.SquadName;

        return "";
    }



    /// <summary>
    /// Получает название Overwatch для фракции.
    /// </summary>
    private string GetOverwatchTitle(string faction)
    {
        var key = faction switch
        {
            "DSM" => "overwatch-title-dsm",
            "NCWL" => "overwatch-title-ncwl",
            "SHI" => "overwatch-title-shi",
            "TAP" => "overwatch-title-tap",
            "IPM" => "overwatch-title-ipm",
            "SAW" => "overwatch-title-saw",
            "GSC" => "overwatch-title-gsc",
            "CD" => "overwatch-title-cd",
            "SRM" => "overwatch-title-srm",
            _ => "overwatch-title-default"
        };

        return Loc.GetString(key);
    }

    /// <summary>
    /// Получает цвет Overwatch для фракции.
    /// </summary>
    private Color GetOverwatchColor(string faction)
    {
        return faction switch
        {
            "DSM" => Color.FromHex("#8A00C2"),
            "NCWL" => Color.FromHex("#cf8e00"),
            "SHI" => Color.FromHex("#666d66"),
            "TAP" => Color.FromHex("#009c08"),
            "IPM" => Color.FromHex("#9b0000"),
            "SAW" => Color.FromHex("#9b0000"),
            "GSC" => Color.FromHex("#9b0000"),
            "CD" => Color.FromHex("#9b0000"),
            "SRM" => Color.FromHex("#015124"),
            _ => Color.DarkGray
        };
    }

    /// <summary>
    /// Получает мировые координаты сущности.
    /// </summary>
    private Vector2? GetMemberCoordinates(EntityUid entity)
    {
        var worldPos = _transformSystem.GetWorldPosition(entity);
        return new Vector2(worldPos.X, worldPos.Y);
    }
}
