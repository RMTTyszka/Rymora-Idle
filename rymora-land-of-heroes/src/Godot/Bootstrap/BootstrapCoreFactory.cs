using System;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.World;
using RymoraLandOfHeroes.GodotAdapter.Content;
using GameParty = RymoraLandOfHeroes.Core.Party.Party;

namespace RymoraLandOfHeroes.GodotAdapter.Bootstrap;

internal static class BootstrapCoreFactory
{
    public const string PartyId = "party-1";
    private const string StartingHeroName = "Hero";

    public static GameApplication CreateApplication(WorldState world, TilePosition startPosition, GameContent content)
    {
        var party = new GameParty(PartyId, startPosition);
        party.AddMember(content.Creatures.CreateCreature(StartingHeroName));

        return new GameApplication(
            world,
            new[] { party },
            content.Config,
            content.Creatures.CreateCreature,
            new FixedRandomSource(1));
    }

    private sealed class FixedRandomSource : IRandomSource
    {
        private readonly int _value;

        public FixedRandomSource(int value)
        {
            _value = value;
        }

        public int NextInclusive(int min, int max)
        {
            return Math.Clamp(_value, min, max);
        }
    }
}
