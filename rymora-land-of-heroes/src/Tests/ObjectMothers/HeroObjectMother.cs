using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;
using RymoraLandOfHeroes.Core.Hero;

namespace RymoraLandOfHeroes.Core.Tests.ObjectMothers;

internal static class HeroObjectMother
{
    public static CreatureConstructorScenario CreatureConstructorInput()
    {
        var progression = new ProgressionConfig(5, 5, 5, 5);
        var attributes = StatBlock<AttributeType>.Create(progression.InitialAttributePoints, progression.AttributeValueDivisor);

        return new CreatureConstructorScenario(
            InputName: "Vit",
            InputAttributes: attributes,
            InputSkills: StatBlock<SkillType>.Create(progression.InitialSkillPoints, progression.SkillValueDivisor),
            InputProperties: StatBlock<PropertyType>.Create(0, 1),
            InputEquipment: new Equipment { MainHand = TestObjectMother.CreateWeapon("Training Sword", 1) },
            InputLifeConfig: new LifeConfig(100, 10),
            InputSprite: new SpriteReference("vit"),
            ExpectedMaxLife: 110f);
    }

    public static TakeDamageScenario LivingCreatureAndFatalDamage()
    {
        return new TakeDamageScenario(
            InputCreature: TestObjectMother.CreateCreature("Vit", new LifeConfig(100, 10)),
            InputDamage: 999,
            ExpectedLife: 0f);
    }

    public static HealScenario InjuredCreatureAndLargeHeal()
    {
        var creature = TestObjectMother.CreateCreature("Vit", new LifeConfig(100, 10));
        creature.TakeDamage(50);

        return new HealScenario(
            InputCreature: creature,
            InputHeal: 999,
            ExpectedLife: 110f);
    }
}

internal sealed record CreatureConstructorScenario(
    string InputName,
    StatBlock<AttributeType> InputAttributes,
    StatBlock<SkillType> InputSkills,
    StatBlock<PropertyType> InputProperties,
    Equipment InputEquipment,
    LifeConfig InputLifeConfig,
    SpriteReference InputSprite,
    float ExpectedMaxLife);

internal sealed record TakeDamageScenario(Creature InputCreature, float InputDamage, float ExpectedLife);

internal sealed record HealScenario(Creature InputCreature, float InputHeal, float ExpectedLife);
