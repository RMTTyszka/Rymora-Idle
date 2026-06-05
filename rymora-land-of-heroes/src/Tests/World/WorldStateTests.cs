using RymoraLandOfHeroes.Core.Tests.ObjectMothers;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;

namespace RymoraLandOfHeroes.Core.Tests.World;

public sealed class WorldStateTests
{
    [Fact]
    public void FindPath_avoids_wall_tiles()
    {
        var scenario = WorldObjectMother.WorldWithWallBetweenOriginAndDestination();

        var path = scenario.InputWorld.FindPath(scenario.InputFrom, scenario.InputTo);

        Assert.DoesNotContain(scenario.ExpectedMissingTile, path);
    }

    [Fact]
    public void FindPath_returns_destination_as_last_waypoint()
    {
        var scenario = WorldObjectMother.WorldWithWallBetweenOriginAndDestination();

        var path = scenario.InputWorld.FindPath(scenario.InputFrom, scenario.InputTo);

        Assert.Equal(scenario.ExpectedDestination, path[^1]);
    }

    [Fact]
    public void FindNearestSafeSpot_returns_nearest_safe_region()
    {
        var scenario = WorldObjectMother.WorldWithSafeSpot();

        var safeSpot = scenario.InputWorld.FindNearestSafeSpot(scenario.InputPosition);

        Assert.Equal(scenario.ExpectedRegionName, safeSpot.Name);
    }

    [Fact]
    public void ShouldTriggerEncounter_uses_additive_terrain_modifier()
    {
        var scenario = WorldObjectMother.WorldWithEncounterModifier();

        var result = scenario.InputWorld.ShouldTriggerEncounter(
            scenario.InputPosition,
            scenario.InputBaseProbability,
            scenario.InputPolicy);

        Assert.Equal(scenario.ExpectedResult, result);
    }

    [Fact]
    public void ShouldTriggerEncounter_returns_false_on_safe_spot()
    {
        var scenario = WorldObjectMother.SafeSpotTileWithEncounterChance();

        var result = scenario.InputWorld.ShouldTriggerEncounter(
            scenario.InputPosition,
            scenario.InputBaseProbability,
            scenario.InputPolicy);

        Assert.Equal(scenario.ExpectedResult, result);
    }

    [Fact]
    public void ShouldTriggerEncounter_uses_region_modifier()
    {
        var scenario = WorldObjectMother.WorldWithRegionEncounterModifier();

        var result = scenario.InputWorld.ShouldTriggerEncounter(
            scenario.InputPosition,
            scenario.InputBaseProbability,
            scenario.InputPolicy);

        Assert.Equal(scenario.ExpectedResult, result);
    }

    [Fact]
    public void ShouldTriggerEncounter_uses_zone_modifier()
    {
        var scenario = WorldObjectMother.WorldWithZoneEncounterModifier();

        var result = scenario.InputWorld.ShouldTriggerEncounter(
            scenario.InputPosition,
            scenario.InputBaseProbability,
            scenario.InputPolicy);

        Assert.Equal(scenario.ExpectedResult, result);
    }

    [Fact]
    public void ShouldTriggerEncounter_clamps_large_probability_after_modifiers()
    {
        var scenario = WorldObjectMother.WorldWithEncounterModifier();

        var result = scenario.InputWorld.ShouldTriggerEncounter(
            scenario.InputPosition,
            baseProbability: 150,
            scenario.InputPolicy);

        Assert.True(result);
    }

    [Fact]
    public void ShouldTriggerEncounter_supports_fractional_probability()
    {
        var world = TestObjectMother.CreateWorld(random: new SequenceRandomSource(1050));

        var result = world.ShouldTriggerEncounter(
            new TilePosition(0, 0),
            baseProbability: 10.5f,
            new EncounterPolicy(EncounterModifierMode.None));

        Assert.True(result);
    }

    [Fact]
    public void GetZone_returns_zone_level()
    {
        var scenario = WorldObjectMother.WorldWithZoneLevel();

        var zone = scenario.InputWorld.GetZone(scenario.InputPosition);

        Assert.Equal(scenario.ExpectedLevel, zone.Level);
    }
}
