using RymoraLandOfHeroes.GodotAdapter.Content;

namespace RymoraLandOfHeroes.Core.Tests.Content;

public sealed class ContentValidatorTests
{
    [Fact]
    public void Validate_accepts_consistent_content()
    {
        ContentValidator.Validate(ValidData());
    }

    [Fact]
    public void Validate_throws_when_creature_weapon_is_missing()
    {
        var data = ValidData() with
        {
            Creatures = new[] { new ContentCreatureData("Hero", "Missing Sword") }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => ContentValidator.Validate(data));

        Assert.Contains("Creature Hero references unknown weapon Missing Sword.", exception.Message);
    }

    [Fact]
    public void Validate_throws_when_encounter_creature_is_missing()
    {
        var data = ValidData() with
        {
            Encounters = new[] { new ContentEncounterData("demo-goblin", new[] { "Missing Goblin" }) }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => ContentValidator.Validate(data));

        Assert.Contains("Encounter demo-goblin references unknown creature Missing Goblin.", exception.Message);
    }

    [Fact]
    public void Validate_throws_when_region_encounter_is_missing()
    {
        var data = ValidData() with
        {
            Regions = new[]
            {
                new ContentRegionData(
                    "wild",
                    0,
                    0,
                    IsSafeSpot: false,
                    new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["Plain"] = new[] { "missing-encounter" }
                    })
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => ContentValidator.Validate(data));

        Assert.Contains("Region wild references unknown encounter missing-encounter for terrain Plain.", exception.Message);
    }

    [Fact]
    public void Validate_throws_when_terrain_material_is_missing()
    {
        var data = ValidData() with
        {
            TerrainTiles = new[] { new ContentTerrainTileData(0, 0, "Plain", "Missing Ore", null) }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => ContentValidator.Validate(data));

        Assert.Contains("Terrain tile (0, 0) references unknown mining material Missing Ore.", exception.Message);
    }

    [Fact]
    public void Validate_throws_when_terrain_atlas_coords_are_duplicated()
    {
        var data = ValidData() with
        {
            TerrainTiles = new[]
            {
                new ContentTerrainTileData(0, 0, "Plain", null, null),
                new ContentTerrainTileData(0, 0, "Forest", null, "Oak")
            }
        };

        var exception = Assert.Throws<InvalidOperationException>(() => ContentValidator.Validate(data));

        Assert.Contains("Duplicate terrain atlas coords (0, 0).", exception.Message);
    }

    private static ContentValidationData ValidData()
    {
        return new ContentValidationData(
            Weapons: new[] { new ContentWeaponData("Training Sword") },
            Materials: new[] { new ContentMaterialData("Iron"), new ContentMaterialData("Oak") },
            Creatures: new[]
            {
                new ContentCreatureData("Hero", "Training Sword"),
                new ContentCreatureData("Goblin", null)
            },
            Encounters: new[] { new ContentEncounterData("demo-goblin", new[] { "Goblin" }) },
            Regions: new[]
            {
                new ContentRegionData(
                    "wild",
                    0,
                    0,
                    IsSafeSpot: false,
                    new Dictionary<string, IReadOnlyList<string>>
                    {
                        ["Plain"] = new[] { "demo-goblin" }
                    })
            },
            Zones: new[] { new ContentZoneData("edge", 0, 0) },
            TerrainTiles: new[] { new ContentTerrainTileData(0, 0, "Plain", "Iron", null) });
    }
}
