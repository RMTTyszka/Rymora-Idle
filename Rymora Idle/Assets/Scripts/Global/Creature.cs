using UnityEngine;

namespace Global
{
    public class Creature
    {
        public string Name { get; set; }
        public int Level{ get; set; }
        public Inventory Inventory { get; set; }
        public Skills Skills { get; set; }
        public Attributes Attributes { get; set; }
        
        
        public Sprite Image { get; set; }


        public Creature()
        {
            Inventory = new Inventory();
            Skills = new Skills();
            Attributes = new Attributes();
        }

        public static Creature FromCreature(Creature creatureTemplate, int level)
        {
            var creature = new Creature();
            creature.Name = creature.Name;
            creature.Level = level;
            creature.Inventory = creature.Inventory;
            return creature;
        }

    }

    public class Attributes
    {
        public Attribute Strength { get; set; }
        public Attribute Agility { get; set; }
        public Attribute Vitality { get; set; }
        public Attribute Wisdom { get; set; }
        public Attribute Intuition { get; set; }
        public Attribute Charisma { get; set; }

        public Attributes()
        {
            Strength = new Attribute();
            Agility = new Attribute();
            Vitality = new Attribute();
            Wisdom = new Attribute();
            Intuition = new Attribute();
            Charisma = new Attribute();
        }
    }  
      public class Skills
    {
        public Skill Mining { get; set; }
    }
}