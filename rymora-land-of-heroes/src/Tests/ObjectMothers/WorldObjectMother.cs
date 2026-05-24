using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;
using RymoraLandOfHeroes.Core.World;

namespace RymoraLandOfHeroes.Core.Tests.ObjectMothers;

internal static class WorldObjectMother
{
    public static FindPathScenario WorldWithWallBetweenOriginAndDestination()
    {
        var wall = new TilePosition(1, 0);

        return new FindPathScenario(
            InputWorld: TestObjectMother.CreateWorld(
                random: new SequenceRandomSource(1),
                wallPositions: new[] { wall }),
            InputFrom: new TilePosition(0, 0),
            InputTo: new TilePosition(2, 0),
            ExpectedMissingTile: wall,
            ExpectedDestination: new TilePosition(2, 0));
    }

    public static FindNearestSafeSpotScenario WorldWithSafeSpot()
    {
        return new FindNearestSafeSpotScenario(
            InputWorld: TestObjectMother.CreateWorld(random: new SequenceRandomSource(1)),
            InputPosition: new TilePosition(0, 0),
            ExpectedRegionName: "Safe");
    }

    public static EncounterModifierScenario WorldWithEncounterModifier()
    {
        return new EncounterModifierScenario(
            InputWorld: TestObjectMother.CreateWorld(random: new SequenceRandomSource(20)),
            InputPosition: new TilePosition(0, 0),
            InputBaseProbability: 10,
            InputPolicy: new EncounterPolicy(EncounterModifierMode.AddToProbability),
            ExpectedResult: true);
    }

    public static SafeSpotEncounterScenario SafeSpotTileWithEncounterChance()
    {
        return new SafeSpotEncounterScenario(
            InputWorld: TestObjectMother.CreateWorld(random: new SequenceRandomSource(1)),
            InputPosition: new TilePosition(3, 0),
            InputBaseProbability: 100,
            InputPolicy: new EncounterPolicy(EncounterModifierMode.AddToProbability),
            ExpectedResult: false);
    }

    public static EncounterModifierScenario WorldWithRegionEncounterModifier()
    {
        return new EncounterModifierScenario(
            InputWorld: TestObjectMother.CreateWorld(
                random: new SequenceRandomSource(20),
                regionEncounterModifier: 15),
            InputPosition: new TilePosition(0, 0),
            InputBaseProbability: 10,
            InputPolicy: new EncounterPolicy(EncounterModifierMode.None),
            ExpectedResult: true);
    }

    public static EncounterModifierScenario WorldWithZoneEncounterModifier()
    {
        return new EncounterModifierScenario(
            InputWorld: TestObjectMother.CreateWorld(
                random: new SequenceRandomSource(20),
                zoneEncounterModifier: 15),
            InputPosition: new TilePosition(0, 0),
            InputBaseProbability: 10,
            InputPolicy: new EncounterPolicy(EncounterModifierMode.None),
            ExpectedResult: true);
    }

    public static ZoneScenario WorldWithZoneLevel()
    {
        return new ZoneScenario(
            InputWorld: TestObjectMother.CreateWorld(random: new SequenceRandomSource(1)),
            InputPosition: new TilePosition(0, 0),
            ExpectedLevel: 1);
    }
}

internal sealed record FindPathScenario(
    WorldState InputWorld,
    TilePosition InputFrom,
    TilePosition InputTo,
    TilePosition ExpectedMissingTile,
    TilePosition ExpectedDestination);

internal sealed record FindNearestSafeSpotScenario(WorldState InputWorld, TilePosition InputPosition, string ExpectedRegionName);

internal sealed record EncounterModifierScenario(
    WorldState InputWorld,
    TilePosition InputPosition,
    float InputBaseProbability,
    EncounterPolicy InputPolicy,
    bool ExpectedResult);

internal sealed record SafeSpotEncounterScenario(
    WorldState InputWorld,
    TilePosition InputPosition,
    float InputBaseProbability,
    EncounterPolicy InputPolicy,
    bool ExpectedResult);

internal sealed record ZoneScenario(WorldState InputWorld, TilePosition InputPosition, int ExpectedLevel);
