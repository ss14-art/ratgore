using System.Numerics;  
using Content.Server.Announcements.Systems;  
using Content.Server.Emp;  
using Content.Server.GameTicking.Rules.Components;  
using Content.Server._Rat.SpaceEvents.Components;  
using Content.Shared._Rat.SpaceEvents;  
using Content.Shared.GameTicking.Components;  
using Robust.Shared.Map;  
using Robust.Shared.Player;  
using Robust.Shared.Random;  
using Content.Server.StationEvents.Events;
using Content.Shared.Ghost;

namespace Content.Server._Rat.SpaceEvents;

public sealed class EmpZoneRule : StationEventSystem<EmpZoneRuleComponent>
{
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    protected override void Added(EntityUid uid, EmpZoneRuleComponent component,
        GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        component.ZoneCenter = _random.NextVector2Box(
            component.MinX, component.MinY,
            component.MaxX, component.MaxY);

        ChatSystem.DispatchGlobalAnnouncement(
            Loc.GetString("emp-zone-event-warning",
                ("x", (int)component.ZoneCenter.X),
                ("y", (int)component.ZoneCenter.Y)),
            colorOverride: Color.Cyan);
    }

    protected override void Started(EntityUid uid, EmpZoneRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        component.ZoneAppearTime = Timing.CurTime + component.ZoneAppearDelay;
        component.NextEmpTime = component.ZoneAppearTime + component.EmpInterval;
    }

    protected override void Ended(EntityUid uid, EmpZoneRuleComponent component,
        GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        RaiseNetworkEvent(new EmpZoneDeactivatedEvent(), Filter.Broadcast());

        ChatSystem.DispatchGlobalAnnouncement(
            Loc.GetString("emp-zone-event-ended"),
            colorOverride: Color.Cyan);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<EmpZoneRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var comp, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                continue;

            if (!comp.ZoneActive && Timing.CurTime >= comp.ZoneAppearTime)
            {
                comp.ZoneActive = true;
                RaiseNetworkEvent(new EmpZoneActivatedEvent(
                    comp.ZoneCenter,
                    comp.ZoneRadius), Filter.Broadcast());
            }

            if (!comp.ZoneActive)
                continue;

            if (Timing.CurTime < comp.NextEmpTime)
                continue;

            comp.NextEmpTime = Timing.CurTime + comp.EmpInterval;

            foreach (var session in _playerManager.Sessions)
            {
                if (session.AttachedEntity == null)
                    continue;

                if (HasComp<GhostComponent>(session.AttachedEntity.Value))
                    continue;  

                var xform = Transform(session.AttachedEntity.Value);
                var worldPos = _transform.GetWorldPosition(xform);
                var dist = (worldPos - comp.ZoneCenter).Length();

                if (dist > comp.ZoneRadius)
                    continue;

                var mapCoords = new MapCoordinates(worldPos, xform.MapID);
                _emp.EmpPulse(mapCoords, comp.EmpRange, comp.EmpEnergyConsumption, comp.EmpDisableDuration);
            }
        }
    }
}