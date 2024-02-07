using System.Diagnostics;
using System.Linq;
using Heroes;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ActionMenu : MonoBehaviour
{
    private MapManager MapManager { get; set; }
    private PartyManager PartyManager { get; set; }
    private ActionButton MoveButton { get; set; }
    private ActionButton MineButton { get; set; }
    private ActionButton CutWoodButton { get; set; }
    private ActionButton EnterDungeonButton { get; set; }
    
    void Awake()
    {
        var actionButtons = GetComponentsInChildren<ActionButton>().ToList();
        MapManager = FindAnyObjectByType<MapManager>();
        PartyManager = FindAnyObjectByType<PartyManager>();
        MoveButton = actionButtons.First(button => button.action == ActionEnum.Move);
        MineButton = actionButtons.First(button => button.action == ActionEnum.Mine);
        CutWoodButton = actionButtons.First(button => button.action == ActionEnum.CutWood);
        EnterDungeonButton = actionButtons.First(button => button.action == ActionEnum.EnterDungeon);
    } 
    
    void OnEnable()
    {
        MoveButton.gameObject.SetActive(true);
        MineButton.gameObject.SetActive(MapManager.CurrentMouseTile.CanMine());
        CutWoodButton.gameObject.SetActive(MapManager.CurrentMouseTile.CanCutWood());
        EnterDungeonButton.gameObject.SetActive(MapManager.CurrentMouseTile.CanEnterDungeon());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move()
    {
        PartyManager.CurrentParty.InitiateMovement(MapManager.CurrentMapPosition);
        gameObject.SetActive(false);
        PartyManager.PublishActionsUpdated(PartyManager.CurrentParty);
    }    
    public void Mine()
    {

        var isAlreadyOnMapTile = PartyManager.CurrentParty.transform.position == MapManager.CurrentMapPosition;
        if (!isAlreadyOnMapTile)
        {
            PartyManager.CurrentParty.InitiateMovement(MapManager.CurrentMapPosition);
        }

        var heroAction = new HeroAction
        {
            Action = null,
            EndTime = null,
            PassedTime = 0,
            ExecutedCount = 0,
            ExecutionAction = null,
            ActionEndType = ActionEndType.ByCount,
            LimitCount = 5,
            TimeToExecute = Heroes.Skills.MineTime,
            Terrain = MapManager.CurrentMouseTile,
            ActionType = HeroActionType.Mine,
        };
        PartyManager.CurrentParty.InitiateMining(heroAction);
        gameObject.SetActive(false);
        PartyManager.PublishActionsUpdated(PartyManager.CurrentParty);
    }  
    public void CutWood()
    {
        var isAlreadyOnMapTile = PartyManager.CurrentParty.transform.position == MapManager.CurrentMapPosition;
        if (!isAlreadyOnMapTile)
        {
            PartyManager.CurrentParty.InitiateMovement(MapManager.CurrentMapPosition);
        }
        var heroAction = new HeroAction
        {
            Action = null,
            EndTime = null,
            PassedTime = 0,
            ExecutedCount = 0,
            ExecutionAction = null,
            ActionEndType = ActionEndType.ByCount,
            LimitCount = 5,
            Terrain = MapManager.CurrentMouseTile,
            ActionType = HeroActionType.CutWood,
        };
        PartyManager.CurrentParty.InitiateCuttingWood(heroAction);
        gameObject.SetActive(false);
        PartyManager.PublishActionsUpdated(PartyManager.CurrentParty);
    }    
    public void EnterDungeon()
    {
        var isAlreadyOnMapTile = PartyManager.CurrentParty.transform.position == MapManager.CurrentMapPosition;
        if (!isAlreadyOnMapTile)
        {
            PartyManager.CurrentParty.InitiateMovement(MapManager.CurrentMapPosition);
        }
        Debug.Log("Entering Dungeon");
    }

}
