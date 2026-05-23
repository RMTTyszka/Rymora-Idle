using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;
using RymoraLandOfHeroes.Core.Content;

namespace RymoraLandOfHeroes.Core.World;

public sealed class WorldState
{
    private static readonly TilePosition[] NeighborOffsets =
    {
        new(1, 0),
        new(1, -1),
        new(0, -1),
        new(-1, 0),
        new(-1, 1),
        new(0, 1)
    };

    private readonly Dictionary<TilePosition, WorldTile> _tiles;
    private readonly IRandomSource _randomSource;

    public WorldState(IEnumerable<WorldTile> tiles, IRandomSource? randomSource = null)
    {
        _tiles = new Dictionary<TilePosition, WorldTile>();
        foreach (var tile in tiles)
        {
            if (!_tiles.TryAdd(tile.Position, tile))
            {
                throw new ArgumentException($"Duplicate world tile at {tile.Position}.", nameof(tiles));
            }
        }

        _randomSource = randomSource ?? new SystemRandomSource();
    }

    public TerrainData GetTerrain(TilePosition position) => GetTile(position).Terrain;

    public RegionData GetRegion(TilePosition position) => GetTile(position).Region;

    public bool IsWalkable(TilePosition position)
    {
        return _tiles.TryGetValue(position, out var tile) && tile.Terrain.IsWalkable;
    }

    public IReadOnlyList<TilePosition> FindPath(TilePosition from, TilePosition to)
    {
        if (from == to)
        {
            return Array.Empty<TilePosition>();
        }

        if (!IsWalkable(from) || !IsWalkable(to))
        {
            return Array.Empty<TilePosition>();
        }

        var open = new PriorityQueue<TilePosition, int>();
        var cameFrom = new Dictionary<TilePosition, TilePosition>();
        var gScore = new Dictionary<TilePosition, int>
        {
            [from] = 0
        };
        var closed = new HashSet<TilePosition>();

        open.Enqueue(from, HexDistance(from, to));

        while (open.TryDequeue(out var current, out _))
        {
            if (current == to)
            {
                return ReconstructPath(from, to, cameFrom);
            }

            if (!closed.Add(current))
            {
                continue;
            }

            foreach (var neighbor in GetWalkableNeighbors(current))
            {
                var tentativeScore = gScore[current] + 1;
                if (gScore.TryGetValue(neighbor, out var knownScore) && tentativeScore >= knownScore)
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeScore;
                open.Enqueue(neighbor, tentativeScore + HexDistance(neighbor, to));
            }
        }

        return Array.Empty<TilePosition>();
    }

    public RegionData FindNearestSafeSpot(TilePosition position)
    {
        return GetNearestSafeSpotTile(position).Region;
    }

    public TilePosition FindNearestSafeSpotPosition(TilePosition position)
    {
        return GetNearestSafeSpotTile(position).Position;
    }

    private WorldTile GetNearestSafeSpotTile(TilePosition position)
    {
        var safeTile = _tiles.Values
            .Where(tile => tile.Region.IsSafeSpot)
            .OrderBy(tile => HexDistance(position, tile.Position))
            .ThenBy(tile => tile.Position.X)
            .ThenBy(tile => tile.Position.Y)
            .FirstOrDefault();

        return safeTile ?? throw new InvalidOperationException("World has no safe spot region.");
    }

    public EncounterTemplate SelectRandomEncounter(TilePosition position)
    {
        var encounters = GetRegion(position).Encounters;
        if (encounters.Count == 0)
        {
            throw new InvalidOperationException($"Region at {position} has no encounters.");
        }

        return encounters[_randomSource.NextInclusive(0, encounters.Count - 1)];
    }

    public bool ShouldTriggerEncounter(TilePosition position, float baseProbability, EncounterPolicy policy)
    {
        var tile = GetTile(position);
        if (tile.Terrain.IsCity || tile.Region.IsSafeSpot || tile.Region.Encounters.Count == 0)
        {
            return false;
        }

        var effectiveProbability = ApplyEncounterModifier(baseProbability, tile.Terrain.EncounterRateModifier, policy);
        effectiveProbability = Math.Clamp(effectiveProbability, 0, 100);
        if (effectiveProbability <= 0)
        {
            return false;
        }

        return _randomSource.NextInclusive(1, 100) <= effectiveProbability;
    }

    public static IReadOnlyList<TilePosition> GetNeighbors(TilePosition position)
    {
        return NeighborOffsets
            .Select(offset => new TilePosition(position.X + offset.X, position.Y + offset.Y))
            .ToArray();
    }

    public static int HexDistance(TilePosition from, TilePosition to)
    {
        var dx = from.X - to.X;
        var dy = from.Y - to.Y;
        return (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dx + dy)) / 2;
    }

    private WorldTile GetTile(TilePosition position)
    {
        if (!_tiles.TryGetValue(position, out var tile))
        {
            throw new KeyNotFoundException($"World tile not found at {position}.");
        }

        return tile;
    }

    private IEnumerable<TilePosition> GetWalkableNeighbors(TilePosition position)
    {
        return GetNeighbors(position).Where(IsWalkable);
    }

    private static IReadOnlyList<TilePosition> ReconstructPath(
        TilePosition from,
        TilePosition to,
        IReadOnlyDictionary<TilePosition, TilePosition> cameFrom)
    {
        var path = new List<TilePosition>();
        var current = to;

        while (current != from)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }

    private static float ApplyEncounterModifier(
        float baseProbability,
        int terrainModifier,
        EncounterPolicy policy)
    {
        return policy.TerrainModifierMode switch
        {
            EncounterModifierMode.None => baseProbability,
            EncounterModifierMode.AddToProbability => baseProbability + terrainModifier,
            EncounterModifierMode.SubtractFromProbability => baseProbability - terrainModifier,
            _ => throw new ArgumentOutOfRangeException(nameof(policy), "Unknown encounter modifier mode.")
        };
    }
}
