using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utils
{
    public static class TilemapExtensions
    {
        public static T[] GetTiles<T>(this Tilemap tilemap) where T : TileBase
        {
            List<T> tiles = new List<T>();
        
            for (int y = tilemap.origin.y; y < (tilemap.origin.y + tilemap.size.y); y++)
            {
                for (int x = tilemap.origin.x; x < (tilemap.origin.x + tilemap.size.x); x++)
                {
                    T tile = tilemap.GetTile<T>(new Vector3Int(x, y, 0));
                    if (tile != null)
                    {
                        tiles.Add(tile);
                    }
                }
            }
            return tiles.ToArray();
        }
    }
}