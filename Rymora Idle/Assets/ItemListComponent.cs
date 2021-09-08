using System.Collections;
using System.Collections.Generic;
using Heroes;
using UnityEngine;
using UnityEngine.UI;

public class ItemListComponent : MonoBehaviour
{

    public PartyManager partyManager;
    public Text textPrefab;
    void Start()
    {
        partyManager.OnHeroSelected += UpdateList;
        partyManager.OnInventoryUpdate += UpdateList;
    }

    public void UpdateList(Hero hero)
    {
        foreach (var child in GetComponentsInChildren<Text>())
        {
            Destroy(child.gameObject);
        }
        foreach (var item in hero.Inventory.GroupedItems)
        {
            var tempTextBox = Instantiate(textPrefab, Vector3.zero, transform.rotation) as Text;
            //Parent to the panel
            tempTextBox.transform.SetParent(transform, false);
            //Set the text box's text element font size and style:
            tempTextBox.fontSize = 12;
            //Set the text box's text element to the current textToDisplay:
            tempTextBox.text = $"{item.Key.Name} - {item.Value}";
        }   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
