using System.Collections.Generic;
using Items.Equipables.Weapons;
using UnityEngine;

namespace Global
{
    public partial class Creature
    {
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
        public List<CombatActionResult> ProcessAttack(List<Creature> monsters)
        {
            var actions = new List<CombatActionResult>();
            var hasMainWeapon = Equipment.MainWeapon != null;
            if (hasMainWeapon)
            {
                var isAttackOffCooldown = Equipment.MainWeaponAttackCooldown <= 0;
                if (isAttackOffCooldown)
                {
                    var action = DoAttack(Equipment.MainWeapon, monsters, isCounter: false);
                    actions.AddRange(action);
                    Equipment.MainWeaponAttackCooldown += Equipment.MainWeapon.AttackSpeed;
                }
            }          
            var hasOffWeapon = Equipment.OffWeapon != null;
            if (hasOffWeapon)
            {
                var isAttackOffCooldown = Equipment.OffWeaponAttackCooldown <= 0;
                if (isAttackOffCooldown)
                {
                    var action = DoAttack(Equipment.OffWeapon, monsters, isCounter: false);
                    actions.AddRange(action);
                    Equipment.OffWeaponAttackCooldown += Equipment.OffWeapon.AttackSpeed;
                }
            }

            return actions;
        }

        private List<CombatActionResult> DoAttack(Weapon weapon, List<Creature> monsters, bool isCounter)
        {
            var actions = new List<CombatActionResult>();
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
                    var action  = new CombatActionResult
                    {
                        Target = target,
                        Performer = this,
                        ActionType = isCounter ? CombatActionType.CounterPhysicalDamage : CombatActionType.PhysicalDamage
                    };
                    var isCritical = CriticalRoll(weapon, target);
                    if (isCritical)
                    {
                        action.ActionType = isCounter ? CombatActionType.CounterCriticalDamage : CombatActionType.CriticalDamage;
                    }

                    float damage = Damage(target ,weapon, isCritical);
                    action.Value = damage;
                    target.TakeDamage(damage, isCritical, isCounter);
                    actions.Add(action);
                } else if (!isCounter){
                    var counterResult = target.CheckForCounter(weapon, this);
                    actions.AddRange(counterResult);
                }
            }

            return actions;
        }

        private List<CombatActionResult> CheckForCounter(Weapon weapon, Creature target)
        {
            var actions = new List<CombatActionResult>();
            int counter = weapon.CounterRating + Properties.CounterAttackRating.TotalBonus;
            counter += CheckForArmorAndWeaponCounter(weapon, target);
            int roll = Random.Range(0,100);
            if (roll <= counter) {
                actions = DoAttack( weapon, new List<Creature>{target}, true);
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

            return actions;
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

        public void TakeHealing(float healAmount)
        {
            CurrentLife += Mathf.RoundToInt(healAmount);
            if (CurrentLife > MaxLife()) {
                CurrentLife = MaxLife();
                IsAlive = true;
                //Destroy(gameObject);
            }
        }

    }
}