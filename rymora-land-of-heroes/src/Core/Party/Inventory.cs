namespace RymoraLandOfHeroes.Core.Party;

public sealed class Inventory
{
    private readonly Dictionary<ItemKey, Item> _items = new();

    public IReadOnlyList<Item> Items => _items.Values.ToArray();

    public float TotalWeight => _items.Values.Sum(item => item.Weight * item.Quantity);

    public void AddItem(Item item)
    {
        ValidateItem(item);

        var key = new ItemKey(item.Name, item.Level);
        if (!_items.TryGetValue(key, out var existing))
        {
            _items[key] = item;
            return;
        }

        if (!NearlyEqual(existing.Weight, item.Weight))
        {
            throw new InvalidOperationException("Grouped items with same name and level must have same weight.");
        }

        _items[key] = existing.WithQuantity(existing.Quantity + item.Quantity);
    }

    public bool RemoveItem(string name, int level, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        var key = new ItemKey(name, level);
        if (!_items.TryGetValue(key, out var existing) || existing.Quantity < quantity)
        {
            return false;
        }

        var remaining = existing.Quantity - quantity;
        if (remaining == 0)
        {
            _items.Remove(key);
            return true;
        }

        _items[key] = existing.WithQuantity(remaining);
        return true;
    }

    public int GetQuantity(string name, int level)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _items.TryGetValue(new ItemKey(name, level), out var item) ? item.Quantity : 0;
    }

    public Item? GetItem(string name, int level)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _items.TryGetValue(new ItemKey(name, level), out var item) ? item : null;
    }

    private static void ValidateItem(Item item)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(item.Name);
        if (item.Quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(item), "Item quantity must be positive.");
        }
    }

    private static bool NearlyEqual(float left, float right) => Math.Abs(left - right) < 0.0001f;

    private readonly record struct ItemKey(string Name, int Level);
}
