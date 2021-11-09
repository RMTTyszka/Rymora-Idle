using System;
using System.Collections;
using System.Collections.Generic;
using Global;
using Heroes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

public class CurrentHeroService : MonoBehaviour
{
    [SerializeField] private PartyManager partyManager;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void HeroSelected(Party party)
    {
        partyManager.PublishHeroSelected(party);
    }
    public void HeroSelected(int heroIndex)
    {
        partyManager.PublishHeroSelected(partyManager.heroes[heroIndex]);
    }



}
