using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Data;
using RymoraLandOfHeroes.GodotAdapter.Data;

namespace RymoraLandOfHeroes.Core.Tests.Godot.Data;

public sealed class JsonSaveStoreTests
{
    [Fact]
    public void Save_then_load_round_trips_save_data()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"rymora-save-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "save-1.json");
        var store = new JsonSaveStore(path);
        var save = new SaveData(
            SaveData.CurrentVersion,
            DateTimeOffset.UnixEpoch,
            12,
            "party-1",
            nameof(Screen.Map),
            new[]
            {
                new PartySaveData(
                    "party-1",
                    new TilePositionSaveData(1, 2),
                    IsInCombat: false,
                    Members: Array.Empty<CreatureSaveData>(),
                    InventoryItems: Array.Empty<ItemSaveData>(),
                    ActionQueue: new ActionQueueSaveData(null, Array.Empty<PartyActionRequestSaveData>()),
                    Automation: AutomationSaveData.Empty)
            },
            Array.Empty<CombatSaveData>());

        store.Save(save);
        var loaded = store.TryLoad();

        Assert.NotNull(loaded);
        Assert.Equal(12, loaded.PlayTimeSeconds);
        Assert.Equal("party-1", loaded.SelectedPartyId);
    }

    [Fact]
    public void TryLoad_returns_null_when_file_missing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"rymora-missing-{Guid.NewGuid():N}", "save-1.json");
        var store = new JsonSaveStore(path);

        var loaded = store.TryLoad();

        Assert.Null(loaded);
    }
}
