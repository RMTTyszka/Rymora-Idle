using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Global;
using Heroes;
using UnityEngine;

public class PartyManager : MonoBehaviour
{

    public delegate void HeroSelected(Hero hero);

    public event HeroSelected OnHeroSelected;

    [SerializeField]
    public List<Hero> heroes;

    public Hero CurrentHero { get; set; }
    // Start is called before the first frame update

    public void PublishHeroSelected(Hero hero)
    {
        CurrentHero = hero;
        OnHeroSelected?.Invoke(hero);
        Vector3 position = new Vector3(hero.gameObject.transform.position.x, hero.gameObject.transform.position.y, Camera.main.transform.position.z);
        Camera.main.transform.position = position;
    }

}