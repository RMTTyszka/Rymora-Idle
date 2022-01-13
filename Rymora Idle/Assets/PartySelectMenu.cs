using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PartySelectMenu : MonoBehaviour
{

    public PartyManager PartyManager { get; set; }

    [FormerlySerializedAs("selectHeroButtonPrefab")] public SelectPartyButton selectPartyButtonPrefab;
    // Start is called before the first frame update
    void Start()
    {
        PartyManager = GameObject.FindGameObjectWithTag("PartyManager").GetComponent<PartyManager>();
        foreach (var partyNode in PartyManager.parties)
        {
            var selectButton = Instantiate(selectPartyButtonPrefab, Vector3.zero, transform.rotation);
            selectButton.transform.SetParent(transform);
            selectButton.transform.localScale = transform.localScale;
            selectButton.LinkPartyNode(partyNode);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
