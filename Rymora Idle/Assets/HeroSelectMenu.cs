using System.Collections;
using System.Collections.Generic;
using Heroes;
using UnityEngine;

public class HeroSelectMenu : MonoBehaviour
{
    public PartyManager PartyManager { get; set; }
    public SelectHeroButton heroButtonPrefab;

    // Start is called before the first frame update
    void Start()
    {
        PartyManager = GameObject.FindGameObjectWithTag("PartyManager").GetComponent<PartyManager>();
        PartyManager.OnPartySelected += OnPartySelected;
    }

    private void OnPartySelected(PartyNode partynode)
    {
        foreach (var child in GetComponentsInChildren<SelectHeroButton>())
        {
            Destroy(child.gameObject);
        }
        foreach (var hero in PartyManager.CurrentPartyNode.Party.Members)
        {
            var heroButton = Instantiate(heroButtonPrefab, Vector3.zero, transform.rotation);
            heroButton.transform.SetParent(transform);
            heroButton.LinkHero(hero);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
