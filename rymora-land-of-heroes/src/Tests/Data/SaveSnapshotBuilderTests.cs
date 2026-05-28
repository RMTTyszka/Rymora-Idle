using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Data;
using RymoraLandOfHeroes.Core.Party;
using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Data;

public sealed class SaveSnapshotBuilderTests
{
    [Fact]
    public void CreateSaveData_saves_party_position_inventory_and_member_life()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMineActionQueued();
        scenario.InputApplication.SelectParty("party-1");
        scenario.InputApplication.Update(1);
        var hero = scenario.AssertParty.Leader!;
        hero.TakeDamage(10);

        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var party = Assert.Single(save.Parties);
        Assert.Equal(new TilePositionSaveData(0, 0), party.Position);
        Assert.Equal(1, party.InventoryItems.Single().Quantity);
        Assert.Equal(hero.Life, party.Members.Single().Life);
        Assert.Equal("party-1", save.SelectedPartyId);
    }

    [Fact]
    public void CreateSaveData_saves_current_queue_progress_and_pending_actions()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMineActionQueued();
        scenario.InputApplication.EnqueueAction("party-1", new PartyActionRequest(
            PartyActionType.CutWood,
            PartyActionEndType.ByCount,
            TimeToExecute: 3,
            LimitCount: 1,
            ItemName: "Oak",
            ItemLevel: 1,
            ItemWeight: 1));
        scenario.InputApplication.Update(0.5f);

        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var queue = save.Parties.Single().ActionQueue;
        Assert.Equal(0.5f, queue.Current!.CurrentTime);
        Assert.Equal("CutWood", queue.Pending.Single().ActionType);
    }

    [Fact]
    public void CreateSaveData_saves_automation_macros_program_and_runner_error()
    {
        var scenario = ApplicationObjectMother.ApplicationWithInvalidMiningProgram();
        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(1);

        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var automation = save.Parties.Single().Automation;
        Assert.Equal("macro-1", automation.Macros.Single().Id);
        Assert.Equal("macro-1", automation.Program.Steps.Single().MacroId);
        Assert.Equal("Error", automation.Runner.State);
        Assert.Contains("cannot mine", automation.Runner.ErrorMessage);
    }

    [Fact]
    public void CreateSaveData_saves_active_combat()
    {
        var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued(encounterProbability: 100);
        scenario.InputApplication.Update(1);

        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var combat = Assert.Single(save.ActiveCombats);
        Assert.Equal("party-1", combat.PartyId);
        Assert.NotEmpty(combat.Heroes);
        Assert.NotEmpty(combat.Monsters);
    }
}
