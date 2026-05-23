using RymoraLandOfHeroes.Core.Hero;
using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Hero;

public sealed class CreatureTests
{
    [Fact]
    public void Constructor_calculates_max_life_from_life_config()
    {
        var scenario = HeroObjectMother.CreatureConstructorInput();

        var creature = new Creature(
            scenario.InputName,
            scenario.InputAttributes,
            scenario.InputSkills,
            scenario.InputProperties,
            scenario.InputEquipment,
            scenario.InputLifeConfig,
            scenario.InputSprite);

        Assert.Equal(scenario.ExpectedMaxLife, creature.MaxLife);
    }

    [Fact]
    public void TakeDamage_clamps_life_at_zero()
    {
        var scenario = HeroObjectMother.LivingCreatureAndFatalDamage();

        scenario.InputCreature.TakeDamage(scenario.InputDamage);

        Assert.Equal(scenario.ExpectedLife, scenario.InputCreature.Life);
    }

    [Fact]
    public void Heal_clamps_life_at_max_life()
    {
        var scenario = HeroObjectMother.InjuredCreatureAndLargeHeal();

        scenario.InputCreature.Heal(scenario.InputHeal);

        Assert.Equal(scenario.ExpectedLife, scenario.InputCreature.Life);
    }
}
