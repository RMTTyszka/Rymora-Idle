using System;
using System.Collections.Generic;
using Godot;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Content;
using RymoraLandOfHeroes.Core.Party;
using RymoraLandOfHeroes.Core.World;
using RymoraLandOfHeroes.GodotAdapter.Content;

namespace RymoraLandOfHeroes.GodotAdapter.World;

public partial class WorldTileMapAdapter : Node
{
    [Export]
    public TileMapLayer? TerrainLayer { get; set; }

    private TerrainTileCatalog? _terrainTiles;
    private IEncounterCatalog? _encounters;

    internal void Configure(TerrainTileCatalog terrainTiles, IEncounterCatalog encounters)
    {
        _terrainTiles = terrainTiles;
        _encounters = encounters;
    }

    public WorldState CreateWorld(IRandomSource? randomSource = null)
    {
        var tiles = ReadTiles();
        return new WorldState(tiles, randomSource);
    }

    public TilePosition FindFirstPosition(TerrainType terrainType)
    {
        foreach (var cell in GetLayer().GetUsedCells())
        {
            var currentTerrainType = GetTerrainTileDefinition(cell).Terrain.Type;
            if (currentTerrainType == terrainType)
            {
                return ToTilePosition(cell);
            }
        }

        throw new InvalidOperationException($"Terrain not found in TileMapLayer: {terrainType}.");
    }

    public Vector2 ToLocalPosition(TilePosition position)
    {
        return GetLayer().MapToLocal(new Vector2I(position.X, position.Y));
    }

    public TilePosition ToTilePosition(Vector2 worldPosition)
    {
        var cell = GetLayer().LocalToMap(GetLayer().ToLocal(worldPosition));
        return ToTilePosition(cell);
    }

    public bool HasTile(TilePosition position)
    {
        return GetLayer().GetCellSourceId(new Vector2I(position.X, position.Y)) != -1;
    }

    internal TerrainTileDefinition GetTerrainTileDefinition(TilePosition position)
    {
        return GetTerrainTileDefinition(new Vector2I(position.X, position.Y));
    }

    internal MaterialItem? GetMaterialForAction(TilePosition position, PartyActionType actionType, IMaterialCatalog materials)
    {
        return GetTerrainTileDefinition(position).GetMaterialForAction(actionType, materials);
    }

    private IReadOnlyList<WorldTile> ReadTiles()
    {
        var layer = GetLayer();
        var usedCells = layer.GetUsedCells();
        if (usedCells.Count == 0)
        {
            throw new InvalidOperationException("TerrainLayer has no cells.");
        }

        var tiles = new List<WorldTile>();
        foreach (var cell in usedCells)
        {
            var definition = GetTerrainTileDefinition(cell);
            var region = new RegionData(
                definition.RegionName,
                definition.IsSafeSpot,
                GetEncounterCatalog().GetEncountersByRegion(definition.RegionName));
            tiles.Add(new WorldTile(ToTilePosition(cell), definition.Terrain, region));
        }

        return tiles;
    }

    private TileMapLayer GetLayer()
    {
        return TerrainLayer ?? throw new InvalidOperationException("TerrainLayer is not assigned.");
    }

    private TerrainTileCatalog GetTerrainTiles()
    {
        return _terrainTiles ?? throw new InvalidOperationException("Terrain tile catalog is not configured.");
    }

    private IEncounterCatalog GetEncounterCatalog()
    {
        return _encounters ?? throw new InvalidOperationException("Encounter catalog is not configured.");
    }

    private TerrainTileDefinition GetTerrainTileDefinition(Vector2I cell)
    {
        return GetTerrainTiles().GetByAtlasCoords(GetLayer().GetCellAtlasCoords(cell));
    }

    private static TilePosition ToTilePosition(Vector2I cell) => new(cell.X, cell.Y);
}
