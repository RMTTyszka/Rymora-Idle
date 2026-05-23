using RymoraLandOfHeroes.Core.Combat;
using RymoraLandOfHeroes.Core.Content;
using RymoraLandOfHeroes.Core.Hero;

namespace RymoraLandOfHeroes.Core.Tests.ObjectMothers;

internal static class CombatObjectMother
{
    public static MainHandCooldownScenario CreatureWithMainHandAttackSpeedBonus()
    {
        var creature = TestObjectMother.CreateCreature("Dual");
        creature.Equipment.MainHand = TestObjectMother.CreateWeapon("Sword", attackSpeed: 2);
        creature.Properties[PropertyType.AttackSpeed].AddPoints(1);

        return new MainHandCooldownScenario(
            InputCreature: creature,
            ExpectedTotalCooldown: 1f);
    }

    public static OffhandCooldownScenario CreatureWithOffhandAttackSpeedBonus()
    {
        var creature = TestObjectMother.CreateCreature("Dual");
        creature.Equipment.MainHand = TestObjectMother.CreateWeapon("Sword", attackSpeed: 2);
        creature.Equipment.Offhand = TestObjectMother.CreateWeapon("Dagger", attackSpeed: 1);
        creature.Properties[PropertyType.AttackSpeed].AddPoints(1);

        return new OffhandCooldownScenario(
            InputCreature: creature,
            ExpectedTotalCooldown: 0.5f);
    }

    public static WeaponCooldownTickScenario ReadyCooldownAfterTick()
    {
        var cooldown = new WeaponCooldown(TestObjectMother.CreateWeapon("Sword", attackSpeed: 1), attackSpeedBonus: 0);

        return new WeaponCooldownTickScenario(
            InputCooldown: cooldown,
            InputDeltaTime: 1,
            ExpectedIsReady: true);
    }

    public static HeroVictoryScenario HeroAgainstWeakMonster()
    {
        var hero = TestObjectMother.CreateCreature("Hero");
        hero.Equipment.MainHand = TestObjectMother.CreateWeapon("Greatsword", attackSpeed: 1, baseDamage: 100);
        var monster = TestObjectMother.CreateCreature("Goblin", new(1, 0));
        var combat = new CombatInstance(
            new[] { hero },
            new[] { monster },
            TestObjectMother.CreateGameConfig().Combat,
            new SequenceRandomSource(1, 1));

        return new HeroVictoryScenario(
            InputCombat: combat,
            InputDeltaTime: 1,
            ExpectedState: CombatState.HeroesVictory);
    }
}

internal sealed record MainHandCooldownScenario(Creature InputCreature, float ExpectedTotalCooldown);

internal sealed record OffhandCooldownScenario(Creature InputCreature, float ExpectedTotalCooldown);

internal sealed record WeaponCooldownTickScenario(WeaponCooldown InputCooldown, float InputDeltaTime, bool ExpectedIsReady);

internal sealed record HeroVictoryScenario(CombatInstance InputCombat, float InputDeltaTime, CombatState ExpectedState);
