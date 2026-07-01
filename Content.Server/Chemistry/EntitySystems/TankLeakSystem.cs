using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Popups;

namespace Content.Server.Chemistry.EntitySystems;

public sealed class TankLeakSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private const float LeakInterval = 1f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TankLeakComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, TankLeakComponent comp, ComponentStartup args)
    {
        comp.LeakAccumulator = 0f;
    }

    public void AddLeak(EntityUid uid, TankLeakComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            comp = EnsureComp<TankLeakComponent>(uid);

        comp.LeakCount++;
        Dirty(uid, comp);

        _popup.PopupEntity(
            Loc.GetString("tank-leak-popup", ("entity", uid)),
            uid,
            PopupType.SmallCaution);

        if (comp.LeakCount >= comp.MaxLeaks)
        {
            _popup.PopupEntity(
                Loc.GetString("tank-leak-destroy-popup", ("entity", uid)),
                uid,
                PopupType.MediumCaution);

            if (_solutionContainer.TryGetSolution(uid, comp.Solution, out _, out var solution))
            {
                var coords = Transform(uid).Coordinates;
                _puddle.TrySpillAt(coords, solution, out _);
            }

            QueueDel(uid);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TankLeakComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.LeakCount <= 0)
                continue;

            comp.LeakAccumulator += frameTime;
            if (comp.LeakAccumulator < LeakInterval)
                continue;

            comp.LeakAccumulator -= LeakInterval;

            var leakAmount = comp.LeakRatePerHole * comp.LeakCount;

            if (!_solutionContainer.TryGetSolution(uid, comp.Solution, out var soln, out var solution))
                continue;

            if (solution.Volume <= 0)
                continue;

            if (leakAmount > solution.Volume)
                leakAmount = solution.Volume;

            var leaked = _solutionContainer.SplitSolution(soln.Value, leakAmount);
            if (leaked.Volume <= 0)
                continue;

            var coords = Transform(uid).Coordinates;
            _puddle.TrySpillAt(coords, leaked, out _, sound: false);
        }
    }
}
