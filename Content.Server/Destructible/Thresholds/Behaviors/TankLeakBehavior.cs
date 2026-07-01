using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Components;
using JetBrains.Annotations;

namespace Content.Server.Destructible.Thresholds.Behaviors;

[UsedImplicitly]
[DataDefinition]
public sealed partial class TankLeakBehavior : IThresholdBehavior
{
    [DataField]
    public int LeaksToAdd = 1;

    [DataField]
    public string Solution = "tank";

    public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
    {
        var tankLeakSystem = system.EntityManager.System<TankLeakSystem>();

        var comp = system.EntityManager.EnsureComponent<TankLeakComponent>(owner);
        comp.Solution = Solution;

        for (var i = 0; i < LeaksToAdd; i++)
        {
            tankLeakSystem.AddLeak(owner, comp);
        }
    }
}
