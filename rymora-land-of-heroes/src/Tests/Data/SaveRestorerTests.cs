using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Data;
using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Data;

public sealed class SaveRestorerTests
{
    [Fact]
    public void Restore_round_trips_party_inventory_queue_and_selected_party()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMineActionQueued();
        scenario.InputApplication.SelectParty("party-1");
        scenario.InputApplication.Update(0.5f);
        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var restored = SaveRestorer.Restore(
            save,
            scenario.InputApplication.World,
            scenario.InputApplication.Config,
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        var party = restored.Parties.Get("party-1");
        Assert.Equal("party-1", restored.UI.SelectedPartyId);
        Assert.Equal(0.5f, party.ActionQueue.Current!.CurrentTime);
    }

    [Fact]
    public void Restore_round_trips_automation_runner_error()
    {
        var scenario = ApplicationObjectMother.ApplicationWithInvalidMiningProgram();
        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(1);
        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var restored = SaveRestorer.Restore(
            save,
            scenario.InputApplication.World,
            scenario.InputApplication.Config,
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        var runner = restored.Parties.Get("party-1").Automation.Runner;
        Assert.Equal(ProgramRunnerState.Error, runner.State);
        Assert.Contains("cannot mine", runner.ErrorMessage);
    }

    [Fact]
    public void Restore_round_trips_active_combat()
    {
        var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued(encounterProbability: 100);
        scenario.InputApplication.Update(1);
        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var restored = SaveRestorer.Restore(
            save,
            scenario.InputApplication.World,
            scenario.InputApplication.Config,
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        Assert.True(restored.Parties.Get("party-1").IsInCombat);
        Assert.True(restored.ActiveCombats.ContainsKey("party-1"));
    }

    [Fact]
    public void Restore_preserves_play_time()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMineActionQueued();
        scenario.InputApplication.Update(1.5f);
        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var restored = SaveRestorer.Restore(
            save,
            scenario.InputApplication.World,
            scenario.InputApplication.Config,
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        Assert.Equal(1.5f, restored.PlayTimeSeconds);
    }

    [Fact]
    public void GameConfig_exposes_save_auto_save_interval()
    {
        var config = TestObjectMother.CreateGameConfig();

        Assert.Equal(30, config.Save.AutoSaveIntervalSeconds);
    }

    [Fact]
    public void Restore_applies_saved_current_screen()
    {
        var save = new SaveData(
            SaveData.CurrentVersion,
            DateTimeOffset.UnixEpoch,
            PlayTimeSeconds: 0,
            SelectedPartyId: null,
            CurrentScreen: nameof(Screen.Combat),
            Parties: Array.Empty<PartySaveData>(),
            ActiveCombats: Array.Empty<CombatSaveData>());

        var restored = SaveRestorer.Restore(
            save,
            TestObjectMother.CreateWorld(new SequenceRandomSource(1)),
            TestObjectMother.CreateGameConfig(),
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        Assert.Equal(Screen.Combat, restored.UI.CurrentScreen);
    }
}
