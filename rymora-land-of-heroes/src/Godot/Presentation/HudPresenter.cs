using System.Linq;
using Godot;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.GodotAdapter.Presentation;

public partial class HudPresenter : PanelContainer
{
    private Label? _label;

    public override void _Ready()
    {
        _label = GetNode<Label>("Margin/Label");
    }

    public void Sync(GameApplication application, Party party)
    {
        if (_label is null)
        {
            return;
        }

        _label.Text = string.Join(
            "\n",
            $"Party: {party.Id}",
            $"Screen: {application.UI.CurrentScreen}",
            $"Position: ({party.Position.X}, {party.Position.Y})",
            $"Action: {FormatAction(party.ActionQueue.Current)}",
            $"Queue: {party.ActionQueue.PendingCount} pending",
            $"Inventory: {FormatInventory(party.Inventory)}");
    }

    private static string FormatAction(PartyActionState? action)
    {
        if (action is null)
        {
            return "Idle";
        }

        var progress = action.Request.TimeToExecute <= 0
            ? 1
            : Mathf.Clamp(action.CurrentTime / action.Request.TimeToExecute, 0, 1);

        return $"{action.Request.ActionType} {progress:P0} ({action.ExecutedCount}/{action.Request.LimitCount ?? 1})";
    }

    private static string FormatInventory(Inventory inventory)
    {
        if (inventory.Items.Count == 0)
        {
            return "empty";
        }

        return string.Join(", ", inventory.Items.Select(item => $"{item.Name} Lv{item.Level} x{item.Quantity}"));
    }
}
