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
        partyManager.CurrentHero.transform.position = mapManager.currentMap.GetCellCenterWorld(Vector3Int.FloorToInt(mapManager.CityPositions[0]));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
