using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.GodotAdapter.Presentation;

public partial class ProgramEditorPresenter : Window
{
    private Label? _titleLabel;
    private ItemList? _stepList;
    private Button? _moveUpButton;
    private Button? _moveDownButton;
    private Button? _removeButton;
    private LineEdit? _stepRepeat;
    private Button? _setStepRepeatButton;
    private LineEdit? _programRepeat;
    private Button? _setProgramRepeatButton;
    private GameApplication? _application;
    private Party? _party;
    private Action? _changed;
    private readonly List<string> _stepIds = new();

    public override void _Ready()
    {
        _titleLabel = GetNodeOrNull<Label>("Margin/Rows/TitleLabel");
        _stepList = GetNodeOrNull<ItemList>("Margin/Rows/StepList");
        _moveUpButton = GetNodeOrNull<Button>("Margin/Rows/StepButtons/MoveUpButton");
        _moveDownButton = GetNodeOrNull<Button>("Margin/Rows/StepButtons/MoveDownButton");
        _removeButton = GetNodeOrNull<Button>("Margin/Rows/StepButtons/RemoveButton");
        _stepRepeat = GetNodeOrNull<LineEdit>("Margin/Rows/StepRepeatFields/StepRepeat");
        _setStepRepeatButton = GetNodeOrNull<Button>("Margin/Rows/StepRepeatFields/SetStepRepeatButton");
        _programRepeat = GetNodeOrNull<LineEdit>("Margin/Rows/ProgramRepeatFields/ProgramRepeat");
        _setProgramRepeatButton = GetNodeOrNull<Button>("Margin/Rows/ProgramRepeatFields/SetProgramRepeatButton");

        CloseRequested += Hide;
        if (_moveUpButton is not null) _moveUpButton.Pressed += () => MoveSelectedStep(-1);
        if (_moveDownButton is not null) _moveDownButton.Pressed += () => MoveSelectedStep(1);
        if (_removeButton is not null) _removeButton.Pressed += OnRemovePressed;
        if (_setStepRepeatButton is not null) _setStepRepeatButton.Pressed += OnSetStepRepeatPressed;
        if (_setProgramRepeatButton is not null) _setProgramRepeatButton.Pressed += OnSetProgramRepeatPressed;
        if (_stepList is not null) _stepList.ItemSelected += OnStepSelected;
    }

    public void Bind(GameApplication application, Party party, Action changed)
    {
        _application = application;
        _party = party;
        _changed = changed;
    }

    public void Open()
    {
        Refresh();
        PopupCentered();
    }

    private void Refresh()
    {
        if (_party is null)
        {
            return;
        }

        if (_titleLabel is not null)
        {
            _titleLabel.Text = $"Program Editor: repeat {FormatRepeat(_party.Automation.Program.Repeat)}";
        }

        if (_programRepeat is not null)
        {
            _programRepeat.Text = (_party.Automation.Program.Repeat.RepeatCount ?? 1).ToString();
        }

        var selectedStep = GetSelectedStepId();
        _stepIds.Clear();
        _stepList?.Clear();
        foreach (var step in _party.Automation.Program.Steps)
        {
            var macro = _party.Automation.TryGetMacro(step.MacroId);
            _stepIds.Add(step.Id);
            _stepList?.AddItem($"{step.Id}: {macro?.Name ?? step.MacroId} repeat {FormatRepeat(step.Repeat)}");
        }

        if (_stepList is not null && _stepIds.Count > 0)
        {
            var index = selectedStep is null ? 0 : _stepIds.IndexOf(selectedStep);
            _stepList.Select(index < 0 ? 0 : index);
            FillSelectedStepFields();
        }
    }

    private void MoveSelectedStep(int offset)
    {
        if (_application is null || _party is null)
        {
            return;
        }

        var index = GetSelectedStepIndex();
        var newIndex = index + offset;
        if (index < 0 || newIndex < 0 || newIndex >= _stepIds.Count)
        {
            return;
        }

        _application.MoveProgramStep(_party.Id, _stepIds[index], newIndex);
        _changed?.Invoke();
        Refresh();
    }

    private void OnRemovePressed()
    {
        if (_application is null || _party is null)
        {
            return;
        }

        var stepId = GetSelectedStepId();
        if (stepId is null)
        {
            return;
        }

        _application.RemoveProgramStep(_party.Id, stepId);
        _changed?.Invoke();
        Refresh();
    }

    private void OnSetStepRepeatPressed()
    {
        if (_application is null || _party is null || _stepRepeat is null)
        {
            return;
        }

        var stepId = GetSelectedStepId();
        if (stepId is null || !TryBuildCountRepeat(_stepRepeat.Text, out var repeat))
        {
            return;
        }

        _application.SetProgramStepRepeat(_party.Id, stepId, repeat);
        _changed?.Invoke();
        Refresh();
    }

    private void OnSetProgramRepeatPressed()
    {
        if (_application is null || _party is null || _programRepeat is null || !TryBuildCountRepeat(_programRepeat.Text, out var repeat))
        {
            return;
        }

        _application.SetProgramRepeat(_party.Id, repeat);
        _changed?.Invoke();
        Refresh();
    }

    private void OnStepSelected(long index)
    {
        FillSelectedStepFields();
    }

    private void FillSelectedStepFields()
    {
        if (_party is null || _stepRepeat is null)
        {
            return;
        }

        var stepId = GetSelectedStepId();
        var step = _party.Automation.Program.Steps.FirstOrDefault(step => step.Id == stepId);
        if (step is not null)
        {
            _stepRepeat.Text = (step.Repeat.RepeatCount ?? 1).ToString();
        }
    }

    private string? GetSelectedStepId()
    {
        var index = GetSelectedStepIndex();
        return index >= 0 && index < _stepIds.Count ? _stepIds[index] : null;
    }

    private int GetSelectedStepIndex()
    {
        var selected = _stepList?.GetSelectedItems();
        return selected is { Length: > 0 } ? selected[0] : -1;
    }

    private static bool TryBuildCountRepeat(string text, out RepeatPolicy repeat)
    {
        repeat = RepeatPolicy.Once;
        if (!int.TryParse(text, out var count) || count <= 0)
        {
            GD.Print("Repeat must be a positive whole number.");
            return false;
        }

        repeat = RepeatPolicy.Count(count);
        return true;
    }

    private static string FormatRepeat(RepeatPolicy repeat)
    {
        return repeat.Mode == RepeatMode.Count ? (repeat.RepeatCount?.ToString() ?? "count") : repeat.Mode.ToString();
    }
}
