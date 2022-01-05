using System;
using System.Collections.Generic;
using Items.Equipables.Weapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Global
{
    public class Creature
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
        
        public float GetAggro() {
            // Less health == More Aggro, and bonus
            return ((1f-LifePercent)*10 + Properties.Threat.TotalBonus);
        }
        public Creature GetAttackTarget(List<Creature> targets) {
            // Get target from a list of possible targets, ahd pick the one with the highest aggro

            if (targets == null) {
                return null;
            }
            Creature target = null;
            foreach (Creature tar in targets) {
                if (target == null) {
                    target = tar;
                } else {
                    if (tar.GetAggro() > target.GetAggro()) {
                        target = tar;
                    }
                }
            }
            return target;
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

        public void ProcessAttack(List<Creature> monsters)
        {
            var hasMainWeapon = Equipment.MainWeapon != null;
            if (hasMainWeapon)
            {
                var isAttackOffCooldown = Equipment.MainWeaponAttackCooldown <= 0;
                if (isAttackOffCooldown)
                {
                    DoAttack(Equipment.MainWeapon, monsters, isCounter: false);
                    Equipment.MainWeaponAttackCooldown += Equipment.MainWeapon.AttackSpeed;
                }
            }          
            var hasOffWeapon = Equipment.OffWeapon != null;
            if (hasOffWeapon)
            {
                var isAttackOffCooldown = Equipment.OffWeaponAttackCooldown <= 0;
                if (isAttackOffCooldown)
                {
                    DoAttack(Equipment.OffWeapon, monsters, isCounter: false);
                    Equipment.OffWeaponAttackCooldown += Equipment.OffWeapon.AttackSpeed;
                }
            }
        }

        private void DoAttack(Weapon weapon, List<Creature> monsters, bool isCounter)
        {
            if (IsActive && !IsCasting)
            {
                var target = GetAttackTarget(monsters);
                var attkRoll = Skills.RollForModifier(weapon.AttackSkill, target.Level);
                attkRoll += Attributes.RollForModifier(weapon.AttackAttribute, target.Level);
                attkRoll += Properties.Hit.TotalBonus;
                attkRoll += weapon.HitModifier;
                attkRoll += Random.Range(1, 101);
                //attkRoll += CheckForArmorAndWeaponCritical(target);
                var evadeRoll = target.EvadeRoll(Level);
                //Debug.Log("attacking " + attkRoll + " x " + evadeRoll );
                if (attkRoll >= evadeRoll) {
                    var isCritical = CriticalRoll(weapon, target);
                    float damage = Damage(target ,weapon, isCritical);
                    target.TakeDamage(damage, isCritical, isCounter);
                } else if (!isCounter){
                    target.CheckForCounter(weapon, this);
                }
            }
        }

        private void CheckForCounter(Weapon weapon, Creature target)
        {
            int counter = weapon.CounterRating + Properties.CounterAttackRating.TotalBonus;
            counter += CheckForArmorAndWeaponCounter(weapon, target);
            int roll = Random.Range(0,100);
            if (roll <= counter) {
                DoAttack( weapon, new List<Creature>{target}, true);
    //			int lvlDif = target.C.lvl;
    //			//StartCoroutine(CounterAnim(target));
    //			//Debug.Log("Target is: "+attackTarget);
    //			if (target.isTarget && target != null) {
    //				int attkRoll = C.skills[weapon.attackSkill].GetMod(lvlDif);
    //				attkRoll += C.attributes[weapon.attackAttr].GetMod(lvlDif);
    //				attkRoll += C.Properties[Properties.Attack].GetValue();
    //				attkRoll += (int)weapon.hitMod;
    //				attkRoll += Random.Range(1, 101);
    //				//attkRoll += CheckForArmorAndWeaponCritical(target);
    //				int evadeRoll = target.EvadeRoll(lvlDif);
    //				//Debug.Log("attacking " + attkRoll + " x " + evadeRoll );
    //				if (attkRoll >= evadeRoll) {
    //					bool isCrit = CriticalRoll(weapon,target);
    //					float damage = Damage(target ,weapon, isCrit);
    //					target.takeDamage(damage, isCrit);
    //				} 
    //			}
		    }
        }
        public int CheckForArmorAndWeaponCounter(Weapon weapon, Creature target)
        {
            var armor = target.Equipment.Armor;
            if (weapon.Size == Weapon.WeaponsSize.Light && armor.Size == Armor.ArmorSize.Heavy) {
                return 30;
            }
            else if (weapon.Size == Weapon.WeaponsSize.Heavy && armor.Size == Armor.ArmorSize.Light) {
                return 30;
            }
            else {
                return 0;
            }
        }

        public float Damage(Creature target, Weapon weapon, bool isCrit) {
            // Causa the damage, based on the attacker, the weapon and the target attributes

            float wepDamage = Random.Range(weapon.MinimumDamage, weapon.MaxDamage);
            // TODO remove this
            wepDamage = (weapon.MinimumDamage + weapon.MaxDamage)/2;
            //Debug.Log(wepDamage);
            wepDamage += weapon.GetTotalBonus();
            wepDamage *= weapon.BaseDamageMultiplier;
            // get damage between characters stats
            //ToDo
            float charDamage = DamageModifier(weapon.AttackSkill, target.Level);
            charDamage -= target.Fortitude(target.Level);
            charDamage *= weapon.AttributeDamageMultiplier;
            charDamage += Attributes.RollForModifier(weapon.DamageAttribute, target.Level) * 2;
            //Debug.Log(charDamage);
            //get protection
            float prot = target.Protection() - ArmorPenetration(weapon);
            prot = prot < 0 ? 0 : prot;
            //Debug.Log("prot Bonus == " + properties["protection"].GetValue());
            //Debug.Log(prot);
            //Debug.Log(ArmorPenetration(wep));
            float total = wepDamage + charDamage - prot;
            total = total < 0 ? 0 : total;
            total *= isCrit ? 2f + Properties.CriticalDamage.TotalBonus/100f : 1;
            return total;
        }
        public float DamageModifier(SkillType skillType, int lvl) {
            // Skill + ArmsLore*2 + Bonus
            var skill = Skills.RollForModifier(skillType, lvl);
            var armsLore = Skills.RollForModifier(SkillType.Armslore, lvl);
            var attackBonus = Properties.AttackDamage.TotalBonus;
            return skill + armsLore * 2 + attackBonus;
        }

        private bool CriticalRoll(Weapon weapon, Creature target)
        {
            //Check if it was a critical hit, comparing the attacker and the target
    
            int crit = Properties.Critical.TotalBonus + 15;
            crit += CheckForArmorAndWeaponCritical(weapon, target);
            int resilience = target.Properties.Resiliense.TotalBonus;
            int roll = Random.Range(0,100);
            //Debug.Log(roll + " " + crit +" " + resilience);
            if (roll <= crit-resilience) {
                //	Debug.Log("CritouU");
                return true;
            } else {
                return false;
            }
        
        }
        public int CheckForArmorAndWeaponCritical(Weapon weapon, Creature target) {
            var armor = target.Equipment.Armor;
            if (weapon.Size == Weapon.WeaponsSize.Light && armor.Size == Armor.ArmorSize.Light) 
            {
                return 30;
            }
            if (weapon.Size == Weapon.WeaponsSize.Heavy && armor.Size == Armor.ArmorSize.Heavy) 
            {
                return 30;
            }
            if (weapon.Size == Weapon.WeaponsSize.Medium) 
            {
                return 10;
            }
            return -15;
        }

        private int EvadeRoll(int level)
        {
            // Agi + Tactics + Bonus + Armor
            var roll = Random.Range(1, 101);
            var armor = Equipment.Armor.Evasion;
            var attribute = Attributes.RollForModifier(AttributeType.Agility, level);
            var tactics = Skills.RollForModifier(SkillType.Tactics, level);
            var evasionBonus = Properties.Evasion.TotalBonus;
            var totalValue = roll + armor + attribute + tactics + evasionBonus;
            return totalValue;

        }
        public float ArmorPenetration(Weapon weapon)
        {
            var weaponPenetration = weapon.ArmorPenetration;
            var armorPenetrationBonus = Properties.ArmorPenetration.TotalBonus;
            return weaponPenetration + armorPenetrationBonus;
        }
        public float Fortitude(int level) 
        {
            // Parry + Vit + Bonus
            var parry = Skills.Parry.RollForModifier(level);
            var vitality = Attributes.Vitality.RollForModifier(level);
            var fortitude = Properties.Fortitude.TotalBonus;
            return parry + vitality + fortitude;
        }
        public float Protection() 
        {
            // Armor + Bonus
            var armorProtection = Equipment.Armor.Protection;
            var protectionBonus = Properties.Protection.TotalBonus;
            return armorProtection + protectionBonus;
        }
        public void TakeDamage(float damage, bool isCrit, bool isCounter = false, bool isMagic = false) {
            if (IsAlive) {
                CurrentLife -= Mathf.RoundToInt(damage);
                if (isCrit) {
                    //  InitCBT(Mathf.RoundToInt(damage).ToString(), "CriticalDamage");
                } else if(isCounter) {
                    //  InitCBT(Mathf.RoundToInt(damage).ToString(), "Damage", true);
			
                } else {
                    //  InitCBT(Mathf.RoundToInt(damage).ToString(), "Damage");
                }
                if (CurrentLife < 0) {
                    CurrentLife = 0;
                    IsAlive = false;
                    IsActive = false;
                    Debug.Log(Name + " has died");
                    // this.sprite.GetComponent<SpriteRenderer>().color = Color.gray;
                    // this.spriteOutline.enabled = false;
                    Die();
                    //Destroy(gameObject);
                }
            }
        }

        public void Die() 
        {
            //Destroy(this.gameObject);
        }

        public void PerformCombatAction(List<Creature> allies, List<Creature> enemies)
        {
            if (IsAlive)
            {
                if (!IsCasting)
                {
                    ProcessAttackCooldown();
                    ProcessAttack(enemies);
                }
                else
                {
                    // TODO process casting
                }
            }
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