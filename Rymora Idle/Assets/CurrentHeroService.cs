using Heroes;
using UnityEngine;

public class CurrentHeroService : MonoBehaviour
{
    [SerializeField] private PartyManager partyManager;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void HeroSelected(PartyNode partyNode)
    {
        partyManager.PublishHeroSelected(partyNode);
    }
    public void HeroSelected(int heroIndex)
    {
        partyManager.PublishHeroSelected(partyManager.parties[heroIndex]);
    }



}
