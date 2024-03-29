using System.Linq;
using Heroes;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public Text weightText;
    public PartyManager PartyManager { get; set; }
    // Start is called before the first frame update
    void Awake()
    {
        PartyManager = FindAnyObjectByType<PartyManager>();
        PartyManager.OnInventoryUpdate += UpdateWeight;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateWeight(Party party)
    {
        weightText.text = $"Weight: {PartyManager.CurrentParty.Hero.Inventory.Items.Sum(item => item.Weight)}";
    }
}
