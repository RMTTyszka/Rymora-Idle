using RymoraLandOfHeroes.Core.Tests.ObjectMothers;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Content;
using GameParty = RymoraLandOfHeroes.Core.Party.Party;

namespace RymoraLandOfHeroes.Core.Tests.Application;

public sealed class GameApplicationTests
{
    [Fact]
    public void Update_mine_adds_item_to_party_inventory()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMineActionQueued();

        scenario.InputApplication.Update(scenario.InputDeltaTime);

        Assert.Equal(
            scenario.ExpectedQuantity,
            scenario.AssertParty.Inventory.GetQuantity(scenario.AssertItemName, scenario.AssertItemLevel));
    }

    [Fact]
    public void Update_transfer_adds_item_to_target_party_inventory()
    {
        var scenario = ApplicationObjectMother.ApplicationWithTransferActionQueued();

        scenario.InputApplication.Update(scenario.InputDeltaTime);

        Assert.Equal(
            scenario.ExpectedQuantity,
            scenario.AssertTargetParty.Inventory.GetQuantity(scenario.AssertItemName, scenario.AssertItemLevel));
    }

    [Fact]
    public void Update_travel_moves_party_to_next_tile()
    {
        var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued();

        scenario.InputApplication.Update(scenario.InputDeltaTime);
        Assert.Equal(scenario.ExpectedPosition, scenario.AssertParty.Position);
    }

    [Fact]
    public void Update_travel_starts_combat_on_encounter()
    {
        var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued(encounterProbability: 100);

        scenario.InputApplication.Update(scenario.InputDeltaTime);
        Assert.Equal(scenario.ExpectedIsInCombat, scenario.AssertParty.IsInCombat);
    }

    [Fact]
    public void Update_travel_accumulates_encounter_time_when_no_encounter_happens()
    {
        var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued(
            encounterProbability: 5,
            encounterRampPerSecond: 10,
            encounterRampMaxBonus: 40,
            worldRandomRolls: new[] { 9000, 9000 },
            combatRandomRolls: new[] { 1, 1 },
            destination: new TilePosition(2, 0));

        scenario.InputApplication.Update(1);

        Assert.Equal(1f, scenario.AssertParty.EncounterState.TravelSecondsSinceReset);
        Assert.False(scenario.AssertParty.IsInCombat);
    }

    [Fact]
    public void Update_travel_starts_combat_after_ramp_increases_chance()
    {
        var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued(
            encounterProbability: 5,
            encounterRampPerSecond: 10,
            encounterRampMaxBonus: 40,
            worldRandomRolls: new[] { 1600, 2000, 0 },
            combatRandomRolls: new[] { 1, 1 },
            destination: new TilePosition(2, 0));

        scenario.InputApplication.Update(1);
        scenario.InputApplication.Update(1);

        Assert.True(scenario.AssertParty.IsInCombat);
        Assert.Equal(0f, scenario.AssertParty.EncounterState.TravelSecondsSinceReset);
    }

    [Fact]
    public void Update_travel_resets_encounter_time_on_safe_spot()
    {
        var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued(
            encounterProbability: 0,
            encounterRampPerSecond: 0,
            encounterRampMaxBonus: 0,
            worldRandomRolls: new[] { 100, 100, 100 },
            combatRandomRolls: new[] { 1, 1 },
            destination: new TilePosition(3, 0));
        scenario.AssertParty.EncounterState.AddTravelTime(4f);

        scenario.InputApplication.Update(1);
        scenario.InputApplication.Update(1);
        scenario.InputApplication.Update(1);

        Assert.Equal(new TilePosition(3, 0), scenario.AssertParty.Position);
        Assert.Equal(0f, scenario.AssertParty.EncounterState.TravelSecondsSinceReset);
    }

    [Fact]
    public void Update_defeated_party_resets_encounter_time_when_moved_to_safe_spot()
    {
        var party = new GameParty("party-1", new TilePosition(0, 0));
        var hero = TestObjectMother.CreateCreature("Hero");
        hero.TakeDamage(hero.MaxLife);
        party.AddMember(hero);
        party.EncounterState.AddTravelTime(4f);
        var application = new GameApplication(
            TestObjectMother.CreateWorld(new SequenceRandomSource(1)),
            new[] { party },
            TestObjectMother.CreateGameConfig(),
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        application.Update(1);

        Assert.Equal(new TilePosition(3, 0), party.Position);
        Assert.Equal(0f, party.EncounterState.TravelSecondsSinceReset);
    }

    [Fact]
    public void StartCombat_resets_party_encounter_time()
    {
        var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued();
        var encounter = new EncounterTemplate(
            "test-encounter",
            "Test Encounter",
            Level: 1,
            new[] { new CreatureTemplate("Goblin", MonsterClass.Combatant, new SpriteReference("goblin")) });
        scenario.AssertParty.EncounterState.AddTravelTime(4f);

        scenario.InputApplication.StartCombat(scenario.AssertParty.Id, encounter);

        Assert.Equal(0f, scenario.AssertParty.EncounterState.TravelSecondsSinceReset);
        Assert.True(scenario.AssertParty.IsInCombat);
    }

    [Theory]
    [InlineData(-1f, 0f, "EncounterRampPerSecond")]
    [InlineData(0f, -1f, "EncounterRampMaxBonus")]
    public void Constructor_throws_when_encounter_ramp_config_is_negative(
        float encounterRampPerSecond,
        float encounterRampMaxBonus,
        string expectedParameter)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => new GameApplication(
            TestObjectMother.CreateWorld(new SequenceRandomSource(1)),
            new[] { new GameParty("party-1", new TilePosition(0, 0)) },
            TestObjectMother.CreateGameConfig(
                encounterRampPerSecond: encounterRampPerSecond,
                encounterRampMaxBonus: encounterRampMaxBonus),
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1)));

        Assert.Equal(expectedParameter, error.ParamName);
    }

    [Fact]
    public void Update_combat_victory_clears_active_combat()
    {
        var scenario = ApplicationObjectMother.ApplicationWithActiveWinningCombat();

        scenario.InputApplication.Update(scenario.InputDeltaTime);
        Assert.Equal(scenario.ExpectedActiveCombats, scenario.InputApplication.ActiveCombats.Count);
    }
}
