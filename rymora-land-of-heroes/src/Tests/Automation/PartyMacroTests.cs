using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.Tests.Automation;

public sealed class PartyMacroTests
{
    [Fact]
    public void MoveTo_action_has_absolute_destination()
    {
        var action = new MoveToMacroAction("move-1", new TilePosition(4, -2));

        Assert.Equal(MacroActionKind.MoveTo, action.Kind);
        Assert.Equal(new TilePosition(4, -2), action.Destination);
    }

    [Fact]
    public void Gather_action_rejects_move_kind()
    {
        var error = Assert.Throws<ArgumentException>(() => new GatherMacroAction(
            "gather-1",
            MacroActionKind.MoveTo,
            "Iron",
            itemLevel: 1,
            itemWeight: 3,
            RepeatPolicy.Once));

        Assert.Contains("Gather action kind must be Mine or CutWood", error.Message);
    }

    [Fact]
    public void Gather_action_rejects_nan_item_weight()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => new GatherMacroAction(
            "gather-1",
            MacroActionKind.Mine,
            "Iron",
            itemLevel: 1,
            itemWeight: float.NaN,
            RepeatPolicy.Once));

        Assert.Equal("itemWeight", error.ParamName);
    }

    [Fact]
    public void Macro_can_remove_and_reorder_actions()
    {
        var macro = new PartyMacro("macro-1", "Mining Run");
        macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(1, 0)));
        macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));
        macro.AddAction(new MoveToMacroAction("move-2", new TilePosition(2, 0)));

        macro.RemoveAction("mine-1");
        macro.MoveAction("move-2", newIndex: 0);

        Assert.Collection(
            macro.Actions,
            action => Assert.Equal("move-2", action.Id),
            action => Assert.Equal("move-1", action.Id));
    }

    [Fact]
    public void Rename_requires_non_empty_name()
    {
        var macro = new PartyMacro("macro-1", "Mining Run");

        Assert.Throws<ArgumentException>(() => macro.Rename(""));
    }

    [Fact]
    public void Macro_updates_gather_action_repeat()
    {
        var macro = new PartyMacro("macro-1", "Mining Run");
        macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));

        macro.SetGatherActionRepeat("mine-1", RepeatPolicy.Count(5));

        var action = Assert.IsType<GatherMacroAction>(macro.Actions[0]);
        Assert.Equal(5, action.Repeat.RepeatCount);
    }
}
