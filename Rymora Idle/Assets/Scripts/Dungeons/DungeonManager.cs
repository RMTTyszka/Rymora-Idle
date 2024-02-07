using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager
{
    public void InstantiateDungeon(Place place)
    {
        var level = place.Level();
    }
}

public class DungeonInstance
{
    public List<Encounter> Encounters { get; set; } = new();
    public int Level { get; set; } 
}
