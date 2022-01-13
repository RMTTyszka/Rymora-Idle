using Global;
using Heroes;
using UnityEngine;

public class ItemListComponent : MonoBehaviour
{

    public PartyManager partyManager;
    public InventoryItemComponent itemPrefab;
    void Start()
    {
        partyManager.OnHeroSelected += UpdateList;
        partyManager.OnInventoryUpdate += UpdateList;
    }

    public void UpdateList(Creature hero)
    {
        foreach (var child in GetComponentsInChildren<InventoryItemComponent>())
        {
            Destroy(child.gameObject);
        }
        foreach (var item in hero.Inventory.GroupedItems)
        {
            var tempTextBox = Instantiate(itemPrefab, Vector3.zero, transform.rotation) as InventoryItemComponent;
            //Parent to the panel
            tempTextBox.transform.SetParent(transform, false);
            tempTextBox.Instantiate(item.Key, item.Value, this);
        }   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RemoveItem(Item item, int quantity)
    {
        partyManager.CurrentPartyNode.RemoveItem(item, quantity);
    }
}
