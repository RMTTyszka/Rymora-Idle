using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Hero;

namespace RymoraLandOfHeroes.Core.Party;

public sealed class Party
{
    private readonly List<Creature> _members = new();

    public Party(string id, TilePosition position, Inventory? inventory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
        Position = position;
        Inventory = inventory ?? new Inventory();
    }

    public string Id { get; }
    public IReadOnlyList<Creature> Members => _members;
    public Creature? Leader => _members.Count == 0 ? null : _members[0];
    public PartyActionQueue ActionQueue { get; } = new();
    public Inventory Inventory { get; }
    public TilePosition Position { get; set; }
    public bool IsInCombat { get; set; }
    public bool HasHero => _members.Count > 0;
    public bool IsAlive => _members.Any(member => member.IsAlive);
    public bool IsDefeated => HasHero && _members.All(member => !member.IsAlive);
    public bool CanRunMapActions => Leader?.IsAlive == true && !IsInCombat;

    public void AddMember(Creature hero)
    {
        if (_members.Contains(hero))
        {
            return;
        }

        _members.Add(hero);
    }

    public bool RemoveMember(Creature hero) => _members.Remove(hero);
}
