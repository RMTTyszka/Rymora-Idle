using RymoraLandOfHeroes.Core.Automation;

namespace RymoraLandOfHeroes.Core.Tests.Automation;

public sealed class PartyProgramTests
{
    [Fact]
    public void Program_adds_macro_reference_as_step()
    {
        var program = new PartyProgram();

        var step = program.AddStep("macro-1", RepeatPolicy.Count(3));

        Assert.Equal("macro-1", step.MacroId);
        Assert.Equal(RepeatMode.Count, step.Repeat.Mode);
        Assert.Equal(3, step.Repeat.RepeatCount);
    }

    [Fact]
    public void Program_reorders_steps()
    {
        var program = new PartyProgram();
        var first = program.AddStep("macro-1", RepeatPolicy.Once);
        var second = program.AddStep("macro-2", RepeatPolicy.Once);

        program.MoveStep(second.Id, newIndex: 0);

        Assert.Collection(
            program.Steps,
            step => Assert.Equal(second.Id, step.Id),
            step => Assert.Equal(first.Id, step.Id));
    }

    [Fact]
    public void Party_automation_has_active_program()
    {
        var automation = new PartyAutomation();

        Assert.NotNull(automation.Program);
        Assert.Empty(automation.Program.Steps);
    }
}
