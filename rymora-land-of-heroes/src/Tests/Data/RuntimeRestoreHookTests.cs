using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Hero;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.Core.Tests.Data;

public sealed class RuntimeRestoreHookTests
{
    [Fact]
    public void PartyActionQueue_restore_preserves_current_progress_and_pending_order()
    {
        var currentRequest = new PartyActionRequest(PartyActionType.Mine, PartyActionEndType.ByCount, 5, LimitCount: 2, ItemName: "Iron", ItemLevel: 1, ItemWeight: 3);
        var current = PartyActionState.Restore(currentRequest, currentTime: 2, passedTime: 7, executedCount: 1, started: true);
        var pending = new[]
        {
            new PartyActionRequest(PartyActionType.CutWood, PartyActionEndType.ByCount, 3, LimitCount: 1, ItemName: "Oak", ItemLevel: 1, ItemWeight: 1)
        };

        var queue = new PartyActionQueue();
        queue.Restore(current, pending);

        Assert.Equal(2, queue.Current!.CurrentTime);
        Assert.Equal(7, queue.Current.PassedTime);
        Assert.Equal(1, queue.Current.ExecutedCount);
        Assert.Equal(PartyActionType.CutWood, queue.PendingRequests.Single().ActionType);
    }

    [Fact]
    public void Creature_restore_preserves_life_and_max_life()
    {
        var creature = Creature.Restore(
            name: "Hero",
            attributes: StatBlock<AttributeType>.Create(5, 5),
            skills: StatBlock<SkillType>.Create(5, 5),
            properties: StatBlock<PropertyType>.Create(0, 1),
            equipment: new Equipment(),
            sprite: default,
            life: 12,
            maxLife: 34);

        Assert.Equal(12, creature.Life);
        Assert.Equal(34, creature.MaxLife);
    }

    [Fact]
    public void ProgramRunner_restore_preserves_error_state()
    {
        var runner = new ProgramRunner();
        var state = new ProgramRunnerRuntimeState(
            State: ProgramRunnerState.Error,
            ErrorMessage: "bad macro",
            CurrentMacroActions: Array.Empty<MacroAction>(),
            CurrentAction: null,
            ProgramStepIndex: 0,
            MacroActionIndex: 0,
            StepIteration: 0,
            ProgramIteration: 0,
            ActionIteration: 0,
            ProgramElapsedSeconds: 0,
            StepElapsedSeconds: 0,
            ActionElapsedSeconds: 0,
            CurrentExecutionId: null);

        runner.Restore(state);

        Assert.Equal(ProgramRunnerState.Error, runner.State);
        Assert.Equal("bad macro", runner.ErrorMessage);
    }
}
