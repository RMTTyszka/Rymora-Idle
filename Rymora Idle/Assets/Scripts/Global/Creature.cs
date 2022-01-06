using System;
using System.Collections.Generic;
using Items.Equipables.Weapons;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Global
{
    public partial class Creature
    {
        public string Name { get; set; }
        public int Level{ get; set; }
        public Inventory Inventory { get; set; }
        public Skills Skills { get; set; }
        public Attributes Attributes { get; set; }
        public Properties Properties { get; set; }
        public Equipment Equipment { get; set; }
        
        
        public Sprite Image { get; set; }

        public int CurrentLife;
        public int MaxLife() 
        {
            int baseLife = 500;
            int vitLife = this.Attributes.Vitality.Value * 10;
            return 	baseLife + vitLife;
        }

        public Creature()
        {
            Inventory = new Inventory();
            Skills = new Skills();
            Attributes = new Attributes();
            Properties = new Properties();
            Equipment = new Equipment();

            IsAlive = true;
        }

        public static Creature FromCreature(Creature creatureTemplate, int level)
        {
            var creature = new Creature();
            creature.Name = creature.Name;
            creature.Level = level;
            creature.Inventory = creature.Inventory;
            return creature;
        }
        
       
        public float LifePercent => CurrentLife/((float)MaxLife())*100;
        public bool IsAlive { get; set; }
        public bool IsCasting { get; set; }
        public bool IsActive { get; set; }

        public void ProcessAttackCooldown()
        {
            if (Equipment.MainWeapon != null)
            {
                Equipment.MainWeaponAttackCooldown -= Time.deltaTime * (1 + Properties.AttackSpeed.TotalBonus/100f);
            }    
            if (Equipment.OffWeapon != null)
            {
                Equipment.OffWeaponAttackCooldown -= Time.deltaTime * (1 + Properties.AttackSpeed.TotalBonus/100f);
            }

        }

        

        [CanBeNull]
        public List<CombatActionResult> PerformCombatAction(List<Creature> allies, List<Creature> enemies)
        {
            var actions = new List<CombatActionResult>();
            if (IsAlive)
            {
                if (!IsCasting)
                {
                    ProcessAttackCooldown();
                    var attackResult = ProcessAttack(enemies);
                    actions.AddRange(attackResult);
                }
                else
                {
                    // TODO process casting
                }
            }

            return actions;
        }
    }


    public class Attributes
    {
        public Attribute Strength { get; set; }
        public Attribute Agility { get; set; }
        public Attribute Vitality { get; set; }
        public Attribute Wisdom { get; set; }
        public Attribute Intuition { get; set; }
        public Attribute Charisma { get; set; }

        public Attributes()
        {
            Strength = new Attribute();
            Agility = new Attribute();
            Vitality = new Attribute();
            Wisdom = new Attribute();
            Intuition = new Attribute();
            Charisma = new Attribute();
        }

        public int RollForModifier(AttributeType attribute, int targetLevel)
        {
            switch (attribute)
            {
                case AttributeType.Strength:
                    return Strength.RollForModifier(targetLevel);
                case AttributeType.Agility:
                    return Agility.RollForModifier(targetLevel);
                case AttributeType.Vitality:
                    return Vitality.RollForModifier(targetLevel);
                case AttributeType.Wisdom:
                    return Wisdom.RollForModifier(targetLevel);
                case AttributeType.Intuition:
                    return Intuition.RollForModifier(targetLevel);
                case AttributeType.Charisma:
                    return Charisma.RollForModifier(targetLevel);
                default:
                    throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null);
            }
        }
    }

    public class Properties
    {
        public Property Threat { get; set; }
        public Property AttackSpeed { get; set; }
        public Property Hit { get; set; }
        public Property Evasion { get; set; }
        public Property Critical { get; set; }
        public Property Resiliense { get; set; }
        public Property AttackDamage { get; set; }
        public Property Fortitude { get; set; }
        public Property Protection { get; set; }
        public Property ArmorPenetration { get; set; }
        public Property CriticalDamage { get; set; }
        public Property CounterAttackRating { get; set; }

        public Properties()
        {
            Threat = new Property();
            AttackSpeed = new Property();
            Hit = new Property();
            Evasion = new Property();
            Critical = new Property();
            Resiliense = new Property();
            AttackDamage = new Property();
            Fortitude = new Property();
            Protection = new Property();
            ArmorPenetration = new Property();
            CriticalDamage = new Property();
            CounterAttackRating = new Property();
        }
    }

    public class Equipment
    {
        public Weapon MainWeapon { get; set; }
        public Weapon OffWeapon { get; set; }
        public Armor Armor { get; set; }
          
          
        public float MainWeaponAttackCooldown { get; set; }
        public float OffWeaponAttackCooldown { get; set; }

        public Equipment()
        {
            Armor = Armor.FromBaseArmor(Armor.ArmorBases.None, 1);
        }
    }
}