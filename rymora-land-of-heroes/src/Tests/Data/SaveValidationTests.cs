using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Data;

namespace RymoraLandOfHeroes.Core.Tests.Data;

public sealed class SaveValidationTests
{
    [Fact]
    public void Validate_rejects_unknown_save_version()
    {
        var save = EmptySave() with { SaveVersion = "999" };

        var error = Assert.Throws<InvalidOperationException>(() => SaveValidation.Validate(save));

        Assert.Contains("Unsupported save version", error.Message);
    }

    [Fact]
    public void Validate_rejects_missing_selected_party()
    {
        var save = EmptySave() with { SelectedPartyId = "missing" };

        var error = Assert.Throws<InvalidOperationException>(() => SaveValidation.Validate(save));

        Assert.Contains("SelectedPartyId", error.Message);
    }

    [Fact]
    public void Validate_rejects_incomplete_travel_request()
    {
        var request = new PartyActionRequestSaveData(
            ActionType: "Travel",
            EndType: "ByCount",
            TimeToExecute: 1,
            LimitCount: 1,
            EndTime: null,
            ItemName: null,
            ItemLevel: null,
            ItemWeight: null,
            Quantity: null,
            TargetPartyId: null,
            Destination: null,
            Path: Array.Empty<TilePositionSaveData>(),
            AutomationActionId: null);
        var party = EmptyParty() with
        {
            ActionQueue = new ActionQueueSaveData(
                Current: new PartyActionStateSaveData(request, 0, 0, 0, Started: true),
                Pending: Array.Empty<PartyActionRequestSaveData>())
        };
        var save = EmptySave() with { Parties = new[] { party } };

        var error = Assert.Throws<InvalidOperationException>(() => SaveValidation.Validate(save));

        Assert.Contains("Destination", error.Message);
    }

    [Fact]
    public void Validate_rejects_unknown_screen()
    {
        var save = EmptySave() with { CurrentScreen = "BadScreen" };

        var error = Assert.Throws<InvalidOperationException>(() => SaveValidation.Validate(save));

        Assert.Contains("CurrentScreen", error.Message);
    }

    [Fact]
    public void Validate_rejects_duplicate_active_combat_party()
    {
        var party = EmptyParty() with { IsInCombat = true };
        var combat = EmptyCombat("party-1");
        var save = EmptySave() with
        {
            Parties = new[] { party },
            ActiveCombats = new[] { combat, combat }
        };

        var error = Assert.Throws<InvalidOperationException>(() => SaveValidation.Validate(save));

        Assert.Contains("Duplicate active combat", error.Message);
    }

    [Fact]
    public void Validate_rejects_party_in_combat_without_active_combat()
    {
        var save = EmptySave() with { Parties = new[] { EmptyParty() with { IsInCombat = true } } };

        var error = Assert.Throws<InvalidOperationException>(() => SaveValidation.Validate(save));

        Assert.Contains("missing active combat", error.Message);
    }

    [Fact]
    public void Validate_rejects_non_finite_action_time()
    {
        var request = new PartyActionRequestSaveData(
            ActionType: "Mine",
            EndType: "ByCount",
            TimeToExecute: float.NaN,
            LimitCount: 1,
            EndTime: null,
            ItemName: "Iron",
            ItemLevel: 1,
            ItemWeight: 3,
            Quantity: null,
            TargetPartyId: null,
            Destination: null,
            Path: Array.Empty<TilePositionSaveData>(),
            AutomationActionId: null);
        var save = EmptySave() with
        {
            Parties = new[]
            {
                EmptyParty() with
                {
                    ActionQueue = new ActionQueueSaveData(
                        Current: new PartyActionStateSaveData(request, 0, 0, 0, Started: true),
                        Pending: Array.Empty<PartyActionRequestSaveData>())
                }
            }
        };

        var error = Assert.Throws<InvalidOperationException>(() => SaveValidation.Validate(save));

        Assert.Contains("TimeToExecute", error.Message);
    }

    private static SaveData EmptySave()
    {
        return new SaveData(
            SaveVersion: SaveData.CurrentVersion,
            SavedAtUtc: DateTimeOffset.UnixEpoch,
            PlayTimeSeconds: 0,
            SelectedPartyId: "party-1",
            CurrentScreen: nameof(Screen.Map),
            Parties: new[] { EmptyParty() },
            ActiveCombats: Array.Empty<CombatSaveData>());
    }

    private static PartySaveData EmptyParty()
    {
        return new PartySaveData(
            PartyId: "party-1",
            Position: new TilePositionSaveData(0, 0),
            IsInCombat: false,
            Members: Array.Empty<CreatureSaveData>(),
            InventoryItems: Array.Empty<ItemSaveData>(),
            ActionQueue: new ActionQueueSaveData(null, Array.Empty<PartyActionRequestSaveData>()),
            Automation: AutomationSaveData.Empty);
    }

    private static CombatSaveData EmptyCombat(string partyId)
    {
        return new CombatSaveData(
            partyId,
            State: "Ongoing",
            Heroes: Array.Empty<HeroCombatantSaveData>(),
            Monsters: Array.Empty<MonsterCombatantSaveData>());
    }
}
