using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.Core.Tests.ObjectMothers;

internal static class InventoryObjectMother
{
    public static AddItemGroupingScenario AddItemWithSameNameAndLevel()
    {
        var inventory = new Inventory();
        inventory.AddItem(new Item("Iron", 1, 3, 2));

        return new AddItemGroupingScenario(
            InputInventory: inventory,
            InputItem: new Item("Iron", 1, 3, 4),
            AssertItemName: "Iron",
            AssertItemLevel: 1,
            ExpectedQuantity: 6);
    }

    public static AddDifferentLevelScenario AddItemWithDifferentLevel()
    {
        var inventory = new Inventory();
        inventory.AddItem(new Item("Iron", 1, 3, 2));

        return new AddDifferentLevelScenario(
            InputInventory: inventory,
            InputItem: new Item("Iron", 2, 4, 1),
            AssertItemName: "Iron",
            AssertItemLevel: 2,
            ExpectedQuantity: 1);
    }

    public static TotalWeightScenario InventoryWithWeightedItems()
    {
        var inventory = new Inventory();
        inventory.AddItem(new Item("Iron", 1, 3, 6));
        inventory.AddItem(new Item("Iron", 2, 4, 1));

        return new TotalWeightScenario(
            InputInventory: inventory,
            ExpectedTotalWeight: 22f);
    }

    public static RemoveItemScenario InventoryWithEnoughItemsToRemove()
    {
        var inventory = new Inventory();
        inventory.AddItem(new Item("Iron", 1, 3, 6));

        return new RemoveItemScenario(
            InputInventory: inventory,
            InputItemName: "Iron",
            InputItemLevel: 1,
            InputQuantity: 5,
            ExpectedRemainingQuantity: 1);
    }

    public static RemoveItemFailureScenario InventoryWithoutEnoughItemsToRemove()
    {
        var inventory = new Inventory();
        inventory.AddItem(new Item("Iron", 1, 3, 1));

        return new RemoveItemFailureScenario(
            InputInventory: inventory,
            InputItemName: "Iron",
            InputItemLevel: 1,
            InputQuantity: 2,
            ExpectedResult: false);
    }
}

internal sealed record AddItemGroupingScenario(
    Inventory InputInventory,
    Item InputItem,
    string AssertItemName,
    int AssertItemLevel,
    int ExpectedQuantity);

internal sealed record AddDifferentLevelScenario(
    Inventory InputInventory,
    Item InputItem,
    string AssertItemName,
    int AssertItemLevel,
    int ExpectedQuantity);

internal sealed record TotalWeightScenario(Inventory InputInventory, float ExpectedTotalWeight);

internal sealed record RemoveItemScenario(
    Inventory InputInventory,
    string InputItemName,
    int InputItemLevel,
    int InputQuantity,
    int ExpectedRemainingQuantity);

internal sealed record RemoveItemFailureScenario(
    Inventory InputInventory,
    string InputItemName,
    int InputItemLevel,
    int InputQuantity,
    bool ExpectedResult);
