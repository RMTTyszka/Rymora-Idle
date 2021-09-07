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
    
    public delegate void ActionUpdated(Hero hero);

    public event ActionUpdated OnActionUpdated;

    [SerializeField]
    public List<Hero> heroes;

    public Hero CurrentHero { get; set; }
    // Start is called before the first frame update

    public void PublishHeroSelected(Hero hero)
    {
        CurrentHero = hero;
        OnHeroSelected?.Invoke(hero);
        Vector3 position = Vector3Int.FloorToInt(new Vector3(hero.gameObject.transform.position.x, hero.gameObject.transform.position.y, Camera.main.transform.position.z));
        Camera.main.transform.position = position;
    }

    public void PublishActionsUpdated(Hero hero)
    {
        OnActionUpdated?.Invoke(hero);
    }

}
