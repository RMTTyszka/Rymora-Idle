using System.Collections.Generic;
using Items.Equipables.Weapons;

public class WeaponDamageStatus
{
    public float MinimumDamage { get; set; }
    public float MaximumDamage { get; set; }
    public float AttackSpeed { get; set; }
    public float HitModifier { get; set; }
    public float ArmorPenetration { get; set; }
    public float DamageMultiplier { get; set; }
    public float AttributeMultiplier { get; set; }
    public float CounterRating { get; set; }
}
public class WeaponPropertyStatus
{
    public Skill HitSkill { get; set; }
    public Attribute HitAttribute { get; set; }
    public Attribute DamageAttribute { get; set; }
}

public static class WeaponData
{
    public static WeaponStatus Status(this WeaponInstance weapon)
    {
        var bySize = DataBySize[weapon.Size];
        var byCategory = DataByDamageCategory[weapon.DamageCategory];
        return new WeaponStatus
        {
            MinimumDamage = bySize.MinimumDamage,
            MaximumDamage = bySize.MaximumDamage,
            AttackSpeed = bySize.AttackSpeed,
            HitModifier = bySize.HitModifier,
            ArmorPenetration = bySize.ArmorPenetration,
            DamageMultiplier = bySize.DamageMultiplier,
            AttributeMultiplier = bySize.AttributeMultiplier,
            CounterRating = bySize.CounterRating,
            HitSkill = byCategory.HitSkill,
            HitAttribute = byCategory.HitAttribute,
            DamageAttribute = byCategory.DamageAttribute
        };
    }

    public static Dictionary<WeaponSize, WeaponDamageStatus> DataBySize = new Dictionary<WeaponSize, WeaponDamageStatus>
    {
        {
            WeaponSize.Light, new WeaponDamageStatus
            {
                MinimumDamage = 8f,
                MaximumDamage = 18f,
                AttackSpeed = 5f,
                HitModifier = 0f,
                ArmorPenetration = 0f,
                DamageMultiplier = 1,
                AttributeMultiplier = 1f,
                CounterRating = 15f,
            }
        },     
        {
            WeaponSize.Medium, new WeaponDamageStatus
            {
                MinimumDamage = 8f,
                MaximumDamage = 18f,
                AttackSpeed = 10f,
                HitModifier = 0f,
                ArmorPenetration = 1f,
                DamageMultiplier = 2f,
                AttributeMultiplier = 2f,
                CounterRating = 10f,
            }
        },      
        {
            WeaponSize.Heavy, new WeaponDamageStatus
            {
                MinimumDamage = 8f,
                MaximumDamage = 18f,
                AttackSpeed = 13f,
                HitModifier = 10f,
                ArmorPenetration = 8f,
                DamageMultiplier = 3f,
                AttributeMultiplier = 3f,
                CounterRating = 5f,
            }
        },
    };
    public static Dictionary<WeaponsDamageCategory, WeaponPropertyStatus> DataByDamageCategory = new Dictionary<WeaponsDamageCategory, WeaponPropertyStatus>
    {
        {
            WeaponsDamageCategory.Cutting, new WeaponPropertyStatus
            {
                HitSkill = Skill.Swordmanship,
                HitAttribute = Attribute.Strength,
                DamageAttribute = Attribute.Strength
            }
        },     
        {
            WeaponsDamageCategory.Piercing, new WeaponPropertyStatus
            {
                HitSkill = Skill.Fencing,
                HitAttribute = Attribute.Agility,
                DamageAttribute = Attribute.Agility
            }
        },      
        {
            WeaponsDamageCategory.Ranged, new WeaponPropertyStatus
            {
                HitSkill = Skill.Archery,
                HitAttribute = Attribute.Agility,
                DamageAttribute = Attribute.Agility
            }
        },          
        {
            WeaponsDamageCategory.Thrown, new WeaponPropertyStatus
            {
                HitSkill = Skill.Archery,
                HitAttribute = Attribute.Agility,
                DamageAttribute = Attribute.Strength
            }
        },         
        {
            WeaponsDamageCategory.Catalyst, new WeaponPropertyStatus
            {
                HitSkill = Skill.Magery,
                HitAttribute = Attribute.Intuition,
                DamageAttribute = Attribute.Wisdom
            }
        },       
        {
            WeaponsDamageCategory.None, new WeaponPropertyStatus
            {
                HitSkill = Skill.Wrestling,
                HitAttribute = Attribute.Vitality,
                DamageAttribute = Attribute.Vitality
            }
        },       
        {
            WeaponsDamageCategory.Smashing, new WeaponPropertyStatus
            {
                HitSkill = Skill.Heavyweaponship,
                HitAttribute = Attribute.Vitality,
                DamageAttribute = Attribute.Strength
            }
        },      
    };
}

public class WeaponStatus : WeaponDamageStatus
{
    public Skill HitSkill { get; set; }
    public Attribute HitAttribute { get; set; }
    public Attribute DamageAttribute { get; set; }
}