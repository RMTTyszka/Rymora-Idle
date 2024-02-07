namespace Global
{
    public class Creature
    {
        public string Name { get; set; }
        public int Level{ get; set; }
        public Inventory Inventory { get; set; }

        public Creature()
        {
            Inventory = new Inventory();
        }

        public static Creature FromCreature(CreatureTemplate creatureTemplate, int level)
        {
            var creature = new Creature();
            creature.Name = creatureTemplate.creatureName;
            creature.Level = level;
            creature.Inventory = new Inventory();
            return creature;
        }

    }
}