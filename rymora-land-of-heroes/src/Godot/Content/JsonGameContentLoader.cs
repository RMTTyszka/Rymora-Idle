using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;
using RymoraLandOfHeroes.Core.Content;
using RymoraLandOfHeroes.Core.World;

namespace RymoraLandOfHeroes.GodotAdapter.Content;

internal static class JsonGameContentLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static GameContent LoadDefault()
    {
        var config = LoadConfig("res://assets/data/game_config.json");
        var weaponTemplates = LoadWeapons("res://assets/data/content/weapons.json");
        var materialItems = LoadMaterials("res://assets/data/content/materials.json");
        var encounterTemplates = LoadEncounters("res://assets/data/content/encounters.json");
        var regionDefinitions = LoadRegions("res://assets/data/world/regions.json");
        var zoneDefinitions = LoadZones("res://assets/data/world/zones.json");
        var creatureDefinitions = LoadCreatures("res://assets/data/content/creatures.json");
        var terrainTileDefinitions = LoadTerrainTiles("res://assets/data/world/terrain_tiles.json");

        ContentValidator.Validate(CreateValidationData(
            weaponTemplates,
            materialItems,
            creatureDefinitions,
            encounterTemplates,
            regionDefinitions,
            zoneDefinitions,
            terrainTileDefinitions));

        var weapons = new RuntimeWeaponCatalog(weaponTemplates);
        var materials = new RuntimeMaterialCatalog(materialItems);
        var encounters = new RuntimeEncounterCatalog(encounterTemplates, regionDefinitions);
        var regions = new RuntimeRegionCatalog(regionDefinitions, encounters);
        var zones = new RuntimeZoneCatalog(zoneDefinitions);
        var creatures = new CreatureCatalog(
            config,
            weapons,
            creatureDefinitions);
        var terrainTiles = new TerrainTileCatalog(terrainTileDefinitions);

        return new GameContent(config, weapons, materials, encounters, regions, zones, creatures, terrainTiles);
    }

    private static ContentValidationData CreateValidationData(
        IReadOnlyList<WeaponTemplate> weapons,
        IReadOnlyList<MaterialItem> materials,
        IReadOnlyList<CreatureDefinition> creatures,
        IReadOnlyList<EncounterTemplate> encounters,
        IReadOnlyList<RegionDefinition> regions,
        IReadOnlyList<ZoneDefinition> zones,
        IReadOnlyList<TerrainTileDefinition> terrainTiles)
    {
        return new ContentValidationData(
            Weapons: weapons.Select(weapon => new ContentWeaponData(weapon.Name)).ToArray(),
            Materials: materials.Select(material => new ContentMaterialData(material.Name)).ToArray(),
            Creatures: creatures.Select(creature => new ContentCreatureData(creature.Name, creature.MainHandWeapon)).ToArray(),
            Encounters: encounters.Select(encounter => new ContentEncounterData(
                encounter.Id,
                encounter.Monsters.Select(monster => monster.CreatureName).ToArray())).ToArray(),
            Regions: regions.Select(region => new ContentRegionData(
                region.Id,
                region.AtlasCoords.X,
                region.AtlasCoords.Y,
                region.IsSafeSpot,
                region.EncounterIdsByTerrain.ToDictionary(
                    pair => pair.Key.ToString(),
                    pair => pair.Value))).ToArray(),
            Zones: zones.Select(zone => new ContentZoneData(zone.Id, zone.AtlasCoords.X, zone.AtlasCoords.Y)).ToArray(),
            TerrainTiles: terrainTiles.Select(tile => new ContentTerrainTileData(
                tile.AtlasCoords.X,
                tile.AtlasCoords.Y,
                tile.Terrain.Type.ToString(),
                tile.MiningMaterial,
                tile.WoodcuttingMaterial)).ToArray());
    }

    private static GameConfig LoadConfig(string path)
    {
        var config = ReadJson<GameConfigDto>(path);
        return new GameConfig(
            config.EncounterProbability,
            config.EncounterInterval,
            new EncounterPolicy(ParseEnum<EncounterModifierMode>(config.EncounterPolicy.TerrainModifierMode)),
            config.CorpseDecayTime,
            new ProgressionConfig(
                config.Progression.InitialAttributePoints,
                config.Progression.InitialSkillPoints,
                config.Progression.AttributeValueDivisor,
                config.Progression.SkillValueDivisor),
            new LifeConfig(config.Life.BaseLife, config.Life.VitalityLifeMultiplier),
            new CollectionConfig(
                config.Collection.DifficultyBase,
                config.Collection.DifficultyPerMaterialLevel,
                config.Collection.MiningActionTime,
                config.Collection.WoodcuttingActionTime),
            new TravelConfig(config.Travel.ActionTime),
            new CombatConfig(
                new RollRange(config.Combat.HitRollRange.Min, config.Combat.HitRollRange.Max),
                new RollRange(config.Combat.EvadeRollRange.Min, config.Combat.EvadeRollRange.Max),
                config.Combat.BaseCriticalMultiplier,
                new TargetingConfig(config.Combat.Targeting.LowLifeWeight, config.Combat.Targeting.ThreatWeight)));
    }

    private static IReadOnlyList<WeaponTemplate> LoadWeapons(string path)
    {
        var weapons = ReadJson<List<WeaponDto>>(path);
        return weapons.ConvertAll(weapon => new WeaponTemplate(
            weapon.Name,
            weapon.Level,
            ParseEnum<WeaponSize>(weapon.Size),
            ParseEnum<WeaponDamageCategory>(weapon.DamageCategory),
            weapon.AttackSpeed,
            weapon.BaseDamage,
            weapon.SizeMultiplier,
            weapon.HitModifier,
            weapon.Penetration,
            weapon.CounterPotential));
    }

    private static IReadOnlyList<MaterialItem> LoadMaterials(string path)
    {
        var materials = ReadJson<List<MaterialDto>>(path);
        return materials.ConvertAll(material => new MaterialItem(material.Name, material.Level, material.Weight));
    }

    private static IReadOnlyList<CreatureDefinition> LoadCreatures(string path)
    {
        var creatures = ReadJson<List<CreatureDto>>(path);
        return creatures.ConvertAll(creature => new CreatureDefinition(
            creature.Name,
            creature.BaseLife,
            creature.VitalityLifeMultiplier,
            creature.MainHandWeapon,
            new SpriteReference(creature.Sprite?.Id ?? string.Empty)));
    }

    private static IReadOnlyList<EncounterTemplate> LoadEncounters(string path)
    {
        var encounters = ReadJson<List<EncounterDto>>(path);
        return encounters.ConvertAll(encounter => new EncounterTemplate(
            encounter.Id,
            encounter.Name,
            encounter.Level,
            encounter.Monsters.ConvertAll(monster => new CreatureTemplate(
                monster.CreatureName,
                ParseEnum<MonsterClass>(monster.Class),
                new SpriteReference(monster.Sprite?.Id ?? string.Empty)))));
    }

    private static IReadOnlyList<RegionDefinition> LoadRegions(string path)
    {
        var regions = ReadJson<List<RegionDto>>(path);
        return regions.ConvertAll(region => new RegionDefinition(
            new Vector2I(region.AtlasX, region.AtlasY),
            region.Id,
            region.Name,
            region.IsSafeSpot,
            region.EncounterProbabilityModifier,
            region.EncountersByTerrain.ToDictionary(
                pair => ParseEnum<TerrainType>(pair.Key),
                pair => (IReadOnlyList<string>)pair.Value),
            GodotColorParser.ParseHex(region.Color)));
    }

    private static IReadOnlyList<ZoneDefinition> LoadZones(string path)
    {
        var zones = ReadJson<List<ZoneDto>>(path);
        return zones.ConvertAll(zone => new ZoneDefinition(
            new Vector2I(zone.AtlasX, zone.AtlasY),
            zone.Id,
            zone.Name,
            zone.Level,
            zone.EncounterProbabilityModifier,
            GodotColorParser.ParseHex(zone.Color)));
    }

    private static IReadOnlyList<TerrainTileDefinition> LoadTerrainTiles(string path)
    {
        var terrainTiles = ReadJson<List<TerrainTileDto>>(path);
        return terrainTiles.ConvertAll(tile => new TerrainTileDefinition(
            new Vector2I(tile.AtlasX, tile.AtlasY),
            new TerrainData(
                ParseEnum<TerrainType>(tile.Type),
                tile.IsWalkable,
                tile.MoveSpeed,
                tile.Quality,
                tile.EncounterRateModifier,
                tile.AllowsMining,
                tile.AllowsWoodcutting,
                tile.IsCity,
                tile.IsPlace),
            tile.MiningMaterial,
            tile.WoodcuttingMaterial,
            GodotColorParser.ParseHex(tile.Color)));
    }

    private static T ReadJson<T>(string path)
    {
        if (!FileAccess.FileExists(path))
        {
            throw new InvalidOperationException($"JSON file not found: {path}.");
        }

        var json = FileAccess.GetFileAsString(path);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new InvalidOperationException($"JSON file is empty or invalid: {path}.");
    }

    private static TEnum ParseEnum<TEnum>(string value)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Unknown {typeof(TEnum).Name} value: {value}.");
    }

    private sealed class GameConfigDto
    {
        public float EncounterProbability { get; set; }
        public float EncounterInterval { get; set; }
        public EncounterPolicyDto EncounterPolicy { get; set; } = new();
        public float CorpseDecayTime { get; set; }
        public ProgressionConfigDto Progression { get; set; } = new();
        public LifeConfigDto Life { get; set; } = new();
        public CollectionConfigDto Collection { get; set; } = new();
        public TravelConfigDto Travel { get; set; } = new();
        public CombatConfigDto Combat { get; set; } = new();
    }

    private sealed class EncounterPolicyDto
    {
        public string TerrainModifierMode { get; set; } = string.Empty;
    }

    private sealed class ProgressionConfigDto
    {
        public float InitialAttributePoints { get; set; }
        public float InitialSkillPoints { get; set; }
        public float AttributeValueDivisor { get; set; }
        public float SkillValueDivisor { get; set; }
    }

    private sealed class LifeConfigDto
    {
        public float BaseLife { get; set; }
        public float VitalityLifeMultiplier { get; set; }
    }

    private sealed class CollectionConfigDto
    {
        public float DifficultyBase { get; set; }
        public float DifficultyPerMaterialLevel { get; set; }
        public float MiningActionTime { get; set; }
        public float WoodcuttingActionTime { get; set; }
    }

    private sealed class TravelConfigDto
    {
        public float ActionTime { get; set; }
    }

    private sealed class CombatConfigDto
    {
        public RollRangeDto HitRollRange { get; set; } = new();
        public RollRangeDto EvadeRollRange { get; set; } = new();
        public float BaseCriticalMultiplier { get; set; }
        public TargetingConfigDto Targeting { get; set; } = new();
    }

    private sealed class RollRangeDto
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }

    private sealed class TargetingConfigDto
    {
        public float LowLifeWeight { get; set; }
        public float ThreatWeight { get; set; }
    }

    private sealed class WeaponDto
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Size { get; set; } = string.Empty;
        public string DamageCategory { get; set; } = string.Empty;
        public float AttackSpeed { get; set; }
        public float BaseDamage { get; set; }
        public float SizeMultiplier { get; set; }
        public float HitModifier { get; set; }
        public float Penetration { get; set; }
        public float CounterPotential { get; set; }
    }

    private sealed class MaterialDto
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public float Weight { get; set; }
    }

    private sealed class CreatureDto
    {
        public string Name { get; set; } = string.Empty;
        public float BaseLife { get; set; }
        public float VitalityLifeMultiplier { get; set; }
        public string? MainHandWeapon { get; set; }
        public SpriteDto? Sprite { get; set; }
    }

    private sealed class EncounterDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public List<EncounterMonsterDto> Monsters { get; set; } = new();
    }

    private sealed class RegionDto
    {
        public int AtlasX { get; set; }
        public int AtlasY { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsSafeSpot { get; set; }
        public float EncounterProbabilityModifier { get; set; }
        public Dictionary<string, List<string>> EncountersByTerrain { get; set; } = new();
        public string Color { get; set; } = "#FFFFFF";
    }

    private sealed class ZoneDto
    {
        public int AtlasX { get; set; }
        public int AtlasY { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public float EncounterProbabilityModifier { get; set; }
        public string Color { get; set; } = "#FFFFFF";
    }

    private sealed class EncounterMonsterDto
    {
        public string CreatureName { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public SpriteDto? Sprite { get; set; }
    }

    private sealed class SpriteDto
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class TerrainTileDto
    {
        public int AtlasX { get; set; }
        public int AtlasY { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsWalkable { get; set; }
        public float MoveSpeed { get; set; }
        public int Quality { get; set; }
        public int EncounterRateModifier { get; set; }
        public bool AllowsMining { get; set; }
        public bool AllowsWoodcutting { get; set; }
        public bool IsCity { get; set; }
        public bool IsPlace { get; set; }
        public string? MiningMaterial { get; set; }
        public string? WoodcuttingMaterial { get; set; }
        public string Color { get; set; } = "#FFFFFF";
    }
}
