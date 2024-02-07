using System;
using System.Linq;
using Global;
using Heroes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PartyManager PartyManager { get; set; }
    private MapManager MapManager { get; set; }
    public ScreenState CurrentScreen { get; set; }
    public Camera worldCamera;
    public Camera combatCamera;

    private void Awake()
    {
        PartyManager = FindAnyObjectByType<PartyManager>();
        MapManager = FindAnyObjectByType<MapManager>();
        CurrentScreen = ScreenState.Map;
        MapManager.GameManager = this;
        worldCamera.gameObject.SetActive(true);
        combatCamera.gameObject.SetActive(false);
        PartyManager.CurrentParty = PartyManager.heroes[0];
        MapManager.OnCitiesPoulated += InitiateHero;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    private void InitiateHero()
    {
        PartyManager.heroes[0].transform.position = MapManager.terrainMap.GetCellCenterWorld(Vector3Int.FloorToInt(MapManager.CityPositions[0]));
        PartyManager.heroes[1].transform.position = MapManager.terrainMap.GetCellCenterWorld(Vector3Int.FloorToInt(MapManager.CityPositions[1]));
        PartyManager.heroes[2].transform.position = MapManager.terrainMap.GetCellCenterWorld(Vector3Int.FloorToInt(MapManager.CityPositions[2]));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


public enum ScreenState
{
    Map = 0,
    Combat = 1
}
