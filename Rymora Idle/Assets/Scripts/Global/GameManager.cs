using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Global;
using Heroes;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{

    public PartyManager partyManager;
    public MapManager mapManager;
    public ScreenState CurrentScreen { get; set; }
    public CombatManager CombatManager { get; set; }
    public Camera worldCamera;
    public Camera combatCamera;
    
    // Start is called before the first frame update
    void Start()
    {
        CombatManager = GameObject.FindGameObjectWithTag("CombatManager").GetComponent<CombatManager>();
        CurrentScreen = ScreenState.Map;
        mapManager.GameManager = this;
        worldCamera.gameObject.SetActive(true);
        combatCamera.gameObject.SetActive(false);
        partyManager.CurrentParty = partyManager.heroes[0];
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

    public void InitiateCombat(Encounter encounter, Party party)
    {
        CombatManager.Monsters = encounter.Monsters.Select(monster => Creature.FromCreature(monster)).ToList();
    }
}

public class Encounter
{
    public List<Creature> Monsters { get; set; }
}


public enum ScreenState
{
    Map = 0,
    Combat = 1
}
