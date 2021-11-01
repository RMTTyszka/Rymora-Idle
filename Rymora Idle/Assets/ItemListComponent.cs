using System.Collections;
using System.Collections.Generic;
using Heroes;
using UnityEngine;
using UnityEngine.UI;

public class ItemListComponent : MonoBehaviour
{

    public PartyManager partyManager;
    public InventoryItemComponent itemPrefab;
    void Start()
    {
        partyManager.OnHeroSelected += UpdateList;
        partyManager.OnInventoryUpdate += UpdateList;
    }

    public void UpdateList(Hero hero)
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
            tempTextBox.Instantiate(item.Key, item.Value);
        }   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
