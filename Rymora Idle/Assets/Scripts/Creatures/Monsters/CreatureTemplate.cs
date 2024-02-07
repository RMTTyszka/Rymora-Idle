using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monsters", menuName = "Monster")]
public class CreatureTemplate : ScriptableObject
{
    public string name;
    public MonsterClass monsterClass;

}

public enum MonsterClass
{
    Combatant = 0,
    Caster = 1,
    Agile = 2
}
