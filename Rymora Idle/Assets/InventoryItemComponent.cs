using UnityEngine;
using UnityEngine.UI;

public class InventoryItemComponent : MonoBehaviour
{

    public Text itemNameText;
    public Text itemQuantityText;
    public ItemListComponent ItemListComponent { get; set; }
    
    public Item Item { get; set; }
    public int Quantity{ get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Instantiate(Item item, int quantity, ItemListComponent itemListComponent)
    {
        Item = item;
        Quantity = quantity;
        itemNameText.text = $"lv {Item.Level} {Item.Name}";
        itemQuantityText.text = $"{Quantity}";
        ItemListComponent = itemListComponent;
    }

    public void RemoveItem()
    {
        ItemListComponent.RemoveItem(Item, 1);
    }
}
