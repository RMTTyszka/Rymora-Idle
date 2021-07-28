using System.Collections;
using System.Collections.Generic;
using Heroes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SelectHeroButton : MonoBehaviour
{
    [SerializeField] private SelectHeroService _selectHeroService;
    [SerializeField] private Hero hero;
    private Text _heroNameText;

    // Start is called before the first frame update
    void Start()
    {
        _heroNameText = gameObject.GetComponentInChildren<Text>();
        _heroNameText.text = hero.Name;
    }
   
    public void HeroSelected()
    {
        _selectHeroService.HeroSelected(hero);
    }
}
