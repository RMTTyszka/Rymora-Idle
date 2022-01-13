using Global;
using Heroes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SelectHeroButton : MonoBehaviour
{
    private PartyManager PartyManager { get; set; }
    private Creature Hero { get; set; }
    private Text _heroNameText;
    // Start is called before the first frame update
    void Awake()
    {
        PartyManager = GameObject.FindGameObjectWithTag("PartyManager").GetComponent<PartyManager>();
        _heroNameText = gameObject.GetComponentInChildren<Text>();
    }

    public void LinkHero(Creature hero)
    {
        Hero = hero;
        _heroNameText.text = Hero.Name;
    }
   
    public void SelectHero()
    {
        PartyManager.PublishHeroSelected(Hero);
    }
}
