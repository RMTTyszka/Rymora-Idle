using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "list", menuName = "Itens/List")]
public class ItemList : ScriptableObject
{
    public List<ItemTemplate> itens;
}