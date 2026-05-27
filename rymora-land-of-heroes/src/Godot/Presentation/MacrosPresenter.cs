using System;
using System.Collections.Generic;
using Godot;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.GodotAdapter.Presentation;

public partial class MacrosPresenter : Control
{
    private LineEdit? _macroName;
    private Button? _recordButton;
    private Button? _saveButton;
    private Button? _cancelButton;
    private ItemList? _macroList;
    private Button? _addToProgramButton;
    private Button? _editMacroButton;
    private Button? _editProgramButton;
    private ItemList? _programList;
    private Button? _moveStepUpButton;
    private Button? _moveStepDownButton;
    private Button? _removeStepButton;
    private OptionButton? _stepRepeatMode;
    private LineEdit? _stepRepeatValue;
    private Button? _setStepRepeatButton;
    private Label? _activeProgramLabel;
    private MacroEditorPresenter? _macroEditor;
    private ProgramEditorPresenter? _programEditor;
    private GameApplication? _application;
    private Party? _party;
    private readonly List<string> _macroIds = new();
    private readonly List<string> _stepIds = new();

    public override void _Ready()
    {
        _macroName = GetNodeOrNull<LineEdit>("MacroName");
        _recordButton = GetNodeOrNull<Button>("RecordButton");
        _saveButton = GetNodeOrNull<Button>("SaveButton");
        _cancelButton = GetNodeOrNull<Button>("CancelButton");
        _macroList = GetNodeOrNull<ItemList>("MacroList");
        _addToProgramButton = GetNodeOrNull<Button>("AddToProgramButton");
        _editMacroButton = GetNodeOrNull<Button>("EditMacroButton");
        _editProgramButton = GetNodeOrNull<Button>("EditProgramButton");
        _programList = GetNodeOrNull<ItemList>("ProgramList");
        _moveStepUpButton = GetNodeOrNull<Button>("MoveStepUpButton");
        _moveStepDownButton = GetNodeOrNull<Button>("MoveStepDownButton");
        _removeStepButton = GetNodeOrNull<Button>("RemoveStepButton");
        _stepRepeatMode = GetNodeOrNull<OptionButton>("StepRepeatMode");
        _stepRepeatValue = GetNodeOrNull<LineEdit>("StepRepeat");
        _setStepRepeatButton = GetNodeOrNull<Button>("SetStepRepeatButton");
        _activeProgramLabel = GetNodeOrNull<Label>("ActiveProgramLabel");
        _macroEditor = GetNodeOrNull<MacroEditorPresenter>("../../../../MacroEditor");
        _programEditor = GetNodeOrNull<ProgramEditorPresenter>("../../../../ProgramEditor");

        RepeatPolicyUi.Configure(_stepRepeatMode);

        if (_recordButton is not null) _recordButton.Pressed += OnRecordPressed;
        if (_saveButton is not null) _saveButton.Pressed += OnSavePressed;
        if (_cancelButton is not null) _cancelButton.Pressed += OnCancelPressed;
        if (_addToProgramButton is not null) _addToProgramButton.Pressed += OnAddToProgramPressed;
        if (_editMacroButton is not null) _editMacroButton.Pressed += OnEditMacroPressed;
        if (_editProgramButton is not null) _editProgramButton.Pressed += OnEditProgramPressed;
        if (_moveStepUpButton is not null) _moveStepUpButton.Pressed += () => MoveSelectedStep(-1);
        if (_moveStepDownButton is not null) _moveStepDownButton.Pressed += () => MoveSelectedStep(1);
        if (_removeStepButton is not null) _removeStepButton.Pressed += OnRemoveStepPressed;
        if (_setStepRepeatButton is not null) _setStepRepeatButton.Pressed += OnSetStepRepeatPressed;
    }

    public void Bind(GameApplication application, Party party)
    {
        _application = application;
        _party = party;
        _macroEditor?.Bind(application, party, Refresh);
        _programEditor?.Bind(application, party, Refresh);
        Refresh();
    }

    public void Refresh()
    {
        if (_party is null)
        {
            return;
        }

        var selectedMacro = GetSelectedMacroId();
        var selectedStep = GetSelectedStepId();
        RefreshMacros(selectedMacro);
        RefreshProgram(selectedStep);

        if (_activeProgramLabel is not null)
        {
            var runner = _party.Automation.Runner;
            _activeProgramLabel.Text = $"Active Program: {runner.State} | Next: {FormatAction(runner.NextAction)}";
        }
    }

    private void RefreshMacros(string? selectedMacro)
    {
        if (_party is null || _macroList is null)
        {
            return;
        }

        _macroIds.Clear();
        _macroList.Clear();
        foreach (var macro in _party.Automation.Macros)
        {
            _macroIds.Add(macro.Id);
            _macroList.AddItem($"{macro.Name} ({macro.Actions.Count} actions)");
        }

        SelectById(_macroList, _macroIds, selectedMacro);
    }

    private void RefreshProgram(string? selectedStep)
    {
        if (_party is null || _programList is null)
        {
            return;
        }

        _stepIds.Clear();
        _programList.Clear();
        foreach (var step in _party.Automation.Program.Steps)
        {
            var macro = _party.Automation.TryGetMacro(step.MacroId);
            _stepIds.Add(step.Id);
            _programList.AddItem($"{macro?.Name ?? step.MacroId} | repeat {RepeatPolicyUi.Format(step.Repeat)}");
        }

        SelectById(_programList, _stepIds, selectedStep);
    }

    private void OnRecordPressed()
    {
        if (_application is null || _party is null)
        {
            return;
        }

        try
        {
            _application.StartRecordingMacro(_party.Id);
            Refresh();
        }
        catch (Exception ex)
        {
            GD.Print($"Record Macro failed: {ex.Message}");
        }
    }

    private void OnSavePressed()
    {
        if (_application is null || _party is null || _macroName is null)
        {
            return;
        }

        try
        {
            _application.SaveRecordingMacro(_party.Id, _macroName.Text);
            _macroName.Text = string.Empty;
            Refresh();
        }
        catch (Exception ex)
        {
            GD.Print($"Save Macro failed: {ex.Message}");
        }
    }

    private void OnCancelPressed()
    {
        if (_application is null || _party is null)
        {
            return;
        }

        _application.CancelRecordingMacro(_party.Id);
        Refresh();
    }

    private void OnAddToProgramPressed()
    {
        if (_application is null || _party is null)
        {
            return;
        }

        var macroId = GetSelectedMacroId();
        if (macroId is null)
        {
            return;
        }

        try
        {
            _application.AddMacroToProgram(_party.Id, macroId, RepeatPolicy.Once);
            Refresh();
        }
        catch (Exception ex)
        {
            GD.Print($"Add Macro to Program failed: {ex.Message}");
        }
    }

    private void OnEditMacroPressed()
    {
        var macroId = GetSelectedMacroId();
        if (macroId is not null)
        {
            _macroEditor?.Open(macroId);
        }
    }

    private void OnEditProgramPressed()
    {
        _programEditor?.Open();
    }

    private void MoveSelectedStep(int offset)
    {
        if (_application is null || _party is null)
        {
            return;
        }

        var selected = GetSelectedProgramIndex();
        if (selected < 0)
        {
            return;
        }

        var newIndex = selected + offset;
        if (newIndex < 0 || newIndex >= _stepIds.Count)
        {
            return;
        }

        _application.MoveProgramStep(_party.Id, _stepIds[selected], newIndex);
        Refresh();
    }

    private void OnRemoveStepPressed()
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
        Refresh();
    }

    private string? GetSelectedMacroId()
    {
        var index = GetSelectedIndex(_macroList);
        return index >= 0 && index < _macroIds.Count ? _macroIds[index] : null;
    }

    private string? GetSelectedStepId()
    {
        var index = GetSelectedProgramIndex();
        return index >= 0 && index < _stepIds.Count ? _stepIds[index] : null;
    }

    private int GetSelectedProgramIndex()
    {
        return GetSelectedIndex(_programList);
    }

    private static int GetSelectedIndex(ItemList? list)
    {
        var selected = list?.GetSelectedItems();
        return selected is { Length: > 0 } ? selected[0] : -1;
    }

    private static void SelectById(ItemList list, IReadOnlyList<string> ids, string? selectedId)
    {
        if (ids.Count == 0)
        {
            return;
        }

        var index = selectedId is null ? 0 : FindIndex(ids, selectedId);
        list.Select(index < 0 ? 0 : index);
    }

    private static int FindIndex(IReadOnlyList<string> ids, string selectedId)
    {
        for (var i = 0; i < ids.Count; i++)
        {
            if (ids[i] == selectedId)
            {
                return i;
            }
        }

        return -1;
    }

    private static string FormatAction(MacroAction? action)
    {
        return action switch
        {
            null => "none",
            MoveToMacroAction move => $"MoveTo ({move.Destination.X}, {move.Destination.Y})",
            GatherMacroAction gather => $"{gather.Kind} x{RepeatPolicyUi.Format(gather.Repeat)}",
            _ => action.Kind.ToString()
        };
    }
}
