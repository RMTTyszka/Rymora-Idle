using System.Linq;
using Heroes;

public class CombatInstance
{
    public Party Party { get; set; }
    public EncounterInstance Encounter { get; set; }
    public CombatManager CombatManager { get; set; }

    public static CombatInstance FromEncounter(Party party, Encounter encounter, int level)
    {
        var combat = new CombatInstance
        {
            Party = party,
            Encounter = new EncounterInstance
            {
                Monsters = encounter.monsters.Select(monster => Creature.FromCreature(monster, level)).ToList()
            }
        };
        return combat;
    }

    public void RunCombat()
    {
        foreach (var hero in Party.Members)
        {
            
        }
    }
}