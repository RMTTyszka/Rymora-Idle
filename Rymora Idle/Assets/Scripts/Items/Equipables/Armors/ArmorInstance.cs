public class ArmorInstance : Equipable
{
    public ArmorCategory Category { get; set; }

    public static ArmorInstance FromTemplate(ArmorTemplate armorTemplate, int level)
    {
        var armor = new ArmorInstance
        {
            Category = armorTemplate.category,
            Level = level,
            Name = armorTemplate.name,
            Slot = Slot.Chest,
            Weight = 1
        };
        return armor;
    }
}

public enum ArmorCategory
{
    Light = 0,
    Medium = 1,
    Heavy = 2
}

public class ArmorStatus
{
    public int Protection { get; set; }
    public int Evasion { get; set; }
}