using GameParty = RymoraLandOfHeroes.Core.Party.Party;

namespace RymoraLandOfHeroes.Core.Application;

public sealed class PartyRegistry
{
    private readonly Dictionary<string, GameParty> _parties;

    public PartyRegistry(IEnumerable<GameParty> parties)
    {
        _parties = parties.ToDictionary(party => party.Id);
    }

    public IReadOnlyCollection<GameParty> All => _parties.Values;
    public GameParty? Selected { get; private set; }

    public GameParty Get(string partyId)
    {
        return _parties.TryGetValue(partyId, out var party)
            ? party
            : throw new KeyNotFoundException($"Party not found: {partyId}.");
    }

    public bool TryGet(string partyId, out GameParty party) => _parties.TryGetValue(partyId, out party!);

    public void Select(string partyId)
    {
        Selected = Get(partyId);
    }
}
