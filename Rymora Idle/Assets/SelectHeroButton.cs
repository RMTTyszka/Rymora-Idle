using System.Collections;
using System.Collections.Generic;
using Heroes;
using UnityEngine;

public class SelectHeroButton : MonoBehaviour
{
    [SerializeField] private SelectHeroService _selectHeroService;
    [SerializeField] private Hero _hero;

    // Start is called before the first frame update
    void Start()
    {
    }
   
    public void HeroSelected()
    {
        _selectHeroService.HeroSelected(_hero);
    }
}
