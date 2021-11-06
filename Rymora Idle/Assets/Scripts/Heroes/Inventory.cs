using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory
{
    public List<Item> Items { get; set; }

    public Dictionary<Item, int> GroupedItems =>
        Items
            .OrderBy(e => e.Name)
            .GroupBy(e => new {e.Name, e.Level})
            .ToDictionary(e => e.First(), e => e.Count());

    public Inventory()
    {
        Items = new List<Item>();
    }

    public void AddItem(Item item)
    {
        if (!Items.Contains(item))
        {
            Items.Add(item);
        }
    }

    public void RemoveItem(Item item, int quantity)
    {
        if (Items.Contains(item))
        {
            for (int i = 0; i < quantity; i++)
            {
                var itemIndex = Items.FindIndex(e => e.Name.Equals(item.Name) && e.Level.Equals(item.Level));
                Items.RemoveAt(itemIndex);
            }
        } 
    }
}
