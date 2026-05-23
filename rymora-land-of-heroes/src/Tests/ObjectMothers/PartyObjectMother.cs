using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Hero;
using RymoraLandOfHeroes.Core.Party;
using GameParty = RymoraLandOfHeroes.Core.Party.Party;

namespace RymoraLandOfHeroes.Core.Tests.ObjectMothers;

internal static class PartyObjectMother
{
    public static NewPartyScenario NewPartyWithoutMembers()
    {
        return new NewPartyScenario(
            InputParty: new GameParty("party-1", new TilePosition(3, 4)),
            ExpectedLeader: null);
    }

    public static AddMemberScenario EmptyPartyAndFirstMember()
    {
        var first = TestObjectMother.CreateCreature("Ada");

        return new AddMemberScenario(
            InputParty: new GameParty("party-1", new TilePosition(3, 4)),
            InputMember: first,
            ExpectedLeader: first);
    }

    public static RemoveMemberScenario PartyWithTwoMembers()
    {
        var first = TestObjectMother.CreateCreature("Ada");
        var second = TestObjectMother.CreateCreature("Ben");
        var party = new GameParty("party-1", new TilePosition(3, 4));
        party.AddMember(first);
        party.AddMember(second);

        return new RemoveMemberScenario(
            InputParty: party,
            InputMember: first,
            ExpectedLeader: second);
    }

    public static StartNextActionScenario QueueWithPendingAction()
    {
        var queue = new PartyActionQueue();
        queue.Enqueue(new PartyActionRequest(PartyActionType.Mine, PartyActionEndType.ByCount, 1, LimitCount: 1));

        return new StartNextActionScenario(
            InputQueue: queue,
            ExpectedStarted: true);
    }

    public static CompleteActionScenario QueueWithCompletedCurrentAction()
    {
        var queue = new PartyActionQueue();
        queue.Enqueue(new PartyActionRequest(PartyActionType.Mine, PartyActionEndType.ByCount, 1, LimitCount: 1));
        var current = queue.StartNextIfIdle()!;
        current.AddProgress(1);
        current.MarkExecuted();

        return new CompleteActionScenario(
            InputQueue: queue,
            ExpectedCurrent: null);
    }
}

internal sealed record NewPartyScenario(GameParty InputParty, Creature? ExpectedLeader);

internal sealed record AddMemberScenario(GameParty InputParty, Creature InputMember, Creature ExpectedLeader);

internal sealed record RemoveMemberScenario(GameParty InputParty, Creature InputMember, Creature ExpectedLeader);

internal sealed record StartNextActionScenario(PartyActionQueue InputQueue, bool ExpectedStarted);

internal sealed record CompleteActionScenario(PartyActionQueue InputQueue, PartyActionState? ExpectedCurrent);
