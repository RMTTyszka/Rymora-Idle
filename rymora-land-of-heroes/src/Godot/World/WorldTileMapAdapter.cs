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

    [Export]
    public TileMapLayer? RegionLayer { get; set; }

    [Export]
    public TileMapLayer? ZoneLayer { get; set; }

    private TerrainTileCatalog? _terrainTiles;
    private RuntimeRegionCatalog? _regions;
    private RuntimeZoneCatalog? _zones;

    internal void Configure(TerrainTileCatalog terrainTiles, RuntimeRegionCatalog regions, RuntimeZoneCatalog zones)
    {
        _terrainTiles = terrainTiles;
        _regions = regions;
        _zones = zones;
    }

    public WorldState CreateWorld(IRandomSource? randomSource = null)
    {
        var tiles = ReadTiles();
        return new WorldState(tiles, randomSource);
    }

    public TilePosition FindFirstPosition(TerrainType terrainType)
    {
        foreach (var cell in GetTerrainLayer().GetUsedCells())
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
        return GetTerrainLayer().MapToLocal(new Vector2I(position.X, position.Y));
    }

    public TilePosition ToTilePosition(Vector2 worldPosition)
    {
        var cell = GetTerrainLayer().LocalToMap(GetTerrainLayer().ToLocal(worldPosition));
        return ToTilePosition(cell);
    }

    public bool HasTile(TilePosition position)
    {
        return GetTerrainLayer().GetCellSourceId(new Vector2I(position.X, position.Y)) != -1;
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
        var layer = GetTerrainLayer();
        var usedCells = layer.GetUsedCells();
        if (usedCells.Count == 0)
        {
            throw new InvalidOperationException("TerrainLayer has no cells.");
        }

        var tiles = new List<WorldTile>();
        foreach (var cell in usedCells)
        {
            var definition = GetTerrainTileDefinition(cell);
            var region = GetRegionTile(cell);
            var zone = GetZoneTile(cell);
            tiles.Add(new WorldTile(ToTilePosition(cell), definition.Terrain, region, zone));
        }

        return tiles;
    }

    private TileMapLayer GetTerrainLayer()
    {
        return TerrainLayer ?? throw new InvalidOperationException("TerrainLayer is not assigned.");
    }

    private TileMapLayer GetRegionLayer()
    {
        return RegionLayer ?? throw new InvalidOperationException("RegionLayer is not assigned.");
    }

    private TileMapLayer GetZoneLayer()
    {
        return ZoneLayer ?? throw new InvalidOperationException("ZoneLayer is not assigned.");
    }

    private TerrainTileCatalog GetTerrainTiles()
    {
        return _terrainTiles ?? throw new InvalidOperationException("Terrain tile catalog is not configured.");
    }

    private RuntimeRegionCatalog GetRegionCatalog()
    {
        return _regions ?? throw new InvalidOperationException("Region catalog is not configured.");
    }

    private RuntimeZoneCatalog GetZoneCatalog()
    {
        return _zones ?? throw new InvalidOperationException("Zone catalog is not configured.");
    }

    private TerrainTileDefinition GetTerrainTileDefinition(Vector2I cell)
    {
        return GetTerrainTiles().GetByAtlasCoords(GetTerrainLayer().GetCellAtlasCoords(cell));
    }

    private RegionData GetRegionTile(Vector2I cell)
    {
        if (GetRegionLayer().GetCellSourceId(cell) == -1)
        {
            throw new InvalidOperationException($"RegionLayer has no cell at {cell}.");
        }

        return GetRegionCatalog().GetRegion(GetRegionLayer().GetCellAtlasCoords(cell));
    }

    private ZoneData GetZoneTile(Vector2I cell)
    {
        if (GetZoneLayer().GetCellSourceId(cell) == -1)
        {
            throw new InvalidOperationException($"ZoneLayer has no cell at {cell}.");
        }

        return GetZoneCatalog().GetZone(GetZoneLayer().GetCellAtlasCoords(cell));
    }

    private static TilePosition ToTilePosition(Vector2I cell) => new(cell.X, cell.Y);
}
