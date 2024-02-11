using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Monsters", menuName = "Monster")]
public class CreatureTemplate : ScriptableObject
{
    [FormerlySerializedAs("name")] public string creatureName;
    public MonsterClass monsterClass;
    // TODO weapon and armor templates
    public Sprite sprite;

}

public enum MonsterClass
{
    Combatant = 0,
    Caster = 1,
    Agile = 2
}
