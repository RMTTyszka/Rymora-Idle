using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Heroes;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public Text weightText;
    public PartyManager PartyManager { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        PartyManager = FindObjectsOfType<PartyManager>().First();
        PartyManager.OnInventoryUpdate += UpdateWeight;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateWeight(PartyNode partyNode)
    {
        weightText.text = $"Weight: {PartyManager.CurrentPartyNode.Party.Hero.Inventory.Items.Sum(item => item.Weight)}";
    }
}
