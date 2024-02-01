using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager
{

}

public class DungeonInstance
{
    public List<Encounter> Encounters { get; set; } = new();
    public int Level { get; set; } 
}
