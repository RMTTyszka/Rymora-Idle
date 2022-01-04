using UnityEngine;

public class GameManager : MonoBehaviour
{

    public PartyManager PartyManager { get; set; }
    public MapManager MapManager;
    public ScreenState CurrentScreen { get; set; }
    public CombatManager CombatManager { get; set; }
    public Camera worldCamera;
    public Camera combatCamera;
    
    // Start is called before the first frame update
    void Start()
    {
        CombatManager = GameObject.FindGameObjectWithTag("CombatManager").GetComponent<CombatManager>();
        PartyManager = GameObject.FindGameObjectWithTag("PartyManager").GetComponent<PartyManager>();
        MapManager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
        CurrentScreen = ScreenState.Map;
        MapManager.GameManager = this;
        worldCamera.gameObject.SetActive(true);
        combatCamera.gameObject.SetActive(true);
        PartyManager.CurrentPartyNode = PartyManager.parties[0];
        MapManager.OnCitiesPoulated += InitiateHero;
    }

    private void InitiateHero()
    {
        PartyManager.parties[0].transform.position = MapManager.map.GetCellCenterWorld(Vector3Int.FloorToInt(MapManager.CityPositions[0]));
        PartyManager.parties[1].transform.position = MapManager.map.GetCellCenterWorld(Vector3Int.FloorToInt(MapManager.CityPositions[1]));
        PartyManager.parties[2].transform.position = MapManager.map.GetCellCenterWorld(Vector3Int.FloorToInt(MapManager.CityPositions[2]));
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
