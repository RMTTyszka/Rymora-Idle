using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Party;

public sealed class PartyActionQueueTests
{
    [Fact]
    public void StartNextIfIdle_starts_first_pending_action()
    {
        var scenario = PartyObjectMother.QueueWithPendingAction();

        var current = scenario.InputQueue.StartNextIfIdle();

        Assert.Equal(scenario.ExpectedStarted, current?.Started);
    }

    [Fact]
    public void CompleteCurrentIfFinished_clears_completed_action()
    {
        var scenario = PartyObjectMother.QueueWithCompletedCurrentAction();

        scenario.InputQueue.CompleteCurrentIfFinished();

        Assert.Equal(scenario.ExpectedCurrent, scenario.InputQueue.Current);
    }
}
