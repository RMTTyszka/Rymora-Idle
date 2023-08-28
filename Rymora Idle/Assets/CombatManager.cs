using System.Collections.Generic;
using System.Linq;
using Global;
using Heroes;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public Party Party { get; set; }
    public List<Creature> Monsters { get; set; }

    public CreatureSpawner monsterSpawner1;
    public CreatureSpawner monsterSpawner2;
    public CreatureSpawner monsterSpawner3;
    public CreatureSpawner monsterSpawner4;
    
    
    public CreatureSpawner heroSpawner1;
    public CreatureSpawner heroSpawner2;
    public CreatureSpawner heroSpawner3;
    public CreatureSpawner animalSpawner;
    // Start is called before the first frame update
    void Start()
    {
        SetHero(heroSpawner1,0);
        SetHero(heroSpawner2,1);
        SetHero(heroSpawner3,2);
        SetMonster(monsterSpawner1, 0);
        SetMonster(monsterSpawner2, 1);
        SetMonster(monsterSpawner3, 2);
        SetMonster(monsterSpawner4, 3);
        SetMonster(animalSpawner, 0);
    }

    private void SetHero(CreatureSpawner creatureSpawner, int index)
    {
        var hero = Party.Members.ElementAtOrDefault(index);
        if (hero != null)
        {
            creatureSpawner.Creature = hero;
        }

    }    
    private void SetMonster(CreatureSpawner creatureSpawner, int index)
    {
        var monster = Monsters.ElementAtOrDefault(index);
        if (monster != null)
        {
            creatureSpawner.Creature = monster;
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
        
    }
}
