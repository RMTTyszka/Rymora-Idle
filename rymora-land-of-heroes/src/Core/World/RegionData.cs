using RymoraLandOfHeroes.Core.Content;

namespace RymoraLandOfHeroes.Core.World;

public sealed record RegionData(
    string Name,
    bool IsSafeSpot,
    IReadOnlyList<EncounterTemplate> Encounters);
