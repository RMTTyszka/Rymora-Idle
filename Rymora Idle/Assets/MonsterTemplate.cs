using System;
using Global;
using Items.Equipables.Weapons;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MonsterTemplate : MonoBehaviour
{
    public Sprite image;
    public string monsterName;

    public AttributeQuality strength;
    public AttributeQuality agility;
    public AttributeQuality vitality;
    public AttributeQuality intuition;
    public AttributeQuality wisdom;
    public AttributeQuality charisma;
    public AttributeQuality mine;

    public Weapon.WeaponsSize? mainWeaponSize;
    public Weapon.WeaponsDamageType? mainWeaponDamageType;
    public Weapon.WeaponsSize? offWeaponSize;
    public Weapon.WeaponsDamageType? offWeaponDamageType;
    public Armor.ArmorSize? armorSize;

    public Creature InstantiateMonster(int level)
    {
        var creature = new Creature();
        creature.Name = monsterName;
        creature.Level = level;
        UpdateAttributes(creature);
        creature.Image = image;
        creature.CurrentLife = creature.MaxLife();

        InstantiateEquipment(creature);
        return creature;
    }

    private void InstantiateEquipment(Creature creature)
    {
        if (mainWeaponSize == null)
        {
            var sizeList = Enum.GetValues(typeof(Weapon.WeaponsSize)) as Weapon.WeaponsSize[];
            var randomizedSize = sizeList[Random.Range(0, sizeList.Length)];
            mainWeaponSize = randomizedSize;
        }      
        if (mainWeaponDamageType == null)
        {
            var damageTypeList = Enum.GetValues(typeof(Weapon.WeaponsDamageType)) as Weapon.WeaponsDamageType[];
            var randomizedDamageType= damageTypeList[Random.Range(0, damageTypeList.Length)];
            mainWeaponDamageType = randomizedDamageType;
        }     
        if (armorSize == null)
        {
            var sizeList = Enum.GetValues(typeof(Armor.ArmorSize)) as Armor.ArmorSize[];
            var randomizedSize= sizeList[Random.Range(0, sizeList.Length)];
            armorSize = randomizedSize;
        }

        var mainWeapon = Weapon.FromSizeAndDamageType(mainWeaponSize.Value, mainWeaponDamageType.Value, creature.Level);
        creature.Equipment.Equip(mainWeapon, Slot.Mainhand);
        if (offWeaponSize != null)
        {
            if (offWeaponDamageType == null)
            {
                var damageTypeList = Enum.GetValues(typeof(Weapon.WeaponsDamageType)) as Weapon.WeaponsDamageType[];
                var randomizedDamageType= damageTypeList[Random.Range(0, damageTypeList.Length)];
                offWeaponDamageType = randomizedDamageType;
            } 
            var offWeapon = Weapon.FromSizeAndDamageType(mainWeaponSize.Value, mainWeaponDamageType.Value, creature.Level);
            creature.Equipment.Equip(offWeapon, Slot.Offhand);
        }

        var baseArmor = Armor.FromSize(armorSize.Value);
        var armor = Armor.FromBaseArmor(baseArmor, creature.Level);
        creature.Equipment.Equip(armor, Slot.Chest);
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
