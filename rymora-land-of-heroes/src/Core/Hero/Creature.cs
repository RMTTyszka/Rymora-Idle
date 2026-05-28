using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;

namespace RymoraLandOfHeroes.Core.Hero;

public sealed class Creature
{
    public Creature(
        string name,
        StatBlock<AttributeType> attributes,
        StatBlock<SkillType> skills,
        StatBlock<PropertyType> properties,
        Equipment equipment,
        LifeConfig lifeConfig,
        SpriteReference sprite = default)
        : this(name, attributes, skills, properties, equipment, lifeConfig, sprite, life: null, maxLife: null)
    {
    }

    private Creature(
        string name,
        StatBlock<AttributeType> attributes,
        StatBlock<SkillType> skills,
        StatBlock<PropertyType> properties,
        Equipment equipment,
        LifeConfig lifeConfig,
        SpriteReference sprite,
        float? life,
        float? maxLife)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Attributes = attributes;
        Skills = skills;
        Properties = properties;
        Equipment = equipment;
        Sprite = sprite;
        var calculatedMaxLife = lifeConfig.BaseLife + Attributes[AttributeType.Vitality].GetValue() * lifeConfig.VitalityLifeMultiplier;
        MaxLife = maxLife ?? calculatedMaxLife;
        Life = life ?? MaxLife;
    }

    public static Creature Restore(
        string name,
        StatBlock<AttributeType> attributes,
        StatBlock<SkillType> skills,
        StatBlock<PropertyType> properties,
        Equipment equipment,
        SpriteReference sprite,
        float life,
        float maxLife)
    {
        if (!float.IsFinite(maxLife) || !float.IsFinite(life) || maxLife < 0 || life < 0 || life > maxLife)
        {
            throw new ArgumentOutOfRangeException(nameof(life), "Saved life must be between zero and max life.");
        }

        return new Creature(name, attributes, skills, properties, equipment, new LifeConfig(0, 0), sprite, life, maxLife);
    }

    public string Name { get; }
    public float Life { get; private set; }
    public float MaxLife { get; }
    public StatBlock<AttributeType> Attributes { get; }
    public StatBlock<SkillType> Skills { get; }
    public StatBlock<PropertyType> Properties { get; }
    public Equipment Equipment { get; }
    public SpriteReference Sprite { get; }
    public bool IsAlive => Life > 0;

    public void TakeDamage(float amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Damage cannot be negative.");
        }

        Life = Math.Max(0, Life - amount);
    }

    public void Heal(float amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Heal cannot be negative.");
        }

        Life = Math.Min(MaxLife, Life + amount);
    }
}
