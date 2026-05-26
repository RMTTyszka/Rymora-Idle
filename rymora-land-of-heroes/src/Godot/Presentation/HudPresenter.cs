using System.Linq;
using Godot;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.GodotAdapter.Presentation;

public partial class HudPresenter : PanelContainer
{
    private Label? _label;
    private Button? _playButton;
    private Button? _pauseButton;
    private Button? _stopButton;
    private GameApplication? _application;
    private Party? _party;

    public override void _Ready()
    {
        _label = GetNodeOrNull<Label>("Margin/Tabs/Status/Label");
        _playButton = GetNodeOrNull<Button>("Margin/Tabs/Status/PlayButton");
        _pauseButton = GetNodeOrNull<Button>("Margin/Tabs/Status/PauseButton");
        _stopButton = GetNodeOrNull<Button>("Margin/Tabs/Status/StopButton");

        if (_playButton is not null)
        {
            _playButton.Pressed += OnPlayPressed;
        }

        if (_pauseButton is not null)
        {
            _pauseButton.Pressed += OnPausePressed;
        }

        if (_stopButton is not null)
        {
            _stopButton.Pressed += OnStopPressed;
        }
    }

    public void Sync(GameApplication application, Party party)
    {
        _application = application;
        _party = party;

        if (_label is null)
        {
            return;
        }

        var runner = party.Automation.Runner;
        _label.Text = string.Join(
            "\n",
            $"Party: {party.Id}",
            $"Screen: {application.UI.CurrentScreen}",
            $"Program: {runner.State}",
            $"Program Error: {runner.ErrorMessage}",
            $"Position: ({party.Position.X}, {party.Position.Y})",
            $"Action: {FormatAction(party.ActionQueue.Current)}",
            $"Next Macro Action: {FormatMacroAction(runner.NextAction)}",
            $"Queue: {party.ActionQueue.PendingCount} pending",
            $"Inventory: {FormatInventory(party.Inventory)}");
    }

    private void OnPlayPressed()
    {
        if (_application is not null && _party is not null)
        {
            _application.PlayProgram(_party.Id);
        }
    }

    private void OnPausePressed()
    {
        if (_application is not null && _party is not null)
        {
            _application.PauseProgram(_party.Id);
        }
    }

    private void OnStopPressed()
    {
        if (_application is not null && _party is not null)
        {
            _application.StopProgram(_party.Id);
        }
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

    private static string FormatMacroAction(MacroAction? action)
    {
        return action switch
        {
            null => "none",
            MoveToMacroAction move => $"MoveTo ({move.Destination.X}, {move.Destination.Y})",
            GatherMacroAction gather => gather.Kind.ToString(),
            _ => action.Kind.ToString()
        };
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
