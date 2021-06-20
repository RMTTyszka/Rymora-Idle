using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class CurrentHeroPanel : MonoBehaviour
{


    public Hero CurrentHero => CurrentHeroService.CurrentHero;

    public Text heroName;
    public Text heroLevel;
    
    // Start is called before the first frame update
    void Start()
    {
        CurrentHeroService.OnHeroSelected += UpdateCurrentIndoDisplayData;
    }

    public void UpdateCurrentIndoDisplayData(Hero hero)
    {
        Debug.Log(heroName);
        heroName.text = hero.Name;
        heroLevel.text = hero.Level.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
