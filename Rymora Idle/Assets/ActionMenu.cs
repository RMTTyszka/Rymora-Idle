using System.Collections;
using System.Collections.Generic;
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
    
    void Awake()
    {
        var actionButtons = GetComponentsInChildren<ActionButton>().ToList();
        MoveButton = actionButtons.First(button => button.action == ActionEnum.Move);
        MineButton = actionButtons.First(button => button.action == ActionEnum.Mine);
        CutWoodButton = actionButtons.First(button => button.action == ActionEnum.CutWood);
    } 
    
    void OnEnable()
    {
        MoveButton.gameObject.SetActive(true);
        MineButton.gameObject.SetActive(mapManager.CurrentMouseTile.CanMine());
        CutWoodButton.gameObject.SetActive(mapManager.CurrentMouseTile.CanCutWood());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move()
    {
        partyManager.CurrentHero.InitiateMovement(mapManager.CurrentMapPosition);
        gameObject.SetActive(false);
        partyManager.PublishWayPointUpdated(partyManager.CurrentHero);
    }    
    public void Mine()
    {

        var isAlreadyOnMapTile = partyManager.CurrentHero.transform.position == mapManager.CurrentMapPosition;
        if (!isAlreadyOnMapTile)
        {
            partyManager.CurrentHero.InitiateMovement(mapManager.CurrentMapPosition);
            partyManager.PublishWayPointUpdated(partyManager.CurrentHero);
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
            Terrain = mapManager.CurrentMouseTile
        };
        partyManager.CurrentHero.InitiateMining(heroAction);
        gameObject.SetActive(false);
    }

}
