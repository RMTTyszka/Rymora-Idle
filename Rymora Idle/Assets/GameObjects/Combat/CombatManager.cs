using System.Collections.Generic;
using System.Linq;
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

    public CreatureSpawner GetSpawner(Creature creature)
    {
        return HeroSpawners.Concat(MonsterSpawners).First(spawner => spawner.CreatureBody?.Creature == creature);
    }

    public void BasicAttackPerformed(BasicAttackResult attackResult)
    {
        var target = GetSpawner(attackResult.Target);
        if (attackResult.Hit)
        {
            target.CreatureBody.CombatantUIElements.InitCBT(attackResult.Value.ToString(), GetAttackTrigger(attackResult), false);
        }

        if (attackResult.CounterAttack is not null)
        {
            var counterTarget = GetSpawner(attackResult.CounterAttack.Target);
            counterTarget.CreatureBody.CombatantUIElements.InitCBT(attackResult.CounterAttack.Value.ToString(), GetAttackTrigger(attackResult.CounterAttack), false);
        }

        Events.Add(new CombatEvent());
    }

    private CbtTriggerType GetAttackTrigger(BasicAttackResult attackResult)
    {
        if (attackResult.Hit && attackResult.Critical)
        {
            return CbtTriggerType.CriticalDamage;
        }
        else
        {
            return CbtTriggerType.Damage;
        }
    }

    public CombatInstance StartCombat(Party party, Encounter encounter, int level)
    {
        var combatInstance = CombatInstance.FromEncounter(party, encounter, level);
        combatInstance.CombatManager = this;
        foreach (var partyMember in party.Members)
        {
            partyMember.Combatant.MainWeaponCooldown = partyMember.Equipment.MainHand.Status().AttackSpeed;
            if (partyMember.Equipment.Offhand is WeaponInstance)
            {
                partyMember.Combatant.SecondaryWeaponCooldown = (partyMember.Equipment.Offhand as WeaponInstance).Status().AttackSpeed;
            }
        }      
        foreach (var monster in combatInstance.Encounter.Monsters)
        {
            monster.Combatant.MainWeaponCooldown = monster.Equipment.MainHand.Status().AttackSpeed;
            if (monster.Equipment.Offhand is WeaponInstance)
            {
                monster.Combatant.SecondaryWeaponCooldown = (monster.Equipment.Offhand as WeaponInstance).Status().AttackSpeed;
            }
        }

        if (CurrentParty == party)
        {
            party.CombatInstance = combatInstance;
            InstantiateCombat(party);
        }

        return combatInstance;
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
        PartyManager.OnHeroSelected += OnHeroSelected;
    }

    public void OnHeroSelected(Party party)
    {
        if (CurrentParty == party)
        {
            return;
        }
        InstantiateCombat(party);
    }

    public void InstantiateCombat(Party party)
    {

        CurrentParty = party;
        if (party.InCombat)
        {
            var index = 0;
            foreach (var hero in party.Members)
            {
              //  HeroSpawners[index++].Clear();
                HeroSpawners[index++].InstantiateCreature(hero);
            }         
            index = 0;
            foreach (var monster in party.CombatInstance.Encounter.Monsters)
            {
            //    MonsterSpawners[index++].Clear();
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
                party.CombatInstance.RunCombatTurn();
            }
        }
    }

    public void FinishCombat(CombatInstance combatInstance, bool heroesWon)
    {
        // TODO tela bonita de loot e etc
        if (CurrentParty == combatInstance.Party)
        {
            Clear();
        }

        var party = combatInstance.Party;

        combatInstance.Party.InCombat = false;
        combatInstance.Party.CombatInstance = null;
        if (!heroesWon)
        {
            party.Die();
        }
    }

    private void Clear()
    {
        foreach (var creatureSpawner in HeroSpawners)
        {
            creatureSpawner.CreatureBody = null;
            foreach (Transform creature in creatureSpawner.transform)
            {
                Destroy(creature.gameObject);
            }
        }       
        foreach (var creatureSpawner in MonsterSpawners)
        {
            foreach (Transform creature in creatureSpawner.transform)
            {
                Destroy(creature.gameObject);
            }
        }
    }
}

public class CombatEvent
{
}
