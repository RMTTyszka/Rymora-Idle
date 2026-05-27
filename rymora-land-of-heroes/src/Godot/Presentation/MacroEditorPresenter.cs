using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.GodotAdapter.Presentation;

public partial class MacroEditorPresenter : PanelContainer
{
    private Label? _titleLabel;
    private LineEdit? _nameEdit;
    private Button? _renameButton;
    private ItemList? _actionList;
    private Button? _moveUpButton;
    private Button? _moveDownButton;
    private Button? _removeButton;
    private LineEdit? _moveX;
    private LineEdit? _moveY;
    private Button? _setMoveButton;
    private OptionButton? _repeatMode;
    private LineEdit? _repeatValue;
    private Button? _setRepeatButton;
    private GameApplication? _application;
    private Party? _party;
    private Action? _changed;
    private string? _macroId;
    private readonly List<string> _actionIds = new();

    public override void _Ready()
    {
        _titleLabel = GetNodeOrNull<Label>("Margin/Rows/TitleLabel");
        _nameEdit = GetNodeOrNull<LineEdit>("Margin/Rows/NameEdit");
        _renameButton = GetNodeOrNull<Button>("Margin/Rows/RenameButton");
        _actionList = GetNodeOrNull<ItemList>("Margin/Rows/ActionList");
        _moveUpButton = GetNodeOrNull<Button>("Margin/Rows/ActionButtons/MoveUpButton");
        _moveDownButton = GetNodeOrNull<Button>("Margin/Rows/ActionButtons/MoveDownButton");
        _removeButton = GetNodeOrNull<Button>("Margin/Rows/ActionButtons/RemoveButton");
        _moveX = GetNodeOrNull<LineEdit>("Margin/Rows/MoveFields/MoveX");
        _moveY = GetNodeOrNull<LineEdit>("Margin/Rows/MoveFields/MoveY");
        _setMoveButton = GetNodeOrNull<Button>("Margin/Rows/MoveFields/SetMoveButton");
        _repeatMode = GetNodeOrNull<OptionButton>("Margin/Rows/RepeatFields/RepeatMode");
        _repeatValue = GetNodeOrNull<LineEdit>("Margin/Rows/RepeatFields/RepeatCount");
        _setRepeatButton = GetNodeOrNull<Button>("Margin/Rows/RepeatFields/SetRepeatButton");

        RepeatPolicyUi.Configure(_repeatMode);

        if (_renameButton is not null) _renameButton.Pressed += OnRenamePressed;
        if (_moveUpButton is not null) _moveUpButton.Pressed += () => MoveSelectedAction(-1);
        if (_moveDownButton is not null) _moveDownButton.Pressed += () => MoveSelectedAction(1);
        if (_removeButton is not null) _removeButton.Pressed += OnRemovePressed;
        if (_setMoveButton is not null) _setMoveButton.Pressed += OnSetMovePressed;
        if (_setRepeatButton is not null) _setRepeatButton.Pressed += OnSetRepeatPressed;
        if (_actionList is not null) _actionList.ItemSelected += OnActionSelected;
    }

    public void Bind(GameApplication application, Party party, Action changed)
    {
        _application = application;
        _party = party;
        _changed = changed;
    }

    public void Open(string macroId)
    {
        _macroId = macroId;
        Show();
        Refresh();
    }

    private void Refresh()
    {
        var macro = GetMacro();
        if (macro is null)
        {
            return;
        }

        if (_titleLabel is not null) _titleLabel.Text = $"Macro Editor: {macro.Name}";
        if (_nameEdit is not null) _nameEdit.Text = macro.Name;

        var selectedAction = GetSelectedActionId();
        _actionIds.Clear();
        _actionList?.Clear();
        foreach (var action in macro.Actions)
        {
            _actionIds.Add(action.Id);
            _actionList?.AddItem(FormatAction(action));
        }

        if (_actionList is not null && _actionIds.Count > 0)
        {
            var index = selectedAction is null ? 0 : _actionIds.IndexOf(selectedAction);
            _actionList.Select(index < 0 ? 0 : index);
            FillSelectedActionFields();
        }
    }

    private void OnRenamePressed()
    {
        if (_application is null || _party is null || _macroId is null || _nameEdit is null)
        {
            return;
        }

        try
        {
            _application.RenameMacro(_party.Id, _macroId, _nameEdit.Text);
            _changed?.Invoke();
            Refresh();
        }
        catch (Exception ex)
        {
            GD.Print($"Rename Macro failed: {ex.Message}");
        }
    }

    private void MoveSelectedAction(int offset)
    {
        if (_application is null || _party is null || _macroId is null)
        {
            return;
        }

        var index = GetSelectedActionIndex();
        var newIndex = index + offset;
        if (index < 0 || newIndex < 0 || newIndex >= _actionIds.Count)
        {
            return;
        }

        _application.MoveMacroAction(_party.Id, _macroId, _actionIds[index], newIndex);
        _changed?.Invoke();
        Refresh();
    }

    private void OnRemovePressed()
    {
        if (_application is null || _party is null || _macroId is null)
        {
            return;
        }

        var actionId = GetSelectedActionId();
        if (actionId is null)
        {
            return;
        }

        _application.RemoveMacroAction(_party.Id, _macroId, actionId);
        _changed?.Invoke();
        Refresh();
    }

    private void OnSetMovePressed()
    {
        if (_application is null || _party is null || _macroId is null || _moveX is null || _moveY is null)
        {
            return;
        }

        var actionId = GetSelectedActionId();
        if (actionId is null || !int.TryParse(_moveX.Text, out var x) || !int.TryParse(_moveY.Text, out var y))
        {
            GD.Print("MoveTo destination requires whole-number X and Y.");
            return;
        }

        try
        {
            _application.SetMoveActionDestination(_party.Id, _macroId, actionId, new TilePosition(x, y));
            _changed?.Invoke();
            Refresh();
        }
        catch (Exception ex)
        {
            GD.Print($"Set MoveTo destination failed: {ex.Message}");
        }
    }

    private void OnSetRepeatPressed()
    {
        if (_application is null || _party is null || _macroId is null)
        {
            return;
        }

        var actionId = GetSelectedActionId();
        if (actionId is null || !RepeatPolicyUi.TryRead(_repeatMode, _repeatValue, out var repeat))
        {
            return;
        }

        try
        {
            _application.SetGatherActionRepeat(_party.Id, _macroId, actionId, repeat);
            _changed?.Invoke();
            Refresh();
        }
        catch (Exception ex)
        {
            GD.Print($"Set action repeat failed: {ex.Message}");
        }
    }

    private void OnActionSelected(long index)
    {
        FillSelectedActionFields();
    }

    private void FillSelectedActionFields()
    {
        var macro = GetMacro();
        var actionId = GetSelectedActionId();
        var action = macro?.Actions.FirstOrDefault(action => action.Id == actionId);
        if (action is MoveToMacroAction move)
        {
            if (_moveX is not null) _moveX.Text = move.Destination.X.ToString();
            if (_moveY is not null) _moveY.Text = move.Destination.Y.ToString();
        }

        if (action is GatherMacroAction gather)
        {
            RepeatPolicyUi.Write(_repeatMode, _repeatValue, gather.Repeat);
        }
    }

    private PartyMacro? GetMacro()
    {
        return _party is null || _macroId is null ? null : _party.Automation.TryGetMacro(_macroId);
    }

    private string? GetSelectedActionId()
    {
        var index = GetSelectedActionIndex();
        return index >= 0 && index < _actionIds.Count ? _actionIds[index] : null;
    }

    private int GetSelectedActionIndex()
    {
        var selected = _actionList?.GetSelectedItems();
        return selected is { Length: > 0 } ? selected[0] : -1;
    }

    private static string FormatAction(MacroAction action)
    {
        return action switch
        {
            MoveToMacroAction move => $"{action.Id}: MoveTo ({move.Destination.X}, {move.Destination.Y})",
            GatherMacroAction gather => $"{action.Id}: {gather.Kind} repeat {RepeatPolicyUi.Format(gather.Repeat)}",
            _ => $"{action.Id}: {action.Kind}"
        };
    }
}
