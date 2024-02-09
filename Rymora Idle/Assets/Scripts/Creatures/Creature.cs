using System.Collections.Generic;
using Items.Equipables.Weapons;
using UnityEngine;

public class Creature
{
    public string Name { get; set; }
    public int Level{ get; set; }
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
            attackResults.Add(attackResult);
        }
        if (Equipment.Offhand is WeaponInstance)
        {
            Combatant.SecondaryWeaponCooldown -= Time.deltaTime * (1 + Properties.Get(Property.AttackSpeed).GetValue(0) / 100f);
            if (Combatant.SecondaryWeaponCooldown <= 0)
            {
                var attackResult = Attack(Equipment.Offhand as WeaponInstance, enemies, allies, false);
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


    public bool IsAlive => Creature.Life > 0;
    public static int Life { get; set; }

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
        throw new System.NotImplementedException();
    }

    public float Damage(Creature target, WeaponInstance weapon, bool isCrit)
    {
        return Combatant.Damage(target, weapon, isCrit);
    }

    public void TakeDamage(float damage)
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
    public BasicAttackResult CounterAttack { get; set; }
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

