using System;
using Global;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterTemplate : MonoBehaviour
{
    public Creature Creature { get; set; }

    public Sprite image;
    public string name;

    public AttributeQuality strength;
    public AttributeQuality agility;
    public AttributeQuality vitality;
    public AttributeQuality intuition;
    public AttributeQuality wisdom;
    public AttributeQuality charisma;
    public AttributeQuality mine;

    public Creature InstantiateMonster(int level)
    {
        var creature = new Creature();
        creature.Name = name;
        creature.Level = level;
        UpdateAttributes(creature);
        return creature;
    }

    private void UpdateAttributes(Creature creature)
    {
        creature.Attributes.Strength = GetAttributeValue(creature.Level, strength);
        creature.Attributes.Agility = GetAttributeValue(creature.Level, agility);
        creature.Attributes.Vitality = GetAttributeValue(creature.Level, vitality);
        creature.Attributes.Wisdom = GetAttributeValue(creature.Level, wisdom);
        creature.Attributes.Intuition = GetAttributeValue(creature.Level, intuition);
        creature.Attributes.Charisma = GetAttributeValue(creature.Level, charisma);
    }

    private Attribute GetAttributeValue(int creatureLevel, AttributeQuality attributeQuality)
    {
        var qualitityModifier = (int) attributeQuality - 2;
        var baseValue = creatureLevel * (5 - 5 * qualitityModifier);
        baseValue = baseValue + Random.Range(-5, 6);
        baseValue = Math.Max(5, baseValue);
        return new Attribute(baseValue);
    }
}

public enum AttributeQuality
{
    ReallyBad = 0,
    Bad = 1,
    Normal = 2,
    Good = 3,
    VeryGood = 4
}
