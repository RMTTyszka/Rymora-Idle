using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Application;

public sealed class GameApplicationAutomationTests
{
    [Fact]
    public void Update_program_moves_then_mines_and_finishes_idle()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();

        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(1);
        scenario.InputApplication.Update(1);
        scenario.InputApplication.Update(0);

        Assert.Equal(new TilePosition(1, 0), scenario.AssertParty.Position);
        Assert.Equal(1, scenario.AssertParty.Inventory.GetQuantity("Iron", 1));
        Assert.Equal(ProgramRunnerState.Idle, scenario.AssertParty.Automation.Runner.State);
    }

    [Fact]
    public void Update_invalid_program_action_sets_runner_error()
    {
        var scenario = ApplicationObjectMother.ApplicationWithInvalidMiningProgram();

        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(1);

        Assert.Equal(ProgramRunnerState.Error, scenario.AssertParty.Automation.Runner.State);
        Assert.Contains("cannot mine", scenario.AssertParty.Automation.Runner.ErrorMessage);
    }

    [Fact]
    public void Stop_program_finishes_current_action_then_resets()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();

        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(0.5f);
        scenario.InputApplication.StopProgram("party-1");
        scenario.InputApplication.Update(0.5f);

        Assert.Equal(ProgramRunnerState.Idle, scenario.AssertParty.Automation.Runner.State);
    }

    [Fact]
    public void Automated_map_action_without_living_hero_sets_runner_error()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgramWithoutHero();

        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(1);

        Assert.Equal(ProgramRunnerState.Error, scenario.AssertParty.Automation.Runner.State);
        Assert.Contains("cannot run map actions", scenario.AssertParty.Automation.Runner.ErrorMessage);
    }
}
