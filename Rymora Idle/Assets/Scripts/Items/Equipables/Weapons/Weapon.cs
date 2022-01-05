using System;

namespace Items.Equipables.Weapons
{
    public class Weapon : Equipable
    {
        public enum WeaponsSize {Light, Medium, Heavy};
        public enum WeaponsDamageType {Smashing, Piercing, Cutting, Catalyst, None, Ranged, Thrown};
        
        public WeaponsSize Size { get; set; }
        public float MinimumDamage { get; set; }
        public float MaxDamage { get; set; }
        public float AttackSpeed { get; set; }
        public int HitModifier { get; set; }
        public float ArmorPenetration { get; set; }
        public float BaseDamageMultiplier { get; set; }
        public float AttributeDamageMultiplier { get; set; }
        public int CounterRating { get; set; }
        public float Range { get; set; }
        public SkillType AttackSkill { get; set; }
        public AttributeType AttackAttribute{ get; set; }
        public AttributeType DamageAttribute{ get; set; }


        public Weapon FromSizeAndDamageType(WeaponsSize size, WeaponsDamageType damageType)
        {
            var weapon = new Weapon();
            switch (size)
            {
                case WeaponsSize.Light:
                    weapon.Size = WeaponsSize.Light;
                    weapon.MinimumDamage = 8;
                    weapon.MaxDamage = 18;
                    weapon.AttackSpeed = 5;
                    weapon.HitModifier = 0;
                    weapon.ArmorPenetration = 0;
                    weapon.BaseDamageMultiplier = 1;
                    weapon.AttributeDamageMultiplier = 1;
                    weapon.CounterRating = 15;
                    // weapon.Range = 8;
                    break;
                case WeaponsSize.Medium:
                    weapon.Size = WeaponsSize.Light;
                    weapon.MinimumDamage = 8;
                    weapon.MaxDamage = 18;
                    weapon.AttackSpeed = 19;
                    weapon.HitModifier = 0;
                    weapon.ArmorPenetration = 1;
                    weapon.BaseDamageMultiplier = 2;
                    weapon.AttributeDamageMultiplier = 2;
                    weapon.CounterRating = 10;
                    // weapon.Range = 8;
                    break;
                case WeaponsSize.Heavy:
                    weapon.Size = WeaponsSize.Light;
                    weapon.MinimumDamage = 8;
                    weapon.MaxDamage = 18;
                    weapon.AttackSpeed = 13;
                    weapon.HitModifier = 10;
                    weapon.ArmorPenetration = 8;
                    weapon.BaseDamageMultiplier = 3;
                    weapon.AttributeDamageMultiplier = 3;
                    weapon.CounterRating = 5;
                    // weapon.Range = 8;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, null);

            }
            
            switch (damageType)
            {
                case WeaponsDamageType.Smashing:
                    weapon.AttackSkill = SkillType.Heavyweaponship;
                    weapon.AttackAttribute = AttributeType.Strength;
                    weapon.DamageAttribute = AttributeType.Strength;
                    break;
                case WeaponsDamageType.Piercing:
                    weapon.AttackSkill = SkillType.Fencing;
                    weapon.AttackAttribute = AttributeType.Agility;
                    weapon.DamageAttribute = AttributeType.Agility;
                    break;
                case WeaponsDamageType.Cutting:
                    weapon.AttackSkill = SkillType.Swordmanship;
                    weapon.AttackAttribute = AttributeType.Strength;
                    weapon.DamageAttribute = AttributeType.Strength;
                    break;
                case WeaponsDamageType.Catalyst:
                    weapon.AttackSkill = SkillType.Magery;
                    weapon.AttackAttribute = AttributeType.Intuition;
                    weapon.DamageAttribute = AttributeType.Wisdom;
                    break;
                case WeaponsDamageType.None:
                    weapon.AttackSkill = SkillType.Wrestling;
                    weapon.AttackAttribute = AttributeType.Vitality;
                    weapon.DamageAttribute = AttributeType.Vitality;
                    break;
                case WeaponsDamageType.Ranged:
                    weapon.AttackSkill = SkillType.Archery;
                    weapon.AttackAttribute = AttributeType.Agility;
                    weapon.DamageAttribute = AttributeType.Agility;
                    break;
                case WeaponsDamageType.Thrown:
                    weapon.AttackSkill = SkillType.Archery;
                    weapon.AttackAttribute = AttributeType.Agility;
                    weapon.DamageAttribute = AttributeType.Strength;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null);
            }

            return weapon;
        }
    }
}