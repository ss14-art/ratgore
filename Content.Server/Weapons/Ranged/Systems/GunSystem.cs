using System.Linq;
using Content.Shared.Physics;
using System.Numerics;
using Content.Server.Cargo.Systems;
using Content.Shared.Contests;
using Content.Server.Power.EntitySystems;
using Content.Shared.Damage.Systems;
using Content.Shared.Effects;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Containers;
using Content.Shared._Lavaland.Weapons.Ranged.Events;

namespace Content.Server.Weapons.Ranged.Systems;

public sealed partial class GunSystem : SharedGunSystem
{
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly DamageExamineSystem _damageExamine = default!;
    [Dependency] private readonly PricingSystem _pricing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BallisticAmmoProviderComponent, PriceCalculationEvent>(OnBallisticPrice);
    }

    private void OnBallisticPrice(EntityUid uid, BallisticAmmoProviderComponent component, ref PriceCalculationEvent args)
    {
        if (string.IsNullOrEmpty(component.Proto) || component.UnspawnedCount == 0)
            return;

        if (!ProtoManager.TryIndex<EntityPrototype>(component.Proto, out var proto))
        {
            Log.Error($"Unable to find fill prototype for price on {component.Proto} on {ToPrettyString(uid)}");
            return;
        }

        // Probably good enough for most.
        var price = _pricing.GetEstimatedPrice(proto);
        args.Price += price * component.UnspawnedCount;
    }

    protected override void Popup(string message, EntityUid? uid, EntityUid? user) { }

    protected override void CreateEffect(EntityUid gunUid, MuzzleFlashEvent message, EntityUid? user = null, EntityUid? player = null)
    {
        var filter = Filter.Pvs(gunUid, entityManager: EntityManager);

        if (TryComp<ActorComponent>(user, out var actor))
            filter.RemovePlayer(actor.PlayerSession);

        if (GunPrediction && TryComp(player, out actor))
            filter.RemovePlayer(actor.PlayerSession);

        RaiseNetworkEvent(message, filter);
    }

    // TODO: Pseudo RNG so the client can predict these.
    protected override void FireHitscan(EntityUid gunUid, GunComponent gun, HitscanPrototype hitscan, EntityCoordinates fromCoordinates, EntityCoordinates toCoordinates, EntityUid? user = null)
    {
        var fromMap = fromCoordinates.ToMap(EntityManager, TransformSystem);
        var toMap = toCoordinates.ToMap(EntityManager, TransformSystem);
        var direction = (toMap.Position - fromMap.Position).Normalized();

        if (direction == Vector2.Zero)
            direction = Vector2.UnitX;

        var ray = new CollisionRay(fromMap.Position, direction, (int) (CollisionGroup.BulletImpassable | CollisionGroup.Opaque));
        var results = Physics.IntersectRay(fromMap.MapId, ray, hitscan.MaxLength, user, false).ToList();

        EntityUid? hitEntity = null;
        float distance = hitscan.MaxLength;

        if (results.Count > 0)
        {
            var result = results[0];
            hitEntity = result.HitEntity;
            distance = result.Distance;

            if (hitEntity != null)
            {
                var damage = hitscan.Damage;
                if (damage != null)
                {
                    Damageable.TryChangeDamage(hitEntity.Value, damage, origin: user);
                }
            }
        }

        FireEffects(fromCoordinates, distance, direction.ToAngle(), hitscan, hitEntity);
    }

    #region Hitscan effects

    private void FireEffects(EntityCoordinates fromCoordinates, float distance, Angle mapDirection, HitscanPrototype hitscan, EntityUid? hitEntity = null)
    {
        // Lord
        // Forgive me for the shitcode I am about to do
        // Effects tempt me not
        var sprites = new List<(NetCoordinates coordinates, Angle angle, SpriteSpecifier sprite, float scale)>();
        var gridUid = fromCoordinates.GetGridUid(EntityManager);
        var angle = mapDirection;

        // We'll get the effects relative to the grid / map of the firer
        // Look you could probably optimise this a bit with redundant transforms at this point.
        var xformQuery = GetEntityQuery<TransformComponent>();

        if (xformQuery.TryGetComponent(gridUid, out var gridXform))
        {
            var (_, gridRot, gridInvMatrix) = TransformSystem.GetWorldPositionRotationInvMatrix(gridXform, xformQuery);

            fromCoordinates = new EntityCoordinates(gridUid.Value,
                Vector2.Transform(fromCoordinates.ToMapPos(EntityManager, TransformSystem), gridInvMatrix));

            // Use the fallback angle I guess?
            angle -= gridRot;
        }

        if (distance >= 1f)
        {
            if (hitscan.MuzzleFlash != null)
            {
                var coords = fromCoordinates.Offset(angle.ToVec().Normalized() / 2);
                var netCoords = GetNetCoordinates(coords);

                sprites.Add((netCoords, angle, hitscan.MuzzleFlash, 1f));
            }

            if (hitscan.TravelFlash != null)
            {
                var coords = fromCoordinates.Offset(angle.ToVec() * (distance + 0.5f) / 2);
                var netCoords = GetNetCoordinates(coords);

                sprites.Add((netCoords, angle, hitscan.TravelFlash, distance - 1.5f));
            }
        }

        if (hitscan.ImpactFlash != null)
        {
            var coords = fromCoordinates.Offset(angle.ToVec() * distance);
            var netCoords = GetNetCoordinates(coords);

            sprites.Add((netCoords, angle.FlipPositive(), hitscan.ImpactFlash, 1f));
        }

        if (sprites.Count > 0)
        {
            RaiseNetworkEvent(new HitscanEvent
            {
                Sprites = sprites,
            }, Filter.Pvs(fromCoordinates, entityMan: EntityManager));
        }
    }

    #endregion
}
