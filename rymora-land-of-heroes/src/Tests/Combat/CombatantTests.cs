using RymoraLandOfHeroes.Core.Combat;
using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Combat;

public sealed class CombatantTests
{
    [Fact]
    public void Constructor_creates_main_hand_cooldown_from_attack_speed()
    {
        var scenario = CombatObjectMother.CreatureWithMainHandAttackSpeedBonus();

        var combatant = new Combatant(scenario.InputCreature);

        Assert.Equal(scenario.ExpectedTotalCooldown, combatant.MainHandCooldown.TotalCooldown);
    }

    [Fact]
    public void Constructor_creates_offhand_cooldown_from_attack_speed()
    {
        var scenario = CombatObjectMother.CreatureWithOffhandAttackSpeedBonus();

        var combatant = new Combatant(scenario.InputCreature);

        Assert.Equal(scenario.ExpectedTotalCooldown, combatant.OffhandCooldown!.TotalCooldown);
    }
}
