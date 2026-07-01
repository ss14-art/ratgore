using System.Linq;
using System.Numerics;
using Content.Shared._Rat.CrewMonitoring;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Pinpointer;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._Rat.CrewMonitoring;

public sealed class RatCrewMonitorSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedIdCardSystem _idCardSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private const float UpdateRate = 2f;
    private float _updateDiff;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RatCrewMonitorComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    private void OnUIOpened(EntityUid uid, RatCrewMonitorComponent component, BoundUIOpenedEvent args)
    {
        UpdateUserInterface(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateDiff += frameTime;
        if (_updateDiff < UpdateRate)
            return;
        _updateDiff -= UpdateRate;

        var query = EntityQueryEnumerator<RatCrewMonitorComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_uiSystem.IsUiOpen(uid, RatCrewMonitorUiKey.Key))
            {
                UpdateUserInterface(uid, comp);
            }
        }
    }

    private void UpdateUserInterface(EntityUid uid, RatCrewMonitorComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_uiSystem.IsUiOpen(uid, RatCrewMonitorUiKey.Key))
            return;

        var xform = Transform(uid);
        if (xform.MapUid != null && TryComp<MapGridComponent>(xform.MapUid.Value, out _))
        {
            var mapUid = xform.MapUid.Value;
            var gridQuery = EntityQueryEnumerator<MapGridComponent>();
            while (gridQuery.MoveNext(out var gridId, out _))
            {
                if (Transform(gridId).MapUid == mapUid)
                    EnsureComp<NavMapComponent>(gridId);
            }
        }

        var sensors = GatherSensors(component);
        _uiSystem.SetUiState(uid, RatCrewMonitorUiKey.Key, new RatCrewMonitorState(sensors));
    }

    private List<SuitSensorStatus> GatherSensors(RatCrewMonitorComponent component)
    {
        var sensors = new List<SuitSensorStatus>();
        var xformQuery = GetEntityQuery<TransformComponent>();
        var trackerQuery = EntityQueryEnumerator<FactionTrackerComponent>();
        var seenEntities = new HashSet<NetEntity>();

        while (trackerQuery.MoveNext(out var uid, out var tracker))
        {
            if (component.Faction != null && tracker.Faction != component.Faction)
                continue;

            EntityUid personUid;
            MobStateComponent? mobState;

            if (TryComp(uid, out mobState))
            {
                personUid = uid;
            }
            else
            {
                var implantXform = xformQuery.GetComponent(uid);
                personUid = implantXform.ParentUid;
                if (personUid == uid || !TryComp(personUid, out mobState))
                    continue;
            }

            var personNetId = GetNetEntity(personUid);
            if (!seenEntities.Add(personNetId))
                continue;

            var userName = Loc.GetString("suit-sensor-component-unknown-name");
            var userJob = Loc.GetString("suit-sensor-component-unknown-job");
            var userJobIcon = "JobIconNoId";
            var userJobDepartments = new List<string>();

            if (_idCardSystem.TryFindIdCard(personUid, out var card))
            {
                if (card.Comp.FullName != null)
                    userName = card.Comp.FullName;
                if (card.Comp.LocalizedJobTitle != null)
                    userJob = card.Comp.LocalizedJobTitle;
                userJobIcon = card.Comp.JobIcon;
                foreach (var department in card.Comp.JobDepartments)
                    userJobDepartments.Add(Loc.GetString(department));
            }
            else
            {
                userName = MetaData(personUid).EntityName;
            }

            var isAlive = !_mobStateSystem.IsDead(personUid, mobState);

            var personXform = xformQuery.GetComponent(personUid);
            NetCoordinates? netCoords = null;
            if (personXform.MapUid != null)
            {
                var gridUid = personXform.GridUid ?? personXform.MapUid;
                var localPos = personXform.GridUid != null
                    ? Vector2.Transform(
                        _transformSystem.GetWorldPosition(personXform, xformQuery),
                        _transformSystem.GetInvWorldMatrix(xformQuery.GetComponent(personXform.GridUid.Value), xformQuery))
                    : _transformSystem.GetWorldPosition(personXform, xformQuery);
                netCoords = new NetCoordinates(GetNetEntity(gridUid.Value), localPos);
            }

            var suid = GetNetEntity(personUid);
            var status = new SuitSensorStatus(suid, userName, userJob, userJobIcon, userJobDepartments)
            {
                IsAlive = isAlive,
                Coordinates = netCoords,
                Timestamp = _gameTiming.CurTime,
                MobState = (byte)mobState.CurrentState
            };

            sensors.Add(status);
        }

        return sensors;
    }
}