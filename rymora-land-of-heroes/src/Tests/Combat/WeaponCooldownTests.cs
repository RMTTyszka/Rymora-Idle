using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Combat;

public sealed class WeaponCooldownTests
{
    [Fact]
    public void Tick_marks_cooldown_ready_when_time_reaches_total()
    {
        var scenario = CombatObjectMother.ReadyCooldownAfterTick();

        scenario.InputCooldown.Tick(scenario.InputDeltaTime);

        Assert.Equal(scenario.ExpectedIsReady, scenario.InputCooldown.IsReady);
    }
}
