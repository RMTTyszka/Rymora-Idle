namespace RymoraLandOfHeroes.Core.World;

public sealed record TerrainData(
    TerrainType Type,
    bool IsWalkable,
    float MoveSpeed,
    int Quality,
    int EncounterRateModifier,
    bool AllowsMining,
    bool AllowsWoodcutting,
    bool IsCity,
    bool IsPlace);
