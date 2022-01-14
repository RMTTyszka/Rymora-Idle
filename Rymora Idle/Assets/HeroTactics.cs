using System.Collections;
using System.Collections.Generic;
using Global;
using UnityEngine;

public class HeroTactics : MonoBehaviour
{
    private PartyManager PartyManager { get; set; }
    private Creature Hero { get; set; }

    public CombatActionComponent combatActionComponentPrefab; 
    // Start is called before the first frame update
    void Start()
    {
        PartyManager = GameObject.FindGameObjectWithTag("PartyManager").GetComponent<PartyManager>();
        PartyManager.OnHeroSelected += HeroSelected;
    }

    private void HeroSelected(Creature hero)
    {
        Hero = hero;
        foreach (var action in GetComponentsInChildren<CombatActionComponent>())
        {
            Destroy(action.gameObject);
        }
        foreach (var action in Hero.CombatActions)
        {
            var actionComponent = Instantiate(combatActionComponentPrefab, Vector3.zero, transform.rotation);
            actionComponent.transform.SetParent(transform);
            actionComponent.Action = action;
            actionComponent.Powers = hero.LearnedPowers;
        }
        var newActionComponent = Instantiate(combatActionComponentPrefab, Vector3.zero, transform.rotation);
        newActionComponent.transform.SetParent(transform);
        newActionComponent.Action = new CombatAction();
        newActionComponent.Powers = hero.LearnedPowers;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}