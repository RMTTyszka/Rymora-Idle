using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Map;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public delegate void CitiesPopulated();

    public event CitiesPopulated OnCitiesPoulated;

    [SerializeField] public Tilemap currentMap;
    private int currentIndex = 0;
    public List<City> Cities { get; set; } = new List<City>();
    public List<Vector3> CityPositions { get; set; } = new List<Vector3>();
    private void Start()
    {
        foreach (var position in currentMap.cellBounds.allPositionsWithin) 
        {
            if (!currentMap.HasTile(position)) {
                continue;
            }

            var tile = currentMap.GetTile(position);
            if (tile is City city)
            {
                print(tile.name);
                Cities.Add(city);
                CityPositions.Add(position);
            }
        }
        OnCitiesPoulated?.Invoke();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = currentMap.WorldToCell(mousePosition);

            MapTerrain clickedTile = currentMap.GetTile(gridPosition) as MapTerrain;
            
            print(clickedTile.CanMine());
            print(clickedTile.CanCutWood());
        }
    }

}
