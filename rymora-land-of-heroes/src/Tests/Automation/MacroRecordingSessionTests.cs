using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;
using PartyEntity = RymoraLandOfHeroes.Core.Party.Party;

namespace RymoraLandOfHeroes.Core.Tests.Automation;

public sealed class MacroRecordingSessionTests
{
    [Fact]
    public void Record_gather_adds_move_to_before_mine()
    {
        var recording = new MacroRecordingSession("recording-1");

        recording.RecordGather(
            target: new TilePosition(2, 0),
            kind: MacroActionKind.Mine,
            itemName: "Iron",
            itemLevel: 1,
            itemWeight: 3);

        Assert.Collection(
            recording.Actions,
            action => Assert.IsType<MoveToMacroAction>(action),
            action => Assert.IsType<GatherMacroAction>(action));
        Assert.Equal(new TilePosition(2, 0), ((MoveToMacroAction)recording.Actions[0]).Destination);
    }

    [Fact]
    public void Invalid_record_gather_leaves_actions_unchanged()
    {
        var recording = new MacroRecordingSession("recording-1");

        Assert.Throws<ArgumentException>(() => recording.RecordGather(
            target: new TilePosition(2, 0),
            kind: MacroActionKind.MoveTo,
            itemName: "Iron",
            itemLevel: 1,
            itemWeight: 3));

        Assert.Empty(recording.Actions);
    }

    [Fact]
    public void Save_requires_macro_name()
    {
        var recording = new MacroRecordingSession("recording-1");

        Assert.Throws<ArgumentException>(() => recording.Save(""));
    }

    [Fact]
    public void Party_owns_automation_state()
    {
        var party = new PartyEntity("party-1", new TilePosition(0, 0));

        Assert.NotNull(party.Automation);
        Assert.Empty(party.Automation.Macros);
    }

    [Fact]
    public void Saving_duplicate_macro_id_is_rejected()
    {
        var automation = new PartyAutomation();
        automation.StartRecording("recording-1");
        automation.SaveRecording("Mining Run");
        automation.StartRecording("recording-1");

        var error = Assert.Throws<InvalidOperationException>(() => automation.SaveRecording("Mining Run Again"));

        Assert.Contains("Macro already exists", error.Message);
    }
}
