using Heroes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SelectPartyButton : MonoBehaviour
{
    private PartyManager PartyManager { get; set; }
    private PartyNode PartyNode { get; set; }
    private Text _heroNameText;
    // Start is called before the first frame update
    void Awake()
    {
        PartyManager = GameObject.FindGameObjectWithTag("PartyManager").GetComponent<PartyManager>();
        _heroNameText = gameObject.GetComponentInChildren<Text>();
    }

    public void LinkPartyNode(PartyNode partyNode)
    {
        PartyNode = partyNode;
        _heroNameText.text = PartyNode.gameObject.name;
    }
   
    public void SelectParty()
    {
        PartyManager.PublishPartySelected(PartyNode);
    }
}
