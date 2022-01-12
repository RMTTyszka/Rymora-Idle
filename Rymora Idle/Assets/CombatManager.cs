using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Global;
using Heroes;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public PartyNode PartyNode { get; set; }
    public int partyIndex;
    public int Level { get; set; }
    public PartyManager PartyManager { get; set; }
    public List<Creature> Monsters { get; set; }
    public Dictionary<Creature, CreatureSpawner> SpawwnerByCreature { get; set; }

    public CreatureSpawner monsterSpawner1;
    public CreatureSpawner monsterSpawner2;
    public CreatureSpawner monsterSpawner3;
    public CreatureSpawner monsterSpawner4;
    
    
    public CreatureSpawner heroSpawner1;
    public CreatureSpawner heroSpawner2;
    public CreatureSpawner heroSpawner3;
    public CreatureSpawner animalSpawner;
    public bool CombatStarted { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        PartyManager = GameObject.FindGameObjectWithTag("PartyManager").GetComponent<PartyManager>();
        PartyNode = PartyManager.parties[partyIndex];

        
        Monsters = new List<Creature>();
        SpawwnerByCreature = new Dictionary<Creature, CreatureSpawner>();

    }

    public void InitiateCombat(Encounter encounter)
    {
        SpawwnerByCreature = new Dictionary<Creature, CreatureSpawner>();
        Monsters.Clear();
        foreach (var monster in encounter.Monsters)
        {
            var newMonster = monster.InstantiateMonster(Level);
            Monsters.Add(newMonster);
        }
        SetHero(heroSpawner1,0);
        SetHero(heroSpawner2,1);
        SetHero(heroSpawner3,2);
                
        SetMonster(monsterSpawner1, 0);
        SetMonster(monsterSpawner2, 1);
        SetMonster(monsterSpawner3, 2);
        SetMonster(monsterSpawner4, 3);
     //   SetMonster(animalSpawner, 0);
     
     // TODO reset combat status

     foreach (var partyMember in PartyNode.Party.Members)
     {
         partyMember.IsActive = true;
         partyMember.Equipment.MainWeaponAttackCooldown = partyMember.Equipment.MainWeapon != null
             ? partyMember.Equipment.MainWeapon.AttackSpeed
             : 0;      
         partyMember.Equipment.OffWeaponAttackCooldown = partyMember.Equipment.OffWeapon != null
             ? partyMember.Equipment.OffWeapon.AttackSpeed
             : 0;
         partyMember.CurrentLife = partyMember.MaxLife();
         partyMember.IsAlive = true;
     }     
     foreach (var monster in Monsters)
     {
         monster.IsActive = true;
         monster.Equipment.MainWeaponAttackCooldown = monster.Equipment.MainWeapon != null
             ? monster.Equipment.MainWeapon.AttackSpeed
             : 0;      
         monster.Equipment.OffWeaponAttackCooldown = monster.Equipment.OffWeapon != null
             ? monster.Equipment.OffWeapon.AttackSpeed
             : 0;
     }
     CombatStarted = true;



    }

    private void Attack(Creature hero, List<Creature> monsters)
    {
    }

    private void SetHero(CreatureSpawner creatureSpawner, int index)
    {
        var hero = PartyNode.Party.Members.ElementAtOrDefault(index);
        if (hero != null)
        {
            creatureSpawner.Add(hero);
            SpawwnerByCreature[hero] = creatureSpawner;
        }

    }    
    private void SetMonster(CreatureSpawner creatureSpawner, int index)
    {
        var monster = Monsters.ElementAtOrDefault(index);
        if (monster != null)
        {
            creatureSpawner.Add(monster);
            SpawwnerByCreature[monster] = creatureSpawner;
        }
    }      
    private void SetAnimal(CreatureSpawner creatureSpawner, int index)
    {
        var monster = Monsters.ElementAtOrDefault(index);
        if (monster != null)
        {
            creatureSpawner.Creature = monster;
        }
    }    

    // Update is called once per frame
    void Update()
    {
        if (CombatStarted)
        {
            foreach (var hero in PartyNode.Party.Members)
            {
                var actionsPerformed = hero.PerformCombatAction(PartyNode.Party.Members, Monsters);
                // TODO animations
                foreach (var actionResult in actionsPerformed)
                {
                    var creatureSpawner = SpawwnerByCreature[actionResult.Target];
                    creatureSpawner.ProcessAction(actionResult);
                }
            }      
            foreach (var monster in Monsters)
            {
                var actionsPerformed = monster.PerformCombatAction(Monsters, PartyNode.Party.Members);
                // TODO animations
                foreach (var actionResult in actionsPerformed)
                {
                    var creatureSpawner = SpawwnerByCreature[actionResult.Target];
                    creatureSpawner.ProcessAction(actionResult);
                };
            }

            CheckForCombatEnd();
        }
    }

    private void CheckForCombatEnd()
    {
        if (Monsters.TrueForAll(monster => !monster.IsAlive))
        {
            EndCombat();
        }       
        if (PartyNode.Party.Members.TrueForAll(hero => !hero.IsAlive))
        {
            EndCombat();
        }
    }

    private void EndCombat()
    {
        // TODO show loot
        CombatStarted = false;
        foreach (var spawner in Spawners())
        {
            spawner.Reset();
        }

        PartyNode.InCombat = false;
    }

    private IEnumerable<CreatureSpawner> Spawners()
    {
        yield return heroSpawner1;
        yield return heroSpawner2;
        yield return heroSpawner3;
        yield return monsterSpawner1;
        yield return monsterSpawner2;
        yield return monsterSpawner3;
        yield return monsterSpawner4;
    }
}
