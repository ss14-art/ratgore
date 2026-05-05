using System.Numerics;
using Content.Server.Chat.Systems;
using Content.Server.Power.Components;
using Content.Shared.Chat;
using Content.Shared.Crescent.Radar;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server.Crescent.Radar;

public sealed class SonarPingSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timer = default!;

    private float _curTime;
    private const float PingCheckInterval = 3f;
    private static readonly TimeSpan AlertCooldown = TimeSpan.FromSeconds(6);

    public override void Initialize()
    {
        SubscribeLocalEvent<RadarDetectorComponent, GetVerbsEvent<ActivationVerb>>(RequestVerbs);
    }

    private void RequestVerbs(EntityUid owner, RadarDetectorComponent comp, ref GetVerbsEvent<ActivationVerb> args)
    {
        args.Verbs.Add(new ActivationVerb
        {
            Text = Loc.GetString("sonar-ping-verb-toggle"),
            Act = () => comp.alertOnPing = !comp.alertOnPing,
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timer.IsFirstTimePredicted)
            return;

        _curTime += frameTime;

        if (_curTime < PingCheckInterval)
            return;

        _curTime = 0f;

        var worldTime = _timer.CurTime;

        var detectorQuery = EntityQueryEnumerator<RadarConsoleComponent, RadarDetectorComponent, TransformComponent, ApcPowerReceiverComponent>();

        while (detectorQuery.MoveNext(out var uid, out var radar, out var detector, out var detectorXform, out var power))
        {
            if (!power.Powered)
                continue;
            if (worldTime - detector.lastAlert < TimeSpan.Zero)
                continue;

            var ourPos = _transform.GetWorldPosition(detectorXform);
            var ourRangeSq = (float)(radar.MaxRange * radar.MaxRange * 0.64);

            var closestSq = float.MaxValue;
            var closestPos = Vector2.Zero;
            var count = 0;

            var pingerQuery = EntityQueryEnumerator<RadarPingerComponent, TransformComponent>();
            while (pingerQuery.MoveNext(out var pingerUid, out _, out var pingerXform))
            {
                if (!_uiSystem.IsUiOpen(pingerUid, RadarConsoleUiKey.Key))
                    continue;

                if (pingerXform.GridUid != null)
                    continue;

                var pingerPos = _transform.GetWorldPosition(pingerXform);
                var deltaSq = (pingerPos - ourPos).LengthSquared();

                if (deltaSq > ourRangeSq)
                    continue;

                count++;
                if (deltaSq < closestSq)
                {
                    closestSq = deltaSq;
                    closestPos = pingerPos;
                }
            }

            if (count == 0)
                continue;

            var azimuth = GetAzimuthDegrees(closestPos - ourPos);
            var distance = Math.Round(MathF.Sqrt(closestSq));

            var message = Loc.GetString("sonar-ping-alert-message",
                ("count", count),
                ("distance", distance),
                ("azimuth", azimuth));

            _chatSystem.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, ChatTransmitRange.Normal);
            detector.lastAlert = worldTime + AlertCooldown;
        }
    }

    private static int GetAzimuthDegrees(Vector2 delta)
    {
        var degrees = (Math.Atan2(delta.X, delta.Y) * (180.0 / Math.PI) + 360.0) % 360.0;
        return (int) Math.Round(degrees) % 360;
    }
}