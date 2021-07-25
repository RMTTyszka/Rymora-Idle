using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Map;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public delegate void CitiesPopulated();

    public event CitiesPopulated OnCitiesPoulated;
    public MapTerrain CurrentMouseTile { get; set; }
    public MapTerrain CurrentPlayerTile { get; set; }
    public GameObject actionMenu;

    [FormerlySerializedAs("currentMap")] [SerializeField] public Tilemap map;
    private int currentIndex = 0;
    public List<City> Cities { get; set; } = new List<City>();
    public List<Vector3> CityPositions { get; set; } = new List<Vector3>();
    
    [SerializeField] private Tilemap hightlightMap;
    [SerializeField] private Tile hoverTile;
    private Vector3Int previousMousePos = new Vector3Int();

    private void Start()
    {
        foreach (var position in map.cellBounds.allPositionsWithin) 
        {
            if (!map.HasTile(position)) {
                continue;
            }

            var tile = map.GetTile(position);
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
        Vector3Int mousePos = GetMousePosition();
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = map.WorldToCell(mousePosition);

            MapTerrain clickedTile = map.GetTile(gridPosition) as MapTerrain;
            if (clickedTile != null)
            {
                CurrentMouseTile = clickedTile;
                actionMenu.transform.position = Input.mousePosition;
                actionMenu.SetActive(true);
                hightlightMap.SetTile(mousePos, null);
            }

        }
        if (!mousePos.Equals(previousMousePos))
        {
            hightlightMap.SetTile(previousMousePos, null); // Remove old hoverTile
            hightlightMap.SetTile(mousePos, hoverTile);
            map.SetTile(mousePos, hoverTile);
            var a = hightlightMap.GetTile(mousePos);
            print	(a.name);
            previousMousePos = mousePos;
        }  
        if (Input.GetMouseButtonDown(0))
        {
        //    hightlightMap.SetTile(previousMousePos, null); // Remove old hoverTile
            hightlightMap.SetTile(mousePos, hoverTile);
        }

    }
    Vector3Int GetMousePosition () {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return hightlightMap.WorldToCell(mouseWorldPos);
    }

}
