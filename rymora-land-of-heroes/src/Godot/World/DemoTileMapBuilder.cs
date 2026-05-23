using System;
using System.Linq;
using Godot;
using RymoraLandOfHeroes.Core.World;
using RymoraLandOfHeroes.GodotAdapter.Content;

namespace RymoraLandOfHeroes.GodotAdapter.World;

public partial class DemoTileMapBuilder : Node
{
    [Export]
    public TileMapLayer? TerrainLayer { get; set; }

    private TerrainTileCatalog? _terrainTiles;

    internal void Configure(TerrainTileCatalog terrainTiles)
    {
        _terrainTiles = terrainTiles;
    }

    public void BuildIfEmpty()
    {
        var terrainTiles = GetTerrainTiles();
        if (TerrainLayer is null || TerrainLayer.GetUsedCells().Count > 0)
        {
            return;
        }

        TerrainLayer.TileSet = CreateTileSet(terrainTiles);

        for (var x = -5; x <= 6; x++)
        {
            for (var y = -4; y <= 4; y++)
            {
                Set(x, y, PickTerrain(x, y), terrainTiles);
            }
        }
    }

    private void Set(int x, int y, TerrainType terrainType, TerrainTileCatalog terrainTiles)
    {
        TerrainLayer!.SetCell(
            new Vector2I(x, y),
            TerrainTileCodes.SourceId,
            terrainTiles.GetByTerrainType(terrainType).AtlasCoords);
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

    private static TileSet CreateTileSet(TerrainTileCatalog terrainTiles)
    {
        var maxAtlasX = terrainTiles.All.Max(tile => tile.AtlasCoords.X);
        var maxAtlasY = terrainTiles.All.Max(tile => tile.AtlasCoords.Y);
        var image = Image.CreateEmpty(
            TerrainTileCodes.TileSize * (maxAtlasX + 1),
            TerrainTileCodes.TileSize * (maxAtlasY + 1),
            useMipmaps: false,
            Image.Format.Rgba8);
        image.Fill(Colors.Transparent);

        foreach (var terrainTile in terrainTiles.All)
        {
            FillTile(image, terrainTile.AtlasCoords, terrainTile.Color);
        }

        var source = new TileSetAtlasSource
        {
            Texture = ImageTexture.CreateFromImage(image),
            TextureRegionSize = new Vector2I(TerrainTileCodes.TileSize, TerrainTileCodes.TileSize)
        };

        foreach (var terrainTile in terrainTiles.All)
        {
            source.CreateTile(terrainTile.AtlasCoords);
        }

        var tileSet = new TileSet
        {
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
                image.SetPixel(x, y, color);
            }
        }

        DrawBorder(image, startX, startY);
    }

    private static void DrawBorder(Image image, int startX, int startY)
    {
        var border = new Color(0.04f, 0.04f, 0.04f, 0.55f);
        var endX = startX + TerrainTileCodes.TileSize - 1;
        var endY = startY + TerrainTileCodes.TileSize - 1;
        for (var offset = 0; offset < TerrainTileCodes.TileSize; offset++)
        {
            image.SetPixel(startX + offset, startY, border);
            image.SetPixel(startX + offset, endY, border);
            image.SetPixel(startX, startY + offset, border);
            image.SetPixel(endX, startY + offset, border);
        }
    }

    private TerrainTileCatalog GetTerrainTiles()
    {
        return _terrainTiles ?? throw new InvalidOperationException("Terrain tile catalog is not configured.");
    }
}
