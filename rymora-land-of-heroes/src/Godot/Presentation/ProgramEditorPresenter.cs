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
    private OptionButton? _stepRepeatMode;
    private LineEdit? _stepRepeatValue;
    private Button? _setStepRepeatButton;
    private OptionButton? _programRepeatMode;
    private LineEdit? _programRepeatValue;
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
        _stepRepeatMode = GetNodeOrNull<OptionButton>("Margin/Rows/StepRepeatFields/StepRepeatMode");
        _stepRepeatValue = GetNodeOrNull<LineEdit>("Margin/Rows/StepRepeatFields/StepRepeat");
        _setStepRepeatButton = GetNodeOrNull<Button>("Margin/Rows/StepRepeatFields/SetStepRepeatButton");
        _programRepeatMode = GetNodeOrNull<OptionButton>("Margin/Rows/ProgramRepeatFields/ProgramRepeatMode");
        _programRepeatValue = GetNodeOrNull<LineEdit>("Margin/Rows/ProgramRepeatFields/ProgramRepeat");
        _setProgramRepeatButton = GetNodeOrNull<Button>("Margin/Rows/ProgramRepeatFields/SetProgramRepeatButton");

        RepeatPolicyUi.Configure(_stepRepeatMode);
        RepeatPolicyUi.Configure(_programRepeatMode);

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
            _titleLabel.Text = $"Program Editor: repeat {RepeatPolicyUi.Format(_party.Automation.Program.Repeat)}";
        }

        RepeatPolicyUi.Write(_programRepeatMode, _programRepeatValue, _party.Automation.Program.Repeat);

        var selectedStep = GetSelectedStepId();
        _stepIds.Clear();
        _stepList?.Clear();
        foreach (var step in _party.Automation.Program.Steps)
        {
            var macro = _party.Automation.TryGetMacro(step.MacroId);
            _stepIds.Add(step.Id);
            _stepList?.AddItem($"{step.Id}: {macro?.Name ?? step.MacroId} repeat {RepeatPolicyUi.Format(step.Repeat)}");
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
        if (_application is null || _party is null)
        {
            return;
        }

        var stepId = GetSelectedStepId();
        if (stepId is null || !RepeatPolicyUi.TryRead(_stepRepeatMode, _stepRepeatValue, out var repeat))
        {
            return;
        }

        _application.SetProgramStepRepeat(_party.Id, stepId, repeat);
        _changed?.Invoke();
        Refresh();
    }

    private void OnSetProgramRepeatPressed()
    {
        if (_application is null || _party is null || !RepeatPolicyUi.TryRead(_programRepeatMode, _programRepeatValue, out var repeat))
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
        if (_party is null)
        {
            return;
        }

        var stepId = GetSelectedStepId();
        var step = _party.Automation.Program.Steps.FirstOrDefault(step => step.Id == stepId);
        if (step is not null)
        {
            RepeatPolicyUi.Write(_stepRepeatMode, _stepRepeatValue, step.Repeat);
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

}
