using System.Collections.Generic;
using Items.Equipables.Weapons;
using UnityEngine;

public class Creature
{
    public string Name { get; set; }
    public int Level{ get; set; }
    
    public int MaxLife 
    {
        get 
        {
            // int baseLife = 500;
            int baseLife = 100;
            int vitLife = Attributes.Get(Attribute.Vitality).GetValue(0) * 10;
            return baseLife + vitLife;
        }
    }
    public float LifePercent => ((float)Life)/((float)MaxLife)*100;
    public int Life { get; set; }

    public Inventory Inventory { get; set; } = new();
    public Equipment Equipment { get; set; } = new();

    public Skills Skills { get; set; } = new();
    public Attributes Attributes { get; set; } = new();
    public Properties Properties { get; set; } = new();
    public Combatant Combatant { get; set; }


    public Sprite Sprite { get; set; }
    public Creature()
    {
        Combatant = new Combatant
        {
            Creature = this
        };
    }
    
    public static Creature FromCreature(CreatureTemplate creatureTemplate, int level)
    {
        var creature = new Creature();
        creature.Name = creatureTemplate.creatureName;
        creature.Level = level;
        creature.Inventory = new Inventory();
        creature.Sprite = creatureTemplate.sprite;
        creature.Equipment.Equip(WeaponInstance.FromTemplate(GameData.UnarmedWeapons.RandomElement(), level));
        creature.Equipment.Equip(ArmorInstance.FromTemplate(GameData.Armors.RandomElement(), level));
        creature.Life = creature.MaxLife; 
        return creature;
    }
    public List<BasicAttackResult> RunBasicAttackTurn(List<Creature> enemies, List<Creature> allies)
    {
        var attackResults = new List<BasicAttackResult>();
        var canAttack = CanAttack();
        if (!canAttack)
        {
            return attackResults;
        }

        Combatant.MainWeaponCooldown -= Time.deltaTime * (1 + Properties.Get(Property.AttackSpeed).GetValue(0) / 100f);
        if (Combatant.MainWeaponCooldown <= 0)
        {
            var attackResult = Attack(Equipment.MainHand, enemies, allies, false);
            Combatant.MainWeaponCooldown += Equipment.MainHand.Status().AttackSpeed;
            attackResults.Add(attackResult);
        }
        if (Equipment.Offhand is WeaponInstance)
        {
            Combatant.SecondaryWeaponCooldown -= Time.deltaTime * (1 + Properties.Get(Property.AttackSpeed).GetValue(0) / 100f);
            if (Combatant.SecondaryWeaponCooldown <= 0)
            {
                var attackResult = Attack(Equipment.Offhand as WeaponInstance, enemies, allies, false);
                Combatant.SecondaryWeaponCooldown += (Equipment.Offhand as WeaponInstance).Status().AttackSpeed;
                attackResults.Add(attackResult);
            }
        }

        return attackResults;
    }

    private bool CanAttack()
    {
        // TODO
        return true;
    }

    private BasicAttackResult Attack(WeaponInstance weapon, List<Creature> enemies, List<Creature> allies, bool isCounter)
    {
        var target = Combatant.GetTargetForAutoAttack(enemies, allies);
        return Combatant.Attack(weapon, target, isCounter);
    }

    public BasicAttackResult CheckForCounter(WeaponInstance weapon, Creature creature)
    {
        return Combatant.CheckForCounter(weapon, creature);
    }


    public bool IsAlive => Life > 0;

    private bool CriticalRoll(WeaponInstance weapon, Creature target)
    {
        return Combatant.CriticalRoll(weapon, target);
    }

    public int EvadeRoll(int level)
    {
        return Combatant.EvadeRoll(level);
    }

    public void RunPowerAttack(List<Creature> enemies, List<Creature> allies)
    {
    }

    public float Damage(Creature target, WeaponInstance weapon, bool isCrit)
    {
        return Combatant.Damage(target, weapon, isCrit);
    }

    public void TakeDamage(int damage)
    {
        // TODO maibe notify animation?
        Combatant.TakeDamage(damage);
    }

    public float Fortitude(int attackerLevel)
    {
        return Combatant.Fortitude(attackerLevel);
    }
    public float ArmorPenetration(WeaponInstance weapon)
    {
        return Combatant.ArmorPenetration(weapon);

    }
    public float Protection() {
        return Combatant.Protection();
    }
}

public class BasicAttackResult
{
    public Creature Attacker { get; set; }
    public Creature Target { get; set; }
    public bool Critical { get; set; }
    public bool Hit { get; set; }
    public BasicAttackResult CounterAttack { get; set; }
    public float Value { get; set; }
}

public enum Slot
{ 
    None = 0, 
    MainHand = 1, 
    Offhand = 2, 
    Head = 3, 
    Neck = 4, 
    Chest = 5,
    Wrist = 6, 
    Hand = 7, 
    FingerLeft = 8, 
    FingerRight = 9, 
    Waist = 10, 
    Feet = 11, 
    Extra = 12
};

