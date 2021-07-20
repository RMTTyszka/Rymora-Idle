using System.Collections;
using System.Collections.Generic;
using Map;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{

    [SerializeField] private Tilemap map;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = map.WorldToCell(mousePosition);

            MapTerrain clickedTile = map.GetTile(gridPosition) as MapTerrain;
            
            print(clickedTile.GetInstanceID());
        }
    }
}
