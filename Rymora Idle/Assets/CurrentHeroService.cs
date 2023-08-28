using Heroes;
using UnityEngine;

public class CurrentHeroService : MonoBehaviour
{
    [SerializeField] private PartyManager partyManager;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void HeroSelected(Party party)
    {
        partyManager.PublishHeroSelected(party);
    }
    public void HeroSelected(int heroIndex)
    {
        partyManager.PublishHeroSelected(partyManager.heroes[heroIndex]);
    }



}
