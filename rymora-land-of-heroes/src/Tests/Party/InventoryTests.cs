using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Party;

public sealed class InventoryTests
{
    [Fact]
    public void AddItem_groups_items_by_name_and_level()
    {
        var scenario = InventoryObjectMother.AddItemWithSameNameAndLevel();

        scenario.InputInventory.AddItem(scenario.InputItem);

        Assert.Equal(scenario.ExpectedQuantity, scenario.InputInventory.GetQuantity(scenario.AssertItemName, scenario.AssertItemLevel));
    }

    [Fact]
    public void AddItem_keeps_different_levels_separate()
    {
        var scenario = InventoryObjectMother.AddItemWithDifferentLevel();

        scenario.InputInventory.AddItem(scenario.InputItem);

        Assert.Equal(scenario.ExpectedQuantity, scenario.InputInventory.GetQuantity(scenario.AssertItemName, scenario.AssertItemLevel));
    }

    [Fact]
    public void TotalWeight_uses_item_weight_times_quantity()
    {
        var scenario = InventoryObjectMother.InventoryWithWeightedItems();

        var totalWeight = scenario.InputInventory.TotalWeight;

        Assert.Equal(scenario.ExpectedTotalWeight, totalWeight);
    }

    [Fact]
    public void RemoveItem_removes_requested_quantity()
    {
        var scenario = InventoryObjectMother.InventoryWithEnoughItemsToRemove();

        scenario.InputInventory.RemoveItem(scenario.InputItemName, scenario.InputItemLevel, scenario.InputQuantity);

        Assert.Equal(scenario.ExpectedRemainingQuantity, scenario.InputInventory.GetQuantity(scenario.InputItemName, scenario.InputItemLevel));
    }

    [Fact]
    public void RemoveItem_returns_false_when_quantity_is_insufficient()
    {
        var scenario = InventoryObjectMother.InventoryWithoutEnoughItemsToRemove();

        var result = scenario.InputInventory.RemoveItem(scenario.InputItemName, scenario.InputItemLevel, scenario.InputQuantity);

        Assert.Equal(scenario.ExpectedResult, result);
    }
}
