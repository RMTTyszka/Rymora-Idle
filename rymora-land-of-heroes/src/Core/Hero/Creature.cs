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
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Attributes = attributes;
        Skills = skills;
        Properties = properties;
        Equipment = equipment;
        Sprite = sprite;
        MaxLife = lifeConfig.BaseLife + Attributes[AttributeType.Vitality].GetValue() * lifeConfig.VitalityLifeMultiplier;
        Life = MaxLife;
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
