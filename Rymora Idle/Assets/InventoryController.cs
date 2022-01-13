using System.Linq;
using Global;
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

    public void UpdateWeight(Creature hero)
    {
        weightText.text = $"Weight: {hero.Inventory.Items.Sum(item => item.Weight)}";
    }
}
