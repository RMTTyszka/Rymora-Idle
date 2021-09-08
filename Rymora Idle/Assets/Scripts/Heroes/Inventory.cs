using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory
{
    public List<Item> Items { get; set; }

    public Dictionary<Item, int> GroupedItems =>
        Items.GroupBy(e => new {e.Name, e.Level}).ToDictionary(e => e.First(), e => e.Count());

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
}
