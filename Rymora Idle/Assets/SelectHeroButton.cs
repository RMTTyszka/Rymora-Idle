using System.Collections;
using System.Collections.Generic;
using Heroes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SelectHeroButton : MonoBehaviour
{
    [FormerlySerializedAs("_selectHeroService")] [SerializeField] private CurrentHeroService currentHeroService;
    [FormerlySerializedAs("hero")] [SerializeField] private Party party;
    private Text _heroNameText;

    // Start is called before the first frame update
    void Start()
    {
        _heroNameText = gameObject.GetComponentInChildren<Text>();
        _heroNameText.text = party.Hero.Name;
    }
   
    public void HeroSelected()
    {
        currentHeroService.HeroSelected(party);
    }
}
