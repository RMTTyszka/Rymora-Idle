using System.Collections.Generic;
using Global;
using UnityEngine;

[CreateAssetMenu(fileName = "Monsters", menuName = "Encounter")]
public class Encounter : ScriptableObject
{
    public List<CreatureTemplate> monsters;
}