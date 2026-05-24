using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;
using RymoraLandOfHeroes.Core.Content;
using RymoraLandOfHeroes.Core.Hero;
using RymoraLandOfHeroes.Core.Party;
using RymoraLandOfHeroes.Core.World;

namespace RymoraLandOfHeroes.GodotAdapter.Content;

internal sealed class GameContent
{
    public GameContent(
        GameConfig config,
        RuntimeWeaponCatalog weapons,
        RuntimeMaterialCatalog materials,
        RuntimeEncounterCatalog encounters,
        RuntimeRegionCatalog regions,
        RuntimeZoneCatalog zones,
        CreatureCatalog creatures,
        TerrainTileCatalog terrainTiles)
    {
        Config = config;
        Weapons = weapons;
        Materials = materials;
        Encounters = encounters;
        Regions = regions;
        Zones = zones;
        Creatures = creatures;
        TerrainTiles = terrainTiles;
    }

    public GameConfig Config { get; }
    public RuntimeWeaponCatalog Weapons { get; }
    public RuntimeMaterialCatalog Materials { get; }
    public RuntimeEncounterCatalog Encounters { get; }
    public RuntimeRegionCatalog Regions { get; }
    public RuntimeZoneCatalog Zones { get; }
    public CreatureCatalog Creatures { get; }
    public TerrainTileCatalog TerrainTiles { get; }
}

internal sealed class RuntimeWeaponCatalog : IWeaponCatalog
{
    private readonly Dictionary<string, WeaponTemplate> _weapons;

    public RuntimeWeaponCatalog(IEnumerable<WeaponTemplate> weapons)
    {
        _weapons = weapons.ToDictionary(weapon => weapon.Name, StringComparer.OrdinalIgnoreCase);
    }

    public WeaponTemplate GetWeapon(string name)
    {
        return _weapons.TryGetValue(name, out var weapon)
            ? weapon
            : throw new KeyNotFoundException($"Weapon not found: {name}.");
    }

    public IReadOnlyList<WeaponTemplate> GetAllWeapons() => _weapons.Values.ToArray();

    public WeaponTemplate GetRandomWeapon()
    {
        return _weapons.Values.FirstOrDefault()
            ?? throw new InvalidOperationException("Weapon catalog is empty.");
    }
}

internal sealed class RuntimeMaterialCatalog : IMaterialCatalog
{
    private readonly Dictionary<string, MaterialItem> _materials;

    public RuntimeMaterialCatalog(IEnumerable<MaterialItem> materials)
    {
        _materials = materials.ToDictionary(material => material.Name, StringComparer.OrdinalIgnoreCase);
    }

    public MaterialItem GetMaterial(string name)
    {
        return _materials.TryGetValue(name, out var material)
            ? material
            : throw new KeyNotFoundException($"Material not found: {name}.");
    }

    public IReadOnlyList<MaterialItem> GetAllMaterials() => _materials.Values.ToArray();
}

internal sealed class RuntimeEncounterCatalog : IEncounterCatalog
{
    private readonly Dictionary<string, EncounterTemplate> _encounters;
    private readonly Dictionary<string, List<EncounterTemplate>> _encountersByRegion;

    public RuntimeEncounterCatalog(IEnumerable<EncounterTemplate> encounters, IEnumerable<RegionDefinition> regions)
    {
        _encounters = encounters.ToDictionary(encounter => encounter.Id, StringComparer.OrdinalIgnoreCase);
        _encountersByRegion = new Dictionary<string, List<EncounterTemplate>>(StringComparer.OrdinalIgnoreCase);

        foreach (var region in regions)
        {
            var regionEncounters = new List<EncounterTemplate>();
            foreach (var encounterId in region.EncounterIdsByTerrain.Values.SelectMany(encounterIds => encounterIds).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                regionEncounters.Add(GetEncounter(encounterId));
            }

            _encountersByRegion.Add(region.Name, regionEncounters);
        }
    }

    public EncounterTemplate GetEncounter(string id)
    {
        return _encounters.TryGetValue(id, out var encounter)
            ? encounter
            : throw new KeyNotFoundException($"Encounter not found: {id}.");
    }

    public IReadOnlyList<EncounterTemplate> GetEncountersByRegion(string regionName)
    {
        return _encountersByRegion.TryGetValue(regionName, out var encounters)
            ? encounters
            : Array.Empty<EncounterTemplate>();
    }

    public EncounterTemplate GetFirstEncounter()
    {
        return _encounters.Values.FirstOrDefault()
            ?? throw new InvalidOperationException("Encounter catalog is empty.");
    }
}

internal sealed class RuntimeRegionCatalog
{
    private readonly Dictionary<Vector2I, RegionDefinition> _regionsByAtlasCoords;
    private readonly Dictionary<string, RegionDefinition> _regionsById;
    private readonly IEncounterCatalog _encounters;

    public RuntimeRegionCatalog(IEnumerable<RegionDefinition> regions, IEncounterCatalog encounters)
    {
        var regionArray = regions.ToArray();
        _regionsByAtlasCoords = regionArray.ToDictionary(region => region.AtlasCoords);
        _regionsById = regionArray.ToDictionary(region => region.Id, StringComparer.OrdinalIgnoreCase);
        _encounters = encounters;
    }

    public IReadOnlyList<RegionDefinition> All => _regionsByAtlasCoords.Values.ToArray();

    public RegionData GetRegion(Vector2I atlasCoords)
    {
        var definition = _regionsByAtlasCoords.TryGetValue(atlasCoords, out var region)
            ? region
            : throw new KeyNotFoundException($"Region not found at atlas coords {atlasCoords}.");

        var encountersByTerrain = definition.EncounterIdsByTerrain.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<EncounterTemplate>)pair.Value.Select(_encounters.GetEncounter).ToArray());

        return new RegionData(
            definition.Id,
            definition.Name,
            definition.IsSafeSpot,
            definition.EncounterProbabilityModifier,
            encountersByTerrain);
    }

    public RegionDefinition GetDefinition(string id)
    {
        return _regionsById.TryGetValue(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Region not found: {id}.");
    }
}

internal sealed record RegionDefinition(
    Vector2I AtlasCoords,
    string Id,
    string Name,
    bool IsSafeSpot,
    float EncounterProbabilityModifier,
    IReadOnlyDictionary<TerrainType, IReadOnlyList<string>> EncounterIdsByTerrain,
    Color Color);

internal sealed class RuntimeZoneCatalog
{
    private readonly Dictionary<Vector2I, ZoneDefinition> _zonesByAtlasCoords;
    private readonly Dictionary<string, ZoneDefinition> _zonesById;

    public RuntimeZoneCatalog(IEnumerable<ZoneDefinition> zones)
    {
        var zoneArray = zones.ToArray();
        _zonesByAtlasCoords = zoneArray.ToDictionary(zone => zone.AtlasCoords);
        _zonesById = zoneArray.ToDictionary(zone => zone.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ZoneDefinition> All => _zonesByAtlasCoords.Values.ToArray();

    public ZoneData GetZone(Vector2I atlasCoords)
    {
        var definition = _zonesByAtlasCoords.TryGetValue(atlasCoords, out var zone)
            ? zone
            : throw new KeyNotFoundException($"Zone not found at atlas coords {atlasCoords}.");

        return new ZoneData(
            definition.Id,
            definition.Name,
            definition.Level,
            definition.EncounterProbabilityModifier);
    }

    public ZoneDefinition GetDefinition(string id)
    {
        return _zonesById.TryGetValue(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Zone not found: {id}.");
    }
}

internal sealed record ZoneDefinition(
    Vector2I AtlasCoords,
    string Id,
    string Name,
    int Level,
    float EncounterProbabilityModifier,
    Color Color);

internal sealed class CreatureCatalog
{
    private readonly GameConfig _config;
    private readonly IWeaponCatalog _weaponCatalog;
    private readonly Dictionary<string, CreatureDefinition> _creatures;

    public CreatureCatalog(
        GameConfig config,
        IWeaponCatalog weaponCatalog,
        IEnumerable<CreatureDefinition> creatures)
    {
        _config = config;
        _weaponCatalog = weaponCatalog;
        _creatures = creatures.ToDictionary(creature => creature.Name, StringComparer.OrdinalIgnoreCase);
    }

    public Creature CreateCreature(string name)
    {
        var definition = _creatures.TryGetValue(name, out var creature)
            ? creature
            : throw new KeyNotFoundException($"Creature not found: {name}.");

        return CreateCreature(definition, definition.Sprite);
    }

    public Creature CreateCreature(CreatureTemplate template)
    {
        var definition = _creatures.TryGetValue(template.CreatureName, out var creature)
            ? creature
            : throw new KeyNotFoundException($"Creature not found: {template.CreatureName}.");

        var sprite = string.IsNullOrWhiteSpace(template.Sprite.Id)
            ? definition.Sprite
            : template.Sprite;

        return CreateCreature(definition, sprite);
    }

    private Creature CreateCreature(CreatureDefinition definition, SpriteReference sprite)
    {
        var attributes = StatBlock<AttributeType>.Create(
            _config.Progression.InitialAttributePoints,
            _config.Progression.AttributeValueDivisor);
        var skills = StatBlock<SkillType>.Create(
            _config.Progression.InitialSkillPoints,
            _config.Progression.SkillValueDivisor);
        var properties = StatBlock<PropertyType>.Create(0, 1);
        var equipment = new Equipment
        {
            MainHand = string.IsNullOrWhiteSpace(definition.MainHandWeapon)
                ? null
                : _weaponCatalog.GetWeapon(definition.MainHandWeapon)
        };

        return new Creature(
            definition.Name,
            attributes,
            skills,
            properties,
            equipment,
            new LifeConfig(definition.BaseLife, definition.VitalityLifeMultiplier),
            sprite);
    }
}

internal sealed record CreatureDefinition(
    string Name,
    float BaseLife,
    float VitalityLifeMultiplier,
    string? MainHandWeapon,
    SpriteReference Sprite);

internal sealed class TerrainTileCatalog
{
    private readonly Dictionary<Vector2I, TerrainTileDefinition> _tilesByAtlasCoords;
    private readonly Dictionary<TerrainType, TerrainTileDefinition> _tilesByTerrainType;

    public TerrainTileCatalog(IEnumerable<TerrainTileDefinition> tiles)
    {
        All = tiles.ToArray();
        _tilesByAtlasCoords = All.ToDictionary(tile => tile.AtlasCoords);
        _tilesByTerrainType = All.ToDictionary(tile => tile.Terrain.Type);
    }

    public IReadOnlyList<TerrainTileDefinition> All { get; }

    public TerrainTileDefinition GetByAtlasCoords(Vector2I atlasCoords)
    {
        return _tilesByAtlasCoords.TryGetValue(atlasCoords, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Terrain tile not found at atlas coords {atlasCoords}.");
    }

    public TerrainTileDefinition GetByTerrainType(TerrainType terrainType)
    {
        return _tilesByTerrainType.TryGetValue(terrainType, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Terrain tile not found for terrain type {terrainType}.");
    }
}

internal sealed record TerrainTileDefinition(
    Vector2I AtlasCoords,
    TerrainData Terrain,
    string? MiningMaterial,
    string? WoodcuttingMaterial,
    Color Color)
{
    public MaterialItem? GetMaterialForAction(PartyActionType actionType, IMaterialCatalog materials)
    {
        var materialName = actionType switch
        {
            PartyActionType.Mine => MiningMaterial,
            PartyActionType.CutWood => WoodcuttingMaterial,
            _ => null
        };

        return string.IsNullOrWhiteSpace(materialName)
            ? null
            : materials.GetMaterial(materialName);
    }
}

internal static class GodotColorParser
{
    public static Color ParseHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return Colors.White;
        }

        var normalized = hex.Trim().TrimStart('#');
        if (normalized.Length != 6)
        {
            throw new FormatException($"Unsupported color format: {hex}.");
        }

        var red = int.Parse(normalized[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255f;
        var green = int.Parse(normalized[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255f;
        var blue = int.Parse(normalized[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255f;
        return new Color(red, green, blue);
    }
}
