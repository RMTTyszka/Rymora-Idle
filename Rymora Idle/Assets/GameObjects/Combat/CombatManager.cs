using System.Collections.Generic;
using System.Linq;
using Global;
using Heroes;
using UnityEngine;

public class CombatManager : MonoBehaviour
{


    public List<CombatEvent> Events { get; set; } = new();

    private List<Party> Parties { get; set; } = new();
    private Party CurrentParty { get; set; }

    private PartyManager PartyManager { get; set; }
    private List<CreatureSpawner> HeroSpawners { get; set; } = new();
    private List<CreatureSpawner> MonsterSpawners { get; set; } = new();
    private List<CreatureSpawner> AnimalSpawners { get; set; } = new();

    public void BasicAttackPerformed(BasicAttackResult attackResult)
    {
        Events.Add(new CombatEvent());
    }

    public void CombatStarted(Party party)
    {
        if (CurrentParty == party)
        {
            InstantiateCombat(party);
        }
    }
    void Awake()
    {
        PartyManager = FindAnyObjectByType<PartyManager>();
        Parties = FindObjectsByType<Party>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        foreach (var creatureSpawner in GetComponentsInChildren<CreatureSpawner>())
        {
            if (creatureSpawner.creatureType == CreatureType.Hero)
            {
                HeroSpawners.Add(creatureSpawner);
            } else if (creatureSpawner.creatureType == CreatureType.Monster)
            {
                MonsterSpawners.Add(creatureSpawner);
            }
            else
            {
                AnimalSpawners.Add(creatureSpawner);
            }
        }
    }

    void Start()
    {
        PartyManager.OnHeroSelected += InstantiateCombat;
    }

    public void InstantiateCombat(Party party)
    {
        CurrentParty = party;
        if (party.InCombat)
        {
            var index = 0;
            foreach (var hero in party.Members)
            {
                HeroSpawners[index++].InstantiateCreature(hero);
            }         
            index = 0;
            foreach (var monster in party.CombatInstance.Encounter.Monsters)
            {
                MonsterSpawners[index++].InstantiateCreature(monster);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var party in Parties)
        {
            if (party.InCombat & party.CombatInstance is not null)
            {
                RunCombat(party.CombatInstance);
            }
        }
    }

    private void RunCombat(CombatInstance partyCombatInstance)
    {
        
    }
}

public class CombatEvent
{
}
