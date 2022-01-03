using Heroes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SelectHeroButton : MonoBehaviour
{
    [FormerlySerializedAs("_selectHeroService")] [SerializeField] private CurrentHeroService currentHeroService;
    [FormerlySerializedAs("party")] [FormerlySerializedAs("hero")] [SerializeField] private PartyNode partyNode;
    private Text _heroNameText;

    // Start is called before the first frame update
    void Start()
    {
        _heroNameText = gameObject.GetComponentInChildren<Text>();
        _heroNameText.text = partyNode.Party.Hero.Name;
    }
   
    public void HeroSelected()
    {
        currentHeroService.HeroSelected(partyNode);
    }
}
