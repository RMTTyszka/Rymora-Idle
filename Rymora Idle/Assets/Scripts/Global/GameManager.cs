using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Global;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{

    public PartyManager partyManager;
    public MapManager mapManager;
    

    // Start is called before the first frame update
    void Start()
    {
        partyManager.CurrentHero = partyManager.heroes[0];
        mapManager.OnCitiesPoulated += InitiateHero;
    }

    private void InitiateHero()
    {
        partyManager.heroes[0].transform.position = mapManager.map.GetCellCenterWorld(Vector3Int.FloorToInt(mapManager.CityPositions[0]));
        partyManager.heroes[1].transform.position = mapManager.map.GetCellCenterWorld(Vector3Int.FloorToInt(mapManager.CityPositions[1]));
        partyManager.heroes[2].transform.position = mapManager.map.GetCellCenterWorld(Vector3Int.FloorToInt(mapManager.CityPositions[2]));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
