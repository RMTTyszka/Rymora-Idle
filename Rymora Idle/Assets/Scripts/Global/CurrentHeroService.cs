using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Global;
using UnityEngine;

public class CurrentHeroService : MonoBehaviour
{

    public delegate void HeroSelected(Hero hero);

    public static event HeroSelected OnHeroSelected;
    
    
    public static Hero CurrentHero { get; set; }
    // Start is called before the first frame update

    public static void PublishHeroSelected(Hero hero)
    {
        CurrentHero = hero;
        OnHeroSelected?.Invoke(hero);
    }

}
