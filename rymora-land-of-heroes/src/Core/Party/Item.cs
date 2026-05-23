namespace RymoraLandOfHeroes.Core.Party;

public sealed record Item(string Name, int Level, float Weight, int Quantity)
{
    public Item WithQuantity(int quantity) => this with { Quantity = quantity };
}
