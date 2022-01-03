public abstract class Item
{
    public string Name { get; set; }
    public int Level { get; set; }
    public decimal Weight { get; set; }

    public int Difficulty => Level * 10 + 50;
}
