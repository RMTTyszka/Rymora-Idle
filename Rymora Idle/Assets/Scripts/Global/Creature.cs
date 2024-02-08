using Heroes;
using UnityEngine;

namespace Global
{
    public class Creature
    {
        public string Name { get; set; }
        public int Level{ get; set; }
        public Inventory Inventory { get; set; }

        public Skills Skills { get; set; } = new();
        public Attributes Attributes { get; set; } = new();


        public Sprite Sprite { get; set; }
        public Creature()
        {
            Inventory = new Inventory();
            Skills = new Skills();
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
}