namespace Items.Equipables.Armors;

public class ArmorInstance : Equipable
{
    public ArmorCategory Category { get; set; }
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