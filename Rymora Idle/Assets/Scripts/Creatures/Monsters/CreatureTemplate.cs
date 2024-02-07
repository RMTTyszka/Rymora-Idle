using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Monsters", menuName = "Monster")]
public class CreatureTemplate : ScriptableObject
{
    [FormerlySerializedAs("name")] public string creatureName;
    public MonsterClass monsterClass;

}

public enum MonsterClass
{
    Combatant = 0,
    Caster = 1,
    Agile = 2
}
