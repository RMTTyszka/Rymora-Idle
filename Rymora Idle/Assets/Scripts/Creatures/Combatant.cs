using System.Collections.Generic;
using System.Linq;
using Items.Equipables.Weapons;
using UnityEngine;

public class Combatant
{
    public Creature Creature { get; set; }
    public int Life { get; set; }
    
    public float MainWeaponCooldown { get; set; }
    public float? SecondaryWeaponCooldown { get; set; }
    public int MaxLife 
    {
        get 
        {
            // int baseLife = 500;
            int baseLife = 100;
            int vitLife = Creature.Attributes.Get(Attribute.Vitality).GetValue(0) * 10;
            return baseLife + vitLife;
        }
    }

    public float LifePercent => ((float)Life)/((float)MaxLife)*100;
    
    public Creature GetTargetForAutoAttack(List<Creature> enemies, List<Creature> allies)
    {
        // Get target from a list of possible targets, ahd pick the one with the highest aggro
        var targets =  enemies;
        if (targets == null) {
            return null;
        }
        var target = targets.OrderByDescending(t => t.Combatant.GetAggro()).First();
        return target;
    }
    public float GetAggro() {
        // Less health == More Aggro, and bonus
        return ((1f-LifePercent)*10 + Creature.Properties.Get(Property.Threat).GetValue(0));
    }

    public int EvadeRoll(int level)
    {
        var evadeRoll = Random.Range(1, 101);
        evadeRoll += Creature.Attributes.Get(Attribute.Agility).GetValue(level);
        evadeRoll += Creature.Skills.Get(Skill.Tactics).GetValue(level);
        evadeRoll += Creature.Properties.Get(Property.Evasion).GetValue();
        evadeRoll += ArmorData.DataByCategory[Creature.Equipment.Chest.Category].Evasion;
        return evadeRoll;
    }
    public float Damage(Creature target, WeaponInstance weapon, bool isCrit) {
        // Causa the damage, based on the attacker, the weapon and the target attributes
        var weaponDataByDamageCategory = WeaponData.DataByDamageCategory[weapon.DamageCategory];
        var weaponDataBySize = WeaponData.DataBySize[weapon.Size];
        
        float wepDamage = Random.Range(weaponDataBySize.MinimumDamage, weaponDataBySize.MaximumDamage);
        //   wepDamage = (weapon.minDamage+ weapon.maxDamage)/2;
        //Debug.Log(wepDamage);
        // TODO weapons have bonuses?   wepDamage += weapon.bonusDamage.getBon();
        wepDamage *= weaponDataBySize.DamageMultiplier;
        // get damage between characters stats
        //ToDo
        float charDamage = GetWeaponDamageModifier(weaponDataByDamageCategory.HitSkill, target.Level);
        charDamage -= target.Fortitude(Creature.Level);
        charDamage *= weaponDataBySize.AttributeMultiplier;
        charDamage += Creature.Attributes.Get(weaponDataByDamageCategory.DamageAttribute).GetValue(target.Level) * 2;
        //Debug.Log(charDamage);
        //get protection
        float prot = target.Protection() - ArmorPenetration(weapon);
        prot = prot < 0 ? 0 : prot;
        //Debug.Log("prot Bonus == " + properties["protection"].GetValue());
        //Debug.Log(prot);
        //Debug.Log(ArmorPenetration(wep));
        float total = wepDamage + charDamage - prot;
        total = total < 0 ? 0 : total;
        total *= isCrit ? 2f + Creature.Properties.Get(Property.CriticalDamage).GetValue()/100f : 1;
        return total;
    }
    public void TakeDamage(float damage) {
        if (Creature.IsAlive) {
            Debug.Log(Creature.Name + $" took {damage} damage");
            Creature.Combatant.Life -= Mathf.RoundToInt(damage);
            if (Creature.Combatant.Life < 0) {
                Creature.Combatant.Life = 0;
                Debug.Log(Creature.Name + " has died");
                /*this.sprite.GetComponent<SpriteRenderer>().color = Color.gray;
                this.spriteOutline.enabled = false;*/
                //Destroy(gameObject);
            }
        }
    }
    public float ArmorPenetration(WeaponInstance weapon)
    {
        var weaponsStatus = weapon.Status();
        return (weaponsStatus.ArmorPenetration
                + Creature.Properties.Get(Property.ArmorPenetration).GetValue());
    }
    public float Protection() {
        // Armor + Bonus
        var armor = Creature.Equipment.Chest;
        var armorDataByCategory = armor.ArmorStatus();
        return (Creature.Properties.Get(Property.Protection).GetValue()
                + armorDataByCategory.Protection);
    }
    public float Fortitude(int lvl) {
        // Parry + Vit + Bonus
        return (Creature.Skills.Get(Skill.Parry).GetValue(lvl) 
                + Creature.Attributes.Get(Attribute.Vitality).GetValue(lvl) 
                + Creature.Properties.Get(Property.Fortitude).GetValue());
    }
    private float GetWeaponDamageModifier(Skill skill, int lvl) {
        // Skill + ArmsLore*2 + Bonus
        return Creature.Skills.Get(skill).GetValue(lvl)
                + Creature.Skills.Get(Skill.Armslore).GetValue(lvl) * 2
                + Creature.Properties.Get(Property.AttackDamage).GetValue();
    }
    public bool CriticalRoll(WeaponInstance weapon, Creature target)
    {
//Check if it was a critical hit, comparing the attacker and the target

        var crit = Creature.Properties.Get(Property.Critical).GetValue(0) + 15;
        crit += CheckForArmorAndWeaponCritical(weapon, target);
        int resilience = target.Properties.Get(Property.Resiliense).GetValue(0);
        int roll = Random.Range(0,100);
        //Debug.Log(roll + " " + crit +" " + resilience);
        if (roll <= crit-resilience) {
            //	Debug.Log("CritouU");
            return true;
        } else {
            return false;
        }    
    }
    public int CheckForArmorAndWeaponCritical(WeaponInstance weapon, Creature target) {
        var armor = target.Equipment.Chest;
        if (weapon.Size == WeaponSize.Light && armor.Category == ArmorCategory.Light) {
            return 30;
        }

        if (weapon.Size == WeaponSize.Heavy && armor.Category == ArmorCategory.Heavy) {
            return 30;
        }

        if (weapon.Size == WeaponSize.Medium) {
            return 10;
        }
        return -15;
    }

    public BasicAttackResult CheckForCounter(WeaponInstance weapon, Creature target)
    {
        var counter = (int)weapon.Status().CounterRating + Creature.Properties.Get(Property.Counter).GetValue();
        counter += CheckForArmorAndWeaponCounter(weapon, target);
        var roll = Random.Range(0,100);
        if (roll <= counter) {
            return Attack(weapon, target, true);
        }
        return null;
    }
    public int CheckForArmorAndWeaponCounter(WeaponInstance weapon, Creature target) {
        var armor = target.Equipment.Chest;
        if (weapon.Size == WeaponSize.Light && armor.Category == ArmorCategory.Heavy) {
            return 30;
        }

        if (weapon.Size == WeaponSize.Heavy && armor.Category == ArmorCategory.Light) {
            return 30;
        }

        return 0;
    }

    public BasicAttackResult Attack(WeaponInstance weapon, Creature target, bool isCounter)
    {
        var result = new BasicAttackResult();
        var weaponDataByDamageCategory = WeaponData.DataByDamageCategory[weapon.DamageCategory];
        var weaponDataBySize = WeaponData.DataBySize[weapon.Size];
        var hitRoll = Random.Range(1, 101);
        hitRoll += Creature.Skills.Get(weaponDataByDamageCategory.HitSkill).GetValue(target.Level);
        hitRoll += Creature.Attributes.Get(weaponDataByDamageCategory.HitAttribute).GetValue(target.Level);
        hitRoll += Creature.Properties.Get(Property.Hit).GetValue();
        hitRoll += (int)weaponDataBySize.HitModifier;

        var evadeRoll = target.EvadeRoll(Creature.Level);
        if (hitRoll >= evadeRoll) {
            var isCrit = CriticalRoll(weapon, target);
            var damage = Damage(target ,weapon, isCrit);
            target.TakeDamage(damage);
            Debug.Log($"{Creature.Name} has hit {target.Name} for {damage} damage" );
        } else if (!isCounter){
            var counterAttack = target.CheckForCounter(weapon, Creature);
            result.CounterAttack = counterAttack;
        }

        return result;
    }

    public bool CanAttack()
    {
        return Creature.IsAlive;
    }
}

public enum TargetType
{
    Enemy = 0,
    Ally = 1,
    Self = 2
}