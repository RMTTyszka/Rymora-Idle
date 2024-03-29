using System;
using System.Collections.Generic;
using Map;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public delegate void CitiesPopulated();

    public event CitiesPopulated OnCitiesPoulated;
    
    public GameManager GameManager { get; set; }
    public Vector3? CurrentMouseTile1 { get; set; }
    public MapTerrain CurrentMouseTile { get; set; }
    public MapTerrain CurrentPlayerTile { get; set; }
    private PartyManager PartyManager { get; set; }
    public GameObject actionMenu;

    public List<City> Cities { get; set; } = new List<City>();
    public List<Vector3> CityPositions { get; set; } = new List<Vector3>();
    
    [FormerlySerializedAs("Terrain Map")] [SerializeField] public Tilemap terrainMap;
    [FormerlySerializedAs("Region Map")] [SerializeField] public Tilemap regionMap;
    [SerializeField] private Tilemap hightlightMap;
    [SerializeField] private Tile hoverTile;
    public Vector3 CurrentMapPosition { get; set; }
    private Vector3Int previousMousePos = new Vector3Int();

    public void Awake()
    {
        PartyManager = FindAnyObjectByType<PartyManager>();
    }

    private void Start()
    {
        foreach (var position in terrainMap.cellBounds.allPositionsWithin) 
        {
            if (!terrainMap.HasTile(position)) {
                continue;
            }

            var tile = terrainMap.GetTile(position);
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
        if (GameManager.CurrentScreen == ScreenState.Map)
        {
            var mousePos = GetMousePosition();
            if (Input.GetMouseButtonDown(1))
            {
                if (!PartyManager.CurrentParty.IsAlive)
                {
                    return;
                }

                if (actionMenu.gameObject.activeInHierarchy)
                {
                    actionMenu.SetActive(false);
                }


                MapTerrain clickedTile = terrainMap.GetTile(mousePos.worldToCellPosition) as MapTerrain;
                if (clickedTile != null)
                {
                    Vector3Int cellPosition = terrainMap.LocalToCell(mousePos.worldToCellPosition);
                    var currentPosition = terrainMap.GetCellCenterLocal(cellPosition);
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
                MapTerrain clickedTile = terrainMap.GetTile(mousePos.worldToCellPosition) as MapTerrain;
                if (clickedTile != null)
                {
                    hightlightMap.SetTile(mousePos.worldToCellPosition, hoverTile);
                    previousMousePos = mousePos.worldToCellPosition;
                }

            }   
        }

        

    }
    (Vector3Int worldToCellPosition, Vector3 worldPosition) GetMousePosition () 
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        return (hightlightMap.WorldToCell(mouseWorldPos), mouseWorldPos);
    }

}
