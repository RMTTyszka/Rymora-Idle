using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Party;
using GameParty = RymoraLandOfHeroes.Core.Party.Party;

namespace RymoraLandOfHeroes.Core.Tests.Party;

public sealed class PartyEncounterStateTests
{
    [Fact]
    public void New_state_starts_with_zero_travel_time()
    {
        var state = new PartyEncounterState();

        Assert.Equal(0f, state.TravelSecondsSinceReset);
    }

    [Fact]
    public void AddTravelTime_accumulates_time()
    {
        var state = new PartyEncounterState();

        state.AddTravelTime(1.5f);
        state.AddTravelTime(0.5f);

        Assert.Equal(2f, state.TravelSecondsSinceReset);
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void AddTravelTime_rejects_invalid_values(float deltaTime)
    {
        var state = new PartyEncounterState();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => state.AddTravelTime(deltaTime));

        Assert.Equal("deltaTime", exception.ParamName);
    }

    [Fact]
    public void Reset_clears_accumulated_time()
    {
        var state = new PartyEncounterState();
        state.AddTravelTime(2f);

        state.Reset();

        Assert.Equal(0f, state.TravelSecondsSinceReset);
    }

    [Fact]
    public void Restore_sets_accumulated_time()
    {
        var state = new PartyEncounterState();

        state.Restore(3.25f);

        Assert.Equal(3.25f, state.TravelSecondsSinceReset);
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void Restore_rejects_invalid_values(float travelSecondsSinceReset)
    {
        var state = new PartyEncounterState();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => state.Restore(travelSecondsSinceReset));

        Assert.Equal("travelSecondsSinceReset", exception.ParamName);
    }

    [Fact]
    public void Party_exposes_stable_encounter_state()
    {
        var party = new GameParty("party-1", new TilePosition(0, 0));

        party.EncounterState.AddTravelTime(1.25f);

        Assert.Same(party.EncounterState, party.EncounterState);
        Assert.Equal(1.25f, party.EncounterState.TravelSecondsSinceReset);
    }
}
