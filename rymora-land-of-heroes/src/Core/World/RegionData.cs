using RymoraLandOfHeroes.Core.Content;

namespace RymoraLandOfHeroes.Core.World;

public sealed record RegionData(
    string Id,
    string Name,
    bool IsSafeSpot,
    float EncounterProbabilityModifier,
    IReadOnlyDictionary<TerrainType, IReadOnlyList<EncounterTemplate>> EncountersByTerrain);
