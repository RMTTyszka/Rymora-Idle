using System;
using System.Collections;
using System.Collections.Generic;
using Heroes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class CurrentHeroPanel : MonoBehaviour
{


    public PartyManager partyManager;
    public Hero CurrentHero { get; set; }

    public Text heroName;
    public Text heroLevel;
    
    // Start is called before the first frame update
    void Start()
    {
        partyManager.OnHeroSelected += UpdateCurrentIndoDisplayData;
    }

    public void UpdateCurrentIndoDisplayData(Hero hero)
    {
        CurrentHero = hero;
        heroName.text = hero.Name;
        heroLevel.text = hero.Level.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
