using System.Linq;
using Heroes;
using UnityEngine;

public class ActionMenu : MonoBehaviour
{

    [SerializeField] private MapManager mapManager;
    [SerializeField] private PartyManager partyManager;
    private ActionButton MoveButton { get; set; }
    private ActionButton MineButton { get; set; }
    private ActionButton CutWoodButton { get; set; }
    private ActionButton EnterDungeonButton { get; set; }
    
    void Awake()
    {
        var actionButtons = GetComponentsInChildren<ActionButton>().ToList();
        MoveButton = actionButtons.First(button => button.action == ActionEnum.Move);
        MineButton = actionButtons.First(button => button.action == ActionEnum.Mine);
        CutWoodButton = actionButtons.First(button => button.action == ActionEnum.CutWood);
        EnterDungeonButton = actionButtons.First(button => button.action == ActionEnum.EnterDungeon);
    } 
    
    void OnEnable()
    {
        MoveButton.gameObject.SetActive(true);
        MineButton.gameObject.SetActive(mapManager.CurrentMouseTile.CanMine());
        CutWoodButton.gameObject.SetActive(mapManager.CurrentMouseTile.CanCutWood());
        EnterDungeonButton.gameObject.SetActive(mapManager.CurrentMouseTile.CanEnterDungeon());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move()
    {
        partyManager.CurrentParty.InitiateMovement(mapManager.CurrentMapPosition);
        gameObject.SetActive(false);
        partyManager.PublishActionsUpdated(partyManager.CurrentParty);
    }    
    public void Mine()
    {

        var isAlreadyOnMapTile = partyManager.CurrentParty.transform.position == mapManager.CurrentMapPosition;
        if (!isAlreadyOnMapTile)
        {
            partyManager.CurrentParty.InitiateMovement(mapManager.CurrentMapPosition);
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
            Terrain = mapManager.CurrentMouseTile,
            ActionName = "Mine"
        };
        partyManager.CurrentParty.InitiateMining(heroAction);
        gameObject.SetActive(false);
        partyManager.PublishActionsUpdated(partyManager.CurrentParty);
    }  
    public void CutWood()
    {
        var isAlreadyOnMapTile = partyManager.CurrentParty.transform.position == mapManager.CurrentMapPosition;
        if (!isAlreadyOnMapTile)
        {
            partyManager.CurrentParty.InitiateMovement(mapManager.CurrentMapPosition);
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
            Terrain = mapManager.CurrentMouseTile,
            ActionName = "Cut Wood"
        };
        partyManager.CurrentParty.InitiateCuttingWood(heroAction);
        gameObject.SetActive(false);
        partyManager.PublishActionsUpdated(partyManager.CurrentParty);
    }    
    public void EnterDungeon()
    {
        var isAlreadyOnMapTile = partyManager.CurrentParty.transform.position == mapManager.CurrentMapPosition;
        if (!isAlreadyOnMapTile)
        {
            partyManager.CurrentParty.InitiateMovement(mapManager.CurrentMapPosition);
        }
        Debug.Log("Entering Dungeon");
    }

}
