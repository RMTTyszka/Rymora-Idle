using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Combat;

public sealed class CombatInstanceTests
{
    [Fact]
    public void RunTurn_sets_heroes_victory_when_monster_dies()
    {
        var scenario = CombatObjectMother.HeroAgainstWeakMonster();

        scenario.InputCombat.RunTurn(scenario.InputDeltaTime);

        Assert.Equal(scenario.ExpectedState, scenario.InputCombat.State);
    }
}
