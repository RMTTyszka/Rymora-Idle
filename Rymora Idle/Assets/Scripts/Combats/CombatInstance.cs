using System.Collections.Generic;
using System.Linq;
using Heroes;
using Items.Equipables.Weapons;
using UnityEngine;

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

    public void RunCombatTurn()
    {
        foreach (var hero in Party.Members.Where(c => c.Combatant.CanAttack()))
        {
            var attackResults = hero.RunBasicAttackTurn(Encounter.Monsters, Party.Members);
            foreach (var attackResult in attackResults)
            {
                CombatManager.BasicAttackPerformed(attackResult);
            }
            hero.RunPowerAttack(Encounter.Monsters, Party.Members);
        }    
        foreach (var monster in Encounter.Monsters.Where(c => c.Combatant.CanAttack()))
        {
            var attackResults = monster.RunBasicAttackTurn( Party.Members, Encounter.Monsters);
            foreach (var attackResult in attackResults)
            {
                CombatManager.BasicAttackPerformed(attackResult);
            }
            monster.RunPowerAttack( Party.Members, Encounter.Monsters);
        }

        if (HeroesWon)
        {
            ProcessLootAndFinish();
        }

        if (HeroesDied)
        {
            ProcessDeath();
        }
    }

    private void ProcessDeath()
    {
        CombatManager.FinishCombat(this, heroesWon: false);
    }

    public bool HeroesDied => Party.Members.All(m => !m.IsAlive);

    private void ProcessLootAndFinish()
    {
        CombatManager.FinishCombat(this, heroesWon: true);
    }

    public bool HeroesWon => Encounter.Monsters.All(m => !m.IsAlive);
}