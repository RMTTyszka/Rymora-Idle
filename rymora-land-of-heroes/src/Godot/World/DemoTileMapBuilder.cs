using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using RymoraLandOfHeroes.Core.World;
using RymoraLandOfHeroes.GodotAdapter.Content;

namespace RymoraLandOfHeroes.GodotAdapter.World;

public partial class DemoTileMapBuilder : Node
{
    private static readonly Vector2[] HexPoints =
    {
        new(TerrainTileCodes.TileSize * 0.50f, 0),
        new(TerrainTileCodes.TileSize - 1, TerrainTileCodes.TileSize * 0.25f),
        new(TerrainTileCodes.TileSize - 1, TerrainTileCodes.TileSize * 0.75f),
        new(TerrainTileCodes.TileSize * 0.50f, TerrainTileCodes.TileSize - 1),
        new(0, TerrainTileCodes.TileSize * 0.75f),
        new(0, TerrainTileCodes.TileSize * 0.25f)
    };

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

    public void BuildIfEmpty()
    {
        var terrainTiles = GetTerrainTiles();
        var regions = GetRegions();
        var zones = GetZones();
        if (TerrainLayer is null || RegionLayer is null || ZoneLayer is null)
        {
            return;
        }

        TerrainLayer.TileSet ??= CreateTileSet(terrainTiles.All.Select(tile => (tile.AtlasCoords, tile.Color)));
        RegionLayer.TileSet ??= CreateTileSet(regions.All.Select(region => (region.AtlasCoords, region.Color)));
        ZoneLayer.TileSet ??= CreateTileSet(zones.All.Select(zone => (zone.AtlasCoords, zone.Color)));
        if (TerrainLayer.GetUsedCells().Count > 0)
        {
            return;
        }

        for (var x = -5; x <= 6; x++)
        {
            for (var y = -4; y <= 4; y++)
            {
                Set(x, y, PickTerrain(x, y), PickRegionId(x, y), PickZoneId(x, y), terrainTiles, regions, zones);
            }
        }
    }

    private void Set(
        int x,
        int y,
        TerrainType terrainType,
        string regionId,
        string zoneId,
        TerrainTileCatalog terrainTiles,
        RuntimeRegionCatalog regions,
        RuntimeZoneCatalog zones)
    {
        var cell = new Vector2I(x, y);
        TerrainLayer!.SetCell(
            cell,
            TerrainTileCodes.SourceId,
            terrainTiles.GetByTerrainType(terrainType).AtlasCoords);
        RegionLayer!.SetCell(
            cell,
            TerrainTileCodes.SourceId,
            regions.GetDefinition(regionId).AtlasCoords);
        ZoneLayer!.SetCell(
            cell,
            TerrainTileCodes.SourceId,
            zones.GetDefinition(zoneId).AtlasCoords);
    }

    private static TerrainType PickTerrain(int x, int y)
    {
        if ((x == -2 && y == 0) || (x == -1 && y == 1) || (x == 2 && y == -1) || (x == 3 && y == 1))
        {
            return TerrainType.Wall;
        }

        if (x == 0 && y == 0)
        {
            return TerrainType.Mine;
        }

        if (x == 5 && y == 0)
        {
            return TerrainType.City;
        }

        if (x == 4 && y == -2)
        {
            return TerrainType.Place;
        }

        if (x == 4 && y == -1)
        {
            return TerrainType.Volcano;
        }

        if (y == 0 && x > 0)
        {
            return TerrainType.Road;
        }

        if (x <= -4)
        {
            return TerrainType.Desert;
        }

        if (x == -3)
        {
            return TerrainType.Jungle;
        }

        if (x == -2)
        {
            return TerrainType.Swamp;
        }

        if (y == -4)
        {
            return TerrainType.Snow;
        }

        if (y == 4)
        {
            return TerrainType.Water;
        }

        if (x >= 5 && y < 0)
        {
            return TerrainType.Ice;
        }

        if ((x + y) % 7 == 0)
        {
            return TerrainType.Hills;
        }

        if ((x + y) % 5 == 0)
        {
            return TerrainType.Forest;
        }

        if ((x - y) % 6 == 0)
        {
            return TerrainType.Mountain;
        }

        return TerrainType.Plain;
    }

    private static string PickRegionId(int x, int y)
    {
        return x == 5 && y == 0 ? "town" : "wild";
    }

    private static string PickZoneId(int x, int y)
    {
        var distance = Math.Max(Math.Abs(x), Math.Abs(y));
        if (distance <= 1)
        {
            return "deep";
        }

        return distance <= 3 ? "interior" : "edge";
    }

    private static TileSet CreateTileSet(IEnumerable<(Vector2I AtlasCoords, Color Color)> tiles)
    {
        var tileArray = tiles.ToArray();
        var maxAtlasX = tileArray.Max(tile => tile.AtlasCoords.X);
        var maxAtlasY = tileArray.Max(tile => tile.AtlasCoords.Y);
        var image = Image.CreateEmpty(
            TerrainTileCodes.TileSize * (maxAtlasX + 1),
            TerrainTileCodes.TileSize * (maxAtlasY + 1),
            useMipmaps: false,
            Image.Format.Rgba8);
        image.Fill(Colors.Transparent);

        foreach (var tile in tileArray)
        {
            FillTile(image, tile.AtlasCoords, tile.Color);
        }

        var source = new TileSetAtlasSource
        {
            Texture = ImageTexture.CreateFromImage(image),
            TextureRegionSize = new Vector2I(TerrainTileCodes.TileSize, TerrainTileCodes.TileSize)
        };

        foreach (var tile in tileArray)
        {
            source.CreateTile(tile.AtlasCoords);
        }

        var tileSet = new TileSet
        {
            TileShape = TileSet.TileShapeEnum.Hexagon,
            TileLayout = TileSet.TileLayoutEnum.Stacked,
            TileOffsetAxis = TileSet.TileOffsetAxisEnum.Horizontal,
            TileSize = new Vector2I(TerrainTileCodes.TileSize, TerrainTileCodes.TileSize)
        };
        tileSet.AddSource(source, TerrainTileCodes.SourceId);
        return tileSet;
    }

    private static void FillTile(Image image, Vector2I atlasCoords, Color color)
    {
        var startX = atlasCoords.X * TerrainTileCodes.TileSize;
        var startY = atlasCoords.Y * TerrainTileCodes.TileSize;
        for (var x = startX; x < startX + TerrainTileCodes.TileSize; x++)
        {
            for (var y = startY; y < startY + TerrainTileCodes.TileSize; y++)
            {
                var local = new Vector2(x - startX + 0.5f, y - startY + 0.5f);
                if (IsInsideHex(local))
                {
                    image.SetPixel(x, y, color);
                }
            }
        }

        DrawBorder(image, startX, startY);
    }

    private static void DrawBorder(Image image, int startX, int startY)
    {
        var border = new Color(0.04f, 0.04f, 0.04f, 0.55f);
        for (var x = startX; x < startX + TerrainTileCodes.TileSize; x++)
        {
            for (var y = startY; y < startY + TerrainTileCodes.TileSize; y++)
            {
                var local = new Vector2(x - startX + 0.5f, y - startY + 0.5f);
                if (IsInsideHex(local) && IsHexEdge(local))
                {
                    image.SetPixel(x, y, border);
                }
            }
        }
    }

    private static bool IsHexEdge(Vector2 point)
    {
        const int borderWidth = 2;
        return !IsInsideHex(point + new Vector2(borderWidth, 0))
            || !IsInsideHex(point - new Vector2(borderWidth, 0))
            || !IsInsideHex(point + new Vector2(0, borderWidth))
            || !IsInsideHex(point - new Vector2(0, borderWidth));
    }

    private static bool IsInsideHex(Vector2 point)
    {
        var inside = false;
        for (int current = 0, previous = HexPoints.Length - 1; current < HexPoints.Length; previous = current++)
        {
            var currentPoint = HexPoints[current];
            var previousPoint = HexPoints[previous];
            if ((currentPoint.Y > point.Y) == (previousPoint.Y > point.Y))
            {
                continue;
            }

            var xAtY = (previousPoint.X - currentPoint.X)
                * (point.Y - currentPoint.Y)
                / (previousPoint.Y - currentPoint.Y)
                + currentPoint.X;
            if (point.X < xAtY)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private TerrainTileCatalog GetTerrainTiles()
    {
        return _terrainTiles ?? throw new InvalidOperationException("Terrain tile catalog is not configured.");
    }

    private RuntimeRegionCatalog GetRegions()
    {
        return _regions ?? throw new InvalidOperationException("Region catalog is not configured.");
    }

    private RuntimeZoneCatalog GetZones()
    {
        return _zones ?? throw new InvalidOperationException("Zone catalog is not configured.");
    }
}
