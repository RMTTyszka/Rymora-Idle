using System.Linq;
using Global;
using Heroes;

public class Combat
{
    public Party Party { get; set; }
    public EncounterInstance Encounter { get; set; }

    public static Combat FromEncounter(Party party, Encounter encounter, int level)
    {
        var combat = new Combat
        {
            Party = party,
            Encounter = new EncounterInstance
            {
                Monsters = encounter.monsters.Select(monster => Creature.FromCreature(monster, level)).ToList()
            }
        };
        return combat;
    }
}