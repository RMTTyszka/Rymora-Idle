using UnityEngine;

public class Creature
{
    public string Name { get; set; }
    public int Level{ get; set; }
    public Inventory Inventory { get; set; } = new();
    public Equipment Equipment { get; set; } = new();

    public Skills Skills { get; set; } = new();
    public Attributes Attributes { get; set; } = new();
    public Properties Properties { get; set; } = new();
    public Combatant Combatant { get; set; }


    public Sprite Sprite { get; set; }
    public Creature()
    {
        Combatant = new Combatant
        {
            Creature = this
        };
    }
    
    public static Creature FromCreature(CreatureTemplate creatureTemplate, int level)
    {
        var creature = new Creature();
        creature.Name = creatureTemplate.creatureName;
        creature.Level = level;
        creature.Inventory = new Inventory();
        creature.Sprite = creatureTemplate.sprite;
        return creature;
    }

}

public enum Slot
{ 
    None = 0, 
    MainHand = 1, 
    Offhand = 2, 
    Head = 3, 
    Neck = 4, 
    Chest = 5,
    Wrist = 6, 
    Hand = 7, 
    FingerLeft = 8, 
    FingerRight = 9, 
    Waist = 10, 
    Feet = 11, 
    Extra = 12
};

