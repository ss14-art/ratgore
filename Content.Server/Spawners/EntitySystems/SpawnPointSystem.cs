using Content.Server.GameTicking;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server.Spawners.EntitySystems;

public sealed class SpawnPointSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawning);
    }

    private void OnPlayerSpawning(PlayerSpawningEvent args)
    {
        if (args.SpawnResult != null)
            return;

        // TODO: Cache all this if it ends up important.
        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new List<EntityCoordinates>();
        var factionLateJoinPositions = new List<EntityCoordinates>();
        var hasFactionLateJoinPositions = false;

        while ( points.MoveNext(out var uid, out var spawnPoint, out var xform))
        {
            if (args.Station != null && _stationSystem.GetOwningStation(uid, xform) != args.Station)
                continue;

            // Delta-V: Allow setting a desired SpawnPointType
            if (args.DesiredSpawnPointType != SpawnPointType.Unset)
            {
                var isMatchingJob = spawnPoint.SpawnType == SpawnPointType.Job &&
                    (args.Job == null || spawnPoint.Job?.ID == args.Job);

                switch (args.DesiredSpawnPointType)
                {
                    case SpawnPointType.Job when isMatchingJob:
                    case SpawnPointType.Observer when spawnPoint.SpawnType == SpawnPointType.Observer:
                        possiblePositions.Add(xform.Coordinates);
                        break;
                    case SpawnPointType.LateJoin when spawnPoint.SpawnType == SpawnPointType.LateJoin:
                        AddLateJoinPosition(uid, xform, possiblePositions, factionLateJoinPositions, args.HumanoidCharacterProfile?.Faction, ref hasFactionLateJoinPositions);
                        break;
                    default:
                        continue;
                }

                continue;
            }

            if (_gameTicker.RunLevel == GameRunLevel.InRound && spawnPoint.SpawnType == SpawnPointType.LateJoin)
            {
                AddLateJoinPosition(uid, xform, possiblePositions, factionLateJoinPositions, args.HumanoidCharacterProfile?.Faction, ref hasFactionLateJoinPositions);
            }

            if (_gameTicker.RunLevel != GameRunLevel.InRound &&
                spawnPoint.SpawnType == SpawnPointType.Job &&
                (args.Job == null || spawnPoint.Job is not null && spawnPoint.Job == args.Job))
            {
                possiblePositions.Add(xform.Coordinates);
            }
        }

        if (hasFactionLateJoinPositions && !string.IsNullOrEmpty(args.HumanoidCharacterProfile?.Faction))
            possiblePositions = factionLateJoinPositions;

        if (possiblePositions.Count == 0)
        {
            Log.Error($"No spawn points were available for {args.Job?.Id ?? "unknown job"}!");
            return;
        }

        var spawnLoc = _random.Pick(possiblePositions);

        args.SpawnResult = _stationSpawning.SpawnPlayerMob(
            spawnLoc,
            args.Job,
            args.HumanoidCharacterProfile,
            args.Station);
    }

    private void AddLateJoinPosition(
        EntityUid uid,
        TransformComponent xform,
        List<EntityCoordinates> possiblePositions,
        List<EntityCoordinates> factionLateJoinPositions,
        string? factionId,
        ref bool hasFactionLateJoinPositions)
    {
        if (!TryComp<FactionLateJoinSpawnPointComponent>(uid, out var factionSpawn))
        {
            possiblePositions.Add(xform.Coordinates);
            return;
        }

        hasFactionLateJoinPositions = true;

        if (!string.IsNullOrEmpty(factionId) && factionSpawn.Faction == factionId)
            factionLateJoinPositions.Add(xform.Coordinates);
    }
}
