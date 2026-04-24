using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Hands;

[TestFixture]
public sealed class HandTests : GameTest
{
    [TestPrototypes]
    private const string Prototypes = @"
- type: entity
  id: TestPickUpThenDropInContainerTestBox
  name: box
  components:
  - type: EntityStorage
  - type: ContainerContainer
    containers:
      entity_storage: !type:Container
- type: entity
  id: HandTestMob
  components:
  - type: Hands
    hands:
      left:
        location: Left
      right:
        location: Right
    sortedHands:
    - left
    - right
    activeHand: left
  - type: Transform
  - type: ContainerContainer
  - type: Physics
  - type: Fixtures
    fixtures:
      fix1:
        shape:
          !type:PhysShapeCircle
          radius: 0.35
  - type: DoAfter
  - type: ComplexInteraction
  - type: MindContainer
  - type: Stripping
  - type: Puller
  - type: UserInterface
  - type: CombatMode
";


    public override PoolSettings PoolSettings => new()
    {
        Connected = true,
        DummyTicker = true
    };

    [Test]
    public async Task TestPickupDrop()
    {
        var pair = Pair;
        var server = pair.Server;

        var entMan = server.ResolveDependency<IEntityManager>();
        var playerMan = server.ResolveDependency<IPlayerManager>();
        var mapSystem = server.System<SharedMapSystem>();
        var sys = entMan.System<SharedHandsSystem>();
        var tSys = entMan.System<TransformSystem>();

        var data = await pair.CreateTestMap();
        await pair.RunTicksSync(5);

        EntityUid item = default;
        EntityUid player = default;
        HandsComponent hands = default!;
        await server.WaitPost(() =>
        {
            var session = playerMan.Sessions.First();
            player = entMan.SpawnEntity("HandTestMob", data.GridCoords);
            playerMan.SetAttachedEntity(session, player);

            var xform = entMan.GetComponent<TransformComponent>(player);
            item = entMan.SpawnEntity("Crowbar", data.GridCoords);
            if (!entMan.TryGetComponent(player, out hands))
            {
                var comps = string.Join(", ", entMan.GetComponents(player).Select(c => c.GetType().Name));
                Assert.Fail($"Player entity {player} ({entMan.GetComponent<MetaDataComponent>(player).EntityName}) does not have HandsComponent! Components: {comps}");
            }
            sys.TryPickupByName(player, item, hands.ActiveHandId!);
        });

        // run ticks here is important, as errors may happen within the container system's frame update methods.
        await pair.RunTicksSync(5);
        Assert.That(sys.GetActiveItem((player, hands)), Is.EqualTo(item));

        await server.WaitPost(() =>
        {
            sys.TryDropEntity(player, item);
        });

        await pair.RunTicksSync(5);
        Assert.That(sys.GetActiveItem((player, hands)), Is.Null);

        await server.WaitPost(() => mapSystem.DeleteMap(data.MapId));
    }

    [Test]
    public async Task TestPickUpThenDropInContainer()
    {
        var pair = Pair;
        var server = pair.Server;
        var map = await pair.CreateTestMap();
        await pair.RunTicksSync(5);

        var entMan = server.ResolveDependency<IEntityManager>();
        var playerMan = server.ResolveDependency<IPlayerManager>();
        var mapSystem = server.System<SharedMapSystem>();
        var sys = entMan.System<SharedHandsSystem>();
        var tSys = entMan.System<TransformSystem>();
        var containerSystem = server.System<SharedContainerSystem>();

        EntityUid item = default;
        EntityUid box = default;
        EntityUid player = default;
        HandsComponent hands = default!;

        // spawn the elusive box and crowbar at the coordinates
        await server.WaitPost(() => box = server.EntMan.SpawnEntity("TestPickUpThenDropInContainerTestBox", map.GridCoords));
        await server.WaitPost(() => item = server.EntMan.SpawnEntity("Crowbar", map.GridCoords));
        // place the player at the exact same coordinates and have them grab the crowbar
        await server.WaitPost(() =>
        {
            var session = playerMan.Sessions.First();
            player = entMan.SpawnEntity("HandTestMob", map.GridCoords);
            playerMan.SetAttachedEntity(session, player);

            if (!entMan.TryGetComponent(player, out hands))
            {
                var comps = string.Join(", ", entMan.GetComponents(player).Select(c => c.GetType().Name));
                Assert.Fail($"Player entity {player} ({entMan.GetComponent<MetaDataComponent>(player).EntityName}) does not have HandsComponent! Components: {comps}");
            }
            sys.TryPickupByName(player, item, hands.ActiveHandId!);
        });
        await pair.RunTicksSync(10);
        Assert.That(sys.GetActiveItem((player, hands)), Is.EqualTo(item));

        // Open then close the box to place the player, who is holding the crowbar, inside of it
        await server.WaitPost(() =>
        {
            var container = containerSystem.EnsureContainer<Container>(box, "entity_storage");
            Assert.That(containerSystem.Insert(player, container), Is.True);
        });
        await pair.RunTicksSync(10);
        Assert.That(containerSystem.IsEntityInContainer(player), Is.True);

        // Dropping the item while the player is inside the box should cause the item
        // to also be inside the same container the player is in now,
        // with the item not being in the player's hands
        await server.WaitPost(() =>
        {
            containerSystem.TryGetContainingContainer(player, out var container);
            Assert.That(container, Is.Not.Null);
            sys.TryDropIntoContainer(player, item, container!);
        });
        await pair.RunTicksSync(10);
        var xform = entMan.GetComponent<TransformComponent>(player);
        var itemXform = entMan.GetComponent<TransformComponent>(item);
        Assert.That(sys.GetActiveItem((player, hands)), Is.Not.EqualTo(item));
        Assert.That(containerSystem.IsInSameOrNoContainer((player, xform), (item, itemXform)));

        await server.WaitPost(() => mapSystem.DeleteMap(map.MapId));
    }
}
