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

        public static Creature FromCreature(Creature creatureTemplate)
        {
            var creature = new Creature();
            creature.Name = creature.Name;
            creature.Level = creature.Level;
            creature.Inventory = creature.Inventory;
            return creature;
        }

    }
}