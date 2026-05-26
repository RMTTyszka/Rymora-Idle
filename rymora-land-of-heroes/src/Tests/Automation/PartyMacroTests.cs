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
}
