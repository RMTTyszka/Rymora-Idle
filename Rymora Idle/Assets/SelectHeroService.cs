using System;
using System.Collections;
using System.Collections.Generic;
using Global;
using Heroes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

public class SelectHeroService : MonoBehaviour
{
    [SerializeField] private PartyManager partyManager;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void HeroSelected(Hero hero)
    {
        partyManager.PublishHeroSelected(hero);
    }
    public void HeroSelected(int heroIndex)
    {
        partyManager.PublishHeroSelected(partyManager.heroes[heroIndex]);
    }



}
