using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Party;

public sealed class PartyTests
{
    [Fact]
    public void New_party_has_no_leader()
    {
        var scenario = PartyObjectMother.NewPartyWithoutMembers();

        var leader = scenario.InputParty.Leader;

        Assert.Equal(scenario.ExpectedLeader, leader);
    }

    [Fact]
    public void AddMember_sets_first_member_as_leader()
    {
        var scenario = PartyObjectMother.EmptyPartyAndFirstMember();

        scenario.InputParty.AddMember(scenario.InputMember);

        Assert.Same(scenario.ExpectedLeader, scenario.InputParty.Leader);
    }

    [Fact]
    public void RemoveMember_promotes_next_member_to_leader()
    {
        var scenario = PartyObjectMother.PartyWithTwoMembers();

        scenario.InputParty.RemoveMember(scenario.InputMember);

        Assert.Same(scenario.ExpectedLeader, scenario.InputParty.Leader);
    }
}
