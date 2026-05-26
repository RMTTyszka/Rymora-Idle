using RymoraLandOfHeroes.Core.Application;
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

    [Fact]
    public void Automated_map_action_with_dead_hero_sets_runner_error()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgramWithDeadHero();

        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(1);

        Assert.Equal(ProgramRunnerState.Error, scenario.AssertParty.Automation.Runner.State);
        Assert.Contains("cannot run map actions", scenario.AssertParty.Automation.Runner.ErrorMessage);
    }

    [Fact]
    public void Application_records_macro_action_without_moving_party()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();
        var start = scenario.AssertParty.Position;

        scenario.InputApplication.StartRecordingMacro("party-1");
        scenario.InputApplication.RecordGatherAction(
            "party-1",
            new TilePosition(1, 0),
            MacroActionKind.Mine,
            "Iron",
            itemLevel: 1,
            itemWeight: 3);
        scenario.InputApplication.SaveRecordingMacro("party-1", "Mine Iron");

        Assert.Equal(start, scenario.AssertParty.Position);
        Assert.Contains(scenario.AssertParty.Automation.Macros, macro => macro.Name == "Mine Iron");
    }

    [Fact]
    public void Application_adds_macro_to_program()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();

        var step = scenario.InputApplication.AddMacroToProgram("party-1", "macro-1", RepeatPolicy.Count(2));

        Assert.Equal("macro-1", step.MacroId);
        Assert.Equal(2, step.Repeat.RepeatCount);
    }

    [Fact]
    public void Application_edits_macro_and_program_repeat()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();
        var step = scenario.AssertParty.Automation.Program.Steps[0];

        scenario.InputApplication.RenameMacro("party-1", "macro-1", "Iron Loop");
        scenario.InputApplication.SetGatherActionRepeat("party-1", "macro-1", "mine-1", RepeatPolicy.Count(4));
        scenario.InputApplication.SetProgramStepRepeat("party-1", step.Id, RepeatPolicy.Count(2));

        var macro = scenario.AssertParty.Automation.GetMacro("macro-1");
        var gather = Assert.IsType<GatherMacroAction>(macro.Actions[1]);
        Assert.Equal("Iron Loop", macro.Name);
        Assert.Equal(4, gather.Repeat.RepeatCount);
        Assert.Equal(2, scenario.AssertParty.Automation.Program.Steps[0].Repeat.RepeatCount);
    }

    [Fact]
    public void HandleInput_routes_recording_and_play_intents()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();

        scenario.InputApplication.HandleInput(new StartRecordingMacroIntent("party-1"));
        scenario.InputApplication.HandleInput(new RecordGatherActionIntent(
            "party-1",
            new TilePosition(1, 0),
            MacroActionKind.Mine,
            "Iron",
            1,
            3));
        scenario.InputApplication.HandleInput(new SaveRecordingMacroIntent("party-1", "Recorded"));
        scenario.InputApplication.HandleInput(new PlayProgramIntent("party-1"));

        Assert.Contains(scenario.AssertParty.Automation.Macros, macro => macro.Name == "Recorded");
        Assert.Equal(ProgramRunnerState.Running, scenario.AssertParty.Automation.Runner.State);
    }

    [Fact]
    public void HandleInput_routes_program_edit_intents()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();
        var originalStep = scenario.AssertParty.Automation.Program.Steps[0];

        scenario.InputApplication.HandleInput(new AddMacroToProgramIntent("party-1", "macro-1", RepeatPolicy.Count(2)));
        var addedStep = scenario.AssertParty.Automation.Program.Steps[1];
        scenario.InputApplication.HandleInput(new SetProgramRepeatIntent("party-1", RepeatPolicy.Count(5)));
        scenario.InputApplication.HandleInput(new SetProgramStepRepeatIntent("party-1", originalStep.Id, RepeatPolicy.Count(3)));
        scenario.InputApplication.HandleInput(new MoveProgramStepIntent("party-1", addedStep.Id, 0));
        scenario.InputApplication.HandleInput(new RemoveProgramStepIntent("party-1", addedStep.Id));

        Assert.Equal(5, scenario.AssertParty.Automation.Program.Repeat.RepeatCount);
        Assert.Single(scenario.AssertParty.Automation.Program.Steps);
        Assert.Equal(originalStep.Id, scenario.AssertParty.Automation.Program.Steps[0].Id);
        Assert.Equal(3, scenario.AssertParty.Automation.Program.Steps[0].Repeat.RepeatCount);
    }

    [Fact]
    public void HandleInput_routes_macro_edit_intents()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();

        scenario.InputApplication.HandleInput(new RenameMacroIntent("party-1", "macro-1", "Iron Loop"));
        scenario.InputApplication.HandleInput(new SetGatherActionRepeatIntent("party-1", "macro-1", "mine-1", RepeatPolicy.Count(4)));
        scenario.InputApplication.HandleInput(new MoveMacroActionIntent("party-1", "macro-1", "mine-1", 0));
        scenario.InputApplication.HandleInput(new RemoveMacroActionIntent("party-1", "macro-1", "move-1"));

        var macro = scenario.AssertParty.Automation.GetMacro("macro-1");
        var gather = Assert.IsType<GatherMacroAction>(Assert.Single(macro.Actions));
        Assert.Equal("Iron Loop", macro.Name);
        Assert.Equal("mine-1", gather.Id);
        Assert.Equal(4, gather.Repeat.RepeatCount);
    }
}
