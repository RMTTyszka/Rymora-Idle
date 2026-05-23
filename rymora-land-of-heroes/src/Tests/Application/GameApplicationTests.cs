using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

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
    public void Update_combat_victory_clears_active_combat()
    {
        var scenario = ApplicationObjectMother.ApplicationWithActiveWinningCombat();

        scenario.InputApplication.Update(scenario.InputDeltaTime);
        Assert.Equal(scenario.ExpectedActiveCombats, scenario.InputApplication.ActiveCombats.Count);
    }
}
