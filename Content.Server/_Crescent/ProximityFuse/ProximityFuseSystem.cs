using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Projectiles;
using System.Numerics;

namespace Content.Server._Crescent.ProximityFuse;

public sealed class ProximityFuseSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<ProximityFuseTargetComponent> _targetQuery;

    public override void Initialize()
    {
        base.Initialize();
        _xformQuery = GetEntityQuery<TransformComponent>();
        _targetQuery = GetEntityQuery<ProximityFuseTargetComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ProximityFuseComponent, ProjectileComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var comp, out var projectile, out var xform))
        {
            if (!_xformQuery.TryGetComponent(projectile.Shooter, out var shooterTransform))
                continue;

            if (comp.Safety > 0)
            {
                comp.Safety -= frameTime;
                continue;
            }

            var ourMapPos = _transform.ToMapCoordinates(xform.Coordinates).Position;
            var nearby = _lookup.GetEntitiesInRange(uid, comp.MaxRange, LookupFlags.Dynamic | LookupFlags.Sundries);

            List<EntityUid>? toRemove = null;
            foreach (var key in comp.Targets.Keys)
            {
                if (!nearby.Contains(key))
                    (toRemove ??= new()).Add(key);
            }
            if (toRemove != null)
                foreach (var key in toRemove)
                    comp.Targets.Remove(key);

            foreach (var near in nearby)
            {
                if (!_targetQuery.HasComponent(near))
                    continue;

                if (!_xformQuery.TryGetComponent(near, out var txform))
                    continue;

                if (shooterTransform.GridUid == txform.GridUid)
                    continue;

                var distance = Vector2.Distance(ourMapPos, _transform.ToMapCoordinates(txform.Coordinates).Position);

                if (comp.Targets.TryGetValue(near, out var lastDistance))
                {
                    comp.Targets[near] = distance;
                    if (distance > lastDistance)
                    {
                        Detonate(uid);
                        break; 
                    }
                }
                else
                {
                    comp.Targets[near] = distance;
                }
            }
        }
    }

    /// <summary>
    /// Explodes the entity if it has an explosive component, otherwise queues it for deletion.
    /// </summary>
    public void Detonate(EntityUid uid)
    {
        if (HasComp<ExplosiveComponent>(uid))
            _explosion.TriggerExplosive(uid);
        else
            QueueDel(uid); 
    }
}