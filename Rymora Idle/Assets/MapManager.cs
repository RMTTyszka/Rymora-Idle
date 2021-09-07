using System;
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
    public Vector3? CurrentMouseTile1 { get; set; }
    public MapTerrain CurrentMouseTile { get; set; }
    public MapTerrain CurrentPlayerTile { get; set; }
    [SerializeField] private PartyManager partyManager;
    public GameObject actionMenu;

    [SerializeField] public Tilemap map;
    public List<City> Cities { get; set; } = new List<City>();
    public List<Vector3> CityPositions { get; set; } = new List<Vector3>();
    
    [SerializeField] private Tilemap hightlightMap;
    [SerializeField] private Tile hoverTile;
    public Vector3 CurrentMapPosition { get; set; }
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
                Cities.Add(city);
                CityPositions.Add(position);
            }

            if (tile is MapTerrain)
            {
                (tile as MapTerrain).Id = Guid.NewGuid();
            }
        }
        OnCitiesPoulated?.Invoke();
    }

    private void Update()
    {
        var mousePos = GetMousePosition();
        if (Input.GetMouseButtonDown(1))
        {
            if (actionMenu.gameObject.activeInHierarchy)
            {
                actionMenu.SetActive(false);
            }


            MapTerrain clickedTile = map.GetTile(mousePos.worldToCellPosition) as MapTerrain;
            if (clickedTile != null)
            {
                Vector3Int cellPosition = map.LocalToCell(mousePos.worldToCellPosition);
                var currentPosition = map.GetCellCenterLocal(cellPosition);
                if (CurrentMouseTile1 != null && CurrentMouseTile1 == currentPosition)
                {
                    actionMenu.SetActive(false);
                    CurrentMouseTile = null;
                    CurrentMouseTile1 = null;
                }
                else
                {
                    CurrentMouseTile1 = currentPosition;
                    CurrentMouseTile = clickedTile;
                    CurrentMapPosition =  mousePos.worldPosition;
                    actionMenu.transform.position = Input.mousePosition;
                    actionMenu.SetActive(true);  
                }


            }

        }      
        if (Input.GetMouseButtonDown(0))
        {
/*            if (actionMenu.gameObject.activeInHierarchy)
            {
                actionMenu.SetActive(false);
                            CurrentMouseTile = null;
            CurrentMapPosition = new Vector3();
            }*/

        }
        if (!mousePos.Equals(previousMousePos))
        {
            hightlightMap.SetTile(previousMousePos, null);
            MapTerrain clickedTile = map.GetTile(mousePos.worldToCellPosition) as MapTerrain;
            if (clickedTile != null)
            {
                hightlightMap.SetTile(mousePos.worldToCellPosition, hoverTile);
                previousMousePos = mousePos.worldToCellPosition;
            }

        }  

    }
    (Vector3Int worldToCellPosition, Vector3 worldPosition) GetMousePosition () {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        return (hightlightMap.WorldToCell(mouseWorldPos), mouseWorldPos);
    }

}
