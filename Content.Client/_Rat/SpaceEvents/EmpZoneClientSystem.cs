using System.Numerics;
using Content.Shared._Rat.SpaceEvents;
using Content.Shared.GameTicking;
using Robust.Client.Graphics;
using Robust.Shared.Player;

namespace Content.Client._Rat.SpaceEvents;

public sealed class EmpZoneClientSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public bool ZoneActive { get; private set; } = false;
    public Vector2 ZoneCenter { get; private set; } = Vector2.Zero;
    public float ZoneRadius { get; private set; } = 500f;

    private EmpZoneScreenOverlay _overlay = default!;
    private bool _playerInZone = false;

    private float _checkTimer = 0f;
    private const float CheckInterval = 5f;

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new EmpZoneScreenOverlay();
        SubscribeNetworkEvent<EmpZoneActivatedEvent>(OnZoneActivated);
        SubscribeNetworkEvent<EmpZoneDeactivatedEvent>(OnZoneDeactivated);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnZoneActivated(EmpZoneActivatedEvent ev)
    {
        ZoneActive = true;
        ZoneCenter = ev.Center;
        ZoneRadius = ev.Radius;
        _checkTimer = CheckInterval;
    }

    private void OnZoneDeactivated(EmpZoneDeactivatedEvent ev)
    {
        ZoneActive = false;
        RemoveOverlayIfNeeded();
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        ZoneActive = false;
        RemoveOverlayIfNeeded();
    }

    private void RemoveOverlayIfNeeded()
    {
        if (!_playerInZone)
            return;
        _playerInZone = false;
        _overlayMan.RemoveOverlay(_overlay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!ZoneActive)
            return;

        _checkTimer += frameTime;
        if (_checkTimer < CheckInterval)
            return;
        _checkTimer = 0f;

        var player = _playerMan.LocalEntity;
        if (player == null)
        {
            RemoveOverlayIfNeeded();
            return;
        }

        var worldPos = _transform.GetWorldPosition(player.Value);
        var dist = (worldPos - ZoneCenter).Length();
        var inZone = dist <= ZoneRadius;

        if (inZone && !_playerInZone)
        {
            _playerInZone = true;
            _overlayMan.AddOverlay(_overlay);
        }
        else if (!inZone && _playerInZone)
        {
            _playerInZone = false;
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}