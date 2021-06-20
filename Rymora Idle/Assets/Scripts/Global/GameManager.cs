using System.Collections;
using System.Collections.Generic;
using Global;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        HeroesManager.InstantiateHeroes();
        CurrentHeroService.CurrentHero = HeroesManager.Heroes[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
