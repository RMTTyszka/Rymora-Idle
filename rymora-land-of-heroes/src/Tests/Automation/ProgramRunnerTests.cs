using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.Tests.Automation;

public sealed class ProgramRunnerTests
{
    [Fact]
    public void Play_emits_first_action_from_first_program_macro()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        automation.Runner.Play();
        var execution = automation.Runner.TryStartNextAction(automation);

        Assert.Equal(ProgramRunnerState.Running, automation.Runner.State);
        Assert.NotNull(execution);
        Assert.IsType<MoveToMacroAction>(execution!.Action);
    }

    [Fact]
    public void Edited_macro_applies_next_time_macro_starts()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Count(2));

        automation.Runner.Play();
        var firstMove = automation.Runner.TryStartNextAction(automation)!;
        automation.GetMacro("macro-1").AddAction(new MoveToMacroAction("move-late", new TilePosition(3, 0)));

        automation.Runner.CompleteAction(firstMove.ExecutionId, elapsedSeconds: 1);
        var firstMine = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.CompleteAction(firstMine.ExecutionId, elapsedSeconds: 1);
        var secondMove = automation.Runner.TryStartNextAction(automation)!;

        Assert.Equal("move-1", secondMove.Action.Id);
    }

    [Fact]
    public void Gather_action_repeat_count_repeats_action_before_next_macro_action()
    {
        var automation = new PartyAutomation();
        var macro = new PartyMacro("macro-1", "Mining");
        macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Count(2)));
        macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(2, 0)));
        automation.AddMacro(macro);
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        automation.Runner.Play();
        var firstMine = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.CompleteAction(firstMine.ExecutionId, elapsedSeconds: 1);
        var secondMine = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.CompleteAction(secondMine.ExecutionId, elapsedSeconds: 1);
        var move = automation.Runner.TryStartNextAction(automation)!;

        Assert.Equal("mine-1", firstMine.Action.Id);
        Assert.Equal("mine-1", secondMine.Action.Id);
        Assert.Equal("move-1", move.Action.Id);
    }

    [Fact]
    public void Program_repeat_count_restarts_sequence()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);
        automation.Program.SetProgramRepeat(RepeatPolicy.Count(2));

        automation.Runner.Play();
        CompleteNext(automation);
        CompleteNext(automation);
        var secondProgramMove = automation.Runner.TryStartNextAction(automation)!;

        Assert.Equal("move-1", secondProgramMove.Action.Id);
    }

    [Fact]
    public void Program_step_duration_repeats_macro_until_elapsed_time_is_reached()
    {
        var automation = new PartyAutomation();
        var macro = new PartyMacro("macro-1", "Patrol");
        macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(1, 0)));
        automation.AddMacro(macro);
        automation.Program.AddStep("macro-1", RepeatPolicy.Duration(2));

        automation.Runner.Play();
        CompleteNext(automation);
        var secondMove = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.CompleteAction(secondMove.ExecutionId, elapsedSeconds: 1);
        var finished = automation.Runner.TryStartNextAction(automation);

        Assert.Equal("move-1", secondMove.Action.Id);
        Assert.Null(finished);
        Assert.Equal(ProgramRunnerState.Idle, automation.Runner.State);
    }

    [Fact]
    public void Stop_waits_for_current_action_then_returns_to_idle()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Forever);

        automation.Runner.Play();
        var execution = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.Stop();
        automation.Runner.CompleteAction(execution.ExecutionId, elapsedSeconds: 1);

        Assert.Equal(ProgramRunnerState.Idle, automation.Runner.State);
    }

    [Fact]
    public void Stop_from_paused_returns_to_idle_immediately()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        automation.Runner.Play();
        var move = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.Pause();
        automation.Runner.CompleteAction(move.ExecutionId, elapsedSeconds: 1);
        automation.Runner.Stop();

        Assert.Equal(ProgramRunnerState.Idle, automation.Runner.State);
    }

    [Fact]
    public void Pause_without_current_action_pauses_immediately()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        automation.Runner.Play();
        automation.Runner.Pause();

        Assert.Equal(ProgramRunnerState.Paused, automation.Runner.State);
    }

    [Fact]
    public void Stop_without_current_action_returns_to_idle_immediately()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        automation.Runner.Play();
        automation.Runner.Stop();

        Assert.Equal(ProgramRunnerState.Idle, automation.Runner.State);
    }

    [Fact]
    public void Pause_waits_for_current_action_and_resume_continues_next_action()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        automation.Runner.Play();
        var move = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.Pause();
        automation.Runner.CompleteAction(move.ExecutionId, elapsedSeconds: 1);

        Assert.Equal(ProgramRunnerState.Paused, automation.Runner.State);

        automation.Runner.Play();
        var mine = automation.Runner.TryStartNextAction(automation)!;

        Assert.Equal("mine-1", mine.Action.Id);
    }

    [Fact]
    public void Fail_sets_error_message()
    {
        var automation = CreateAutomationWithMiningMacro();

        automation.Runner.Fail("No path to destination.");

        Assert.Equal(ProgramRunnerState.Error, automation.Runner.State);
        Assert.Equal("No path to destination.", automation.Runner.ErrorMessage);
    }

    [Fact]
    public void Missing_macro_reference_sets_error()
    {
        var automation = new PartyAutomation();
        automation.Program.AddStep("missing-macro", RepeatPolicy.Once);

        automation.Runner.Play();
        var execution = automation.Runner.TryStartNextAction(automation);

        Assert.Null(execution);
        Assert.Equal(ProgramRunnerState.Error, automation.Runner.State);
        Assert.Equal("Macro not found: missing-macro.", automation.Runner.ErrorMessage);
    }

    [Fact]
    public void Empty_macro_reference_sets_error()
    {
        var automation = new PartyAutomation();
        automation.AddMacro(new PartyMacro("macro-1", "Empty"));
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        automation.Runner.Play();
        var execution = automation.Runner.TryStartNextAction(automation);

        Assert.Null(execution);
        Assert.Equal(ProgramRunnerState.Error, automation.Runner.State);
        Assert.Equal("Macro has no actions: macro-1.", automation.Runner.ErrorMessage);
    }

    [Fact]
    public void Empty_macro_reference_under_repeating_program_sets_error()
    {
        var automation = new PartyAutomation();
        automation.AddMacro(new PartyMacro("macro-1", "Empty"));
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);
        automation.Program.SetProgramRepeat(RepeatPolicy.Forever);

        automation.Runner.Play();
        var execution = automation.Runner.TryStartNextAction(automation);

        Assert.Null(execution);
        Assert.Equal(ProgramRunnerState.Error, automation.Runner.State);
        Assert.Equal("Macro has no actions: macro-1.", automation.Runner.ErrorMessage);
    }

    [Fact]
    public void CompleteAction_rejects_invalid_elapsed_seconds()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        automation.Runner.Play();
        var execution = automation.Runner.TryStartNextAction(automation)!;
        var invalidElapsedSeconds = new[]
        {
            float.NaN,
            -1,
            float.PositiveInfinity,
            float.NegativeInfinity
        };

        foreach (var elapsedSeconds in invalidElapsedSeconds)
        {
            var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
                automation.Runner.CompleteAction(execution.ExecutionId, elapsedSeconds));

            Assert.Equal("elapsedSeconds", error.ParamName);
        }
    }

    private static void CompleteNext(PartyAutomation automation)
    {
        var execution = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.CompleteAction(execution.ExecutionId, elapsedSeconds: 1);
    }

    private static PartyAutomation CreateAutomationWithMiningMacro()
    {
        var automation = new PartyAutomation();
        var macro = new PartyMacro("macro-1", "Mining");
        macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(1, 0)));
        macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));
        automation.AddMacro(macro);
        return automation;
    }
}
