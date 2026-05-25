using System;
using System.Collections.Generic;
using System.Linq;

namespace RymoraLandOfHeroes.GodotAdapter.Content;

public sealed record ContentValidationData(
    IReadOnlyList<ContentWeaponData> Weapons,
    IReadOnlyList<ContentMaterialData> Materials,
    IReadOnlyList<ContentCreatureData> Creatures,
    IReadOnlyList<ContentEncounterData> Encounters,
    IReadOnlyList<ContentRegionData> Regions,
    IReadOnlyList<ContentZoneData> Zones,
    IReadOnlyList<ContentTerrainTileData> TerrainTiles);

public sealed record ContentWeaponData(string Name);

public sealed record ContentMaterialData(string Name);

public sealed record ContentCreatureData(string Name, string? MainHandWeapon);

public sealed record ContentEncounterData(string Id, IReadOnlyList<string> CreatureNames);

public sealed record ContentRegionData(
    string Id,
    int AtlasX,
    int AtlasY,
    bool IsSafeSpot,
    IReadOnlyDictionary<string, IReadOnlyList<string>> EncounterIdsByTerrain);

public sealed record ContentZoneData(string Id, int AtlasX, int AtlasY);

public sealed record ContentTerrainTileData(
    int AtlasX,
    int AtlasY,
    string Type,
    string? MiningMaterial,
    string? WoodcuttingMaterial);

public static class ContentValidator
{
    public static void Validate(ContentValidationData data)
    {
        var weaponNames = ToNameSet(data.Weapons.Select(weapon => weapon.Name));
        var materialNames = ToNameSet(data.Materials.Select(material => material.Name));
        var creatureNames = ToNameSet(data.Creatures.Select(creature => creature.Name));
        var encounterIds = ToNameSet(data.Encounters.Select(encounter => encounter.Id));

        ValidateDuplicateAtlasCoords("terrain", data.TerrainTiles.Select(tile => (tile.AtlasX, tile.AtlasY)));
        ValidateDuplicateAtlasCoords("region", data.Regions.Select(region => (region.AtlasX, region.AtlasY)));
        ValidateDuplicateAtlasCoords("zone", data.Zones.Select(zone => (zone.AtlasX, zone.AtlasY)));
        ValidateCreatureWeapons(data.Creatures, weaponNames);
        ValidateEncounterCreatures(data.Encounters, creatureNames);
        ValidateRegionEncounters(data.Regions, encounterIds);
        ValidateTerrainMaterials(data.TerrainTiles, materialNames);
    }

    private static HashSet<string> ToNameSet(IEnumerable<string> names)
    {
        return names.Where(name => !string.IsNullOrWhiteSpace(name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static void ValidateDuplicateAtlasCoords(string catalogName, IEnumerable<(int X, int Y)> atlasCoords)
    {
        var seen = new HashSet<(int X, int Y)>();
        foreach (var coords in atlasCoords)
        {
            if (!seen.Add(coords))
            {
                throw new InvalidOperationException($"Duplicate {catalogName} atlas coords ({coords.X}, {coords.Y}).");
            }
        }
    }

    private static void ValidateCreatureWeapons(IReadOnlyList<ContentCreatureData> creatures, HashSet<string> weaponNames)
    {
        foreach (var creature in creatures)
        {
            if (string.IsNullOrWhiteSpace(creature.MainHandWeapon) || weaponNames.Contains(creature.MainHandWeapon))
            {
                continue;
            }

            throw new InvalidOperationException($"Creature {creature.Name} references unknown weapon {creature.MainHandWeapon}.");
        }
    }

    private static void ValidateEncounterCreatures(IReadOnlyList<ContentEncounterData> encounters, HashSet<string> creatureNames)
    {
        foreach (var encounter in encounters)
        {
            foreach (var creatureName in encounter.CreatureNames)
            {
                if (creatureNames.Contains(creatureName))
                {
                    continue;
                }

                throw new InvalidOperationException($"Encounter {encounter.Id} references unknown creature {creatureName}.");
            }
        }
    }

    private static void ValidateRegionEncounters(IReadOnlyList<ContentRegionData> regions, HashSet<string> encounterIds)
    {
        foreach (var region in regions)
        {
            foreach (var (terrain, regionEncounterIds) in region.EncounterIdsByTerrain)
            {
                foreach (var encounterId in regionEncounterIds)
                {
                    if (encounterIds.Contains(encounterId))
                    {
                        continue;
                    }

                    throw new InvalidOperationException($"Region {region.Id} references unknown encounter {encounterId} for terrain {terrain}.");
                }
            }
        }
    }

    private static void ValidateTerrainMaterials(IReadOnlyList<ContentTerrainTileData> terrainTiles, HashSet<string> materialNames)
    {
        foreach (var tile in terrainTiles)
        {
            ValidateTerrainMaterial(tile, tile.MiningMaterial, materialNames, "mining");
            ValidateTerrainMaterial(tile, tile.WoodcuttingMaterial, materialNames, "woodcutting");
        }
    }

    private static void ValidateTerrainMaterial(
        ContentTerrainTileData tile,
        string? materialName,
        HashSet<string> materialNames,
        string actionName)
    {
        if (string.IsNullOrWhiteSpace(materialName) || materialNames.Contains(materialName))
        {
            return;
        }

        throw new InvalidOperationException($"Terrain tile ({tile.AtlasX}, {tile.AtlasY}) references unknown {actionName} material {materialName}.");
    }
}
