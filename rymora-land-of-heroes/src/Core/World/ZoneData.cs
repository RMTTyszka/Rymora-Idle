namespace RymoraLandOfHeroes.Core.World;

public sealed record ZoneData(
    string Id,
    string Name,
    int Level,
    float EncounterProbabilityModifier);
