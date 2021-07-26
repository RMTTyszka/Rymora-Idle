using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        partyManager.CurrentHero.Waypoints.Clear();
        partyManager.CurrentHero.Waypoints.Add(mapManager.CurrentMapPosition);
    }

}