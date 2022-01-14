using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Global;
using UnityEngine;
using UnityEngine.Serialization;

public class HeroTactics : MonoBehaviour
{
    private PartyManager PartyManager { get; set; }
    private Creature Hero { get; set; }

    public CombatActionComponent combatActionComponentPrefab; 
    public List<CombatActionComponent> TacticsRows { get; set; } 
    public GameObject addButton;

    public HeroTactics()
    {
        TacticsRows = new List<CombatActionComponent>();
    }
    // Start is called before the first frame update
    void Start()
    {
        PartyManager = GameObject.FindGameObjectWithTag("PartyManager").GetComponent<PartyManager>();
        PartyManager.OnHeroSelected += HeroSelected;
        Destroy(GetComponentsInChildren<CombatActionComponent>().Select(e => e.gameObject).First());
    }

    private void HeroSelected(Creature hero)
    {
        Hero = hero;
        foreach (var action in TacticsRows)
        {
            Destroy(action.gameObject);
        }
        TacticsRows.Clear();

        var index = 0;
        foreach (var action in Hero.CombatActions)
        {
            var actionComponent = Instantiate(combatActionComponentPrefab, Vector3.zero, transform.rotation);
            actionComponent.transform.SetParent(transform);
            actionComponent.Action = action;
            actionComponent.Powers = Hero.LearnedPowers;
            actionComponent.Index = index++;
            TacticsRows.Add(actionComponent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddRow()
    {
        if (Hero.CombatActions.Count < Hero.MaxTactics)
        {
            var action = new CombatAction();
            var newActionComponent = Instantiate(combatActionComponentPrefab, Vector3.zero, transform.rotation);
            newActionComponent.transform.SetParent(transform);
            newActionComponent.Action = action;
            newActionComponent.Powers = Hero.LearnedPowers;
            newActionComponent.Index = Hero.CombatActions.Count;
            TacticsRows.Add(newActionComponent);
            addButton.transform.SetAsLastSibling();
            Hero.CombatActions.Add(action);
        }
    }

    public void RemoveRow(int index)
    {
        Hero.CombatActions.RemoveAt(index);
        Destroy(TacticsRows.Select(e => e.gameObject).ToList().ElementAt(index));
        TacticsRows.RemoveAt(index);
        index = 0;
        foreach (var action in TacticsRows)
        {
            action.Index = index++;
        }
    }

}