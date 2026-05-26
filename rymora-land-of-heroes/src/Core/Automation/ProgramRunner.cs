namespace RymoraLandOfHeroes.Core.Automation;

public enum ProgramRunnerState
{
    Idle,
    Running,
    PauseRequested,
    Paused,
    StopRequested,
    Error
}

public sealed class ProgramRunner
{
    private IReadOnlyList<MacroAction> _currentMacroActions = Array.Empty<MacroAction>();
    private int _programStepIndex;
    private int _macroActionIndex;
    private int _stepIteration;
    private int _programIteration;
    private int _actionIteration;
    private float _programElapsedSeconds;
    private float _stepElapsedSeconds;
    private float _actionElapsedSeconds;
    private string? _currentExecutionId;

    public ProgramRunnerState State { get; private set; } = ProgramRunnerState.Idle;
    public string? ErrorMessage { get; private set; }
    public MacroAction? CurrentAction { get; private set; }
    public MacroAction? NextAction => _macroActionIndex < _currentMacroActions.Count ? _currentMacroActions[_macroActionIndex] : null;

    public void Play()
    {
        if (State == ProgramRunnerState.Paused)
        {
            State = ProgramRunnerState.Running;
            return;
        }

        _programStepIndex = 0;
        _macroActionIndex = 0;
        _stepIteration = 0;
        _programIteration = 0;
        _actionIteration = 0;
        _programElapsedSeconds = 0;
        _stepElapsedSeconds = 0;
        _actionElapsedSeconds = 0;
        _currentMacroActions = Array.Empty<MacroAction>();
        _currentExecutionId = null;
        ErrorMessage = null;
        CurrentAction = null;
        State = ProgramRunnerState.Running;
    }

    public void Pause()
    {
        if (State == ProgramRunnerState.Running)
        {
            State = ProgramRunnerState.PauseRequested;
        }
    }

    public void Stop()
    {
        if (State == ProgramRunnerState.Paused)
        {
            ResetToIdle();
            return;
        }

        if (State is ProgramRunnerState.Running or ProgramRunnerState.PauseRequested)
        {
            State = ProgramRunnerState.StopRequested;
        }
    }

    public MacroActionExecution? TryStartNextAction(PartyAutomation automation)
    {
        if (State != ProgramRunnerState.Running || _currentExecutionId is not null)
        {
            return null;
        }

        if (automation.Program.Steps.Count == 0)
        {
            Fail("Program has no Macro steps.");
            return null;
        }

        if (_currentMacroActions.Count == 0 || _macroActionIndex >= _currentMacroActions.Count)
        {
            if (!StartNextMacro(automation))
            {
                if (State == ProgramRunnerState.Running)
                {
                    State = ProgramRunnerState.Idle;
                }

                return null;
            }
        }

        var action = _currentMacroActions[_macroActionIndex];
        _actionIteration++;
        _currentExecutionId = $"run-{Guid.NewGuid():N}";
        CurrentAction = action;
        return new MacroActionExecution(_currentExecutionId, action);
    }

    public void CompleteAction(string executionId, float elapsedSeconds)
    {
        if (_currentExecutionId != executionId)
        {
            throw new InvalidOperationException($"Unknown automation execution id: {executionId}.");
        }

        _currentExecutionId = null;
        CurrentAction = null;
        _actionElapsedSeconds += elapsedSeconds;
        _stepElapsedSeconds += elapsedSeconds;
        _programElapsedSeconds += elapsedSeconds;

        var shouldRepeatCurrentAction = ShouldRepeatCurrentAction();
        if (!shouldRepeatCurrentAction)
        {
            _macroActionIndex++;
            _actionIteration = 0;
            _actionElapsedSeconds = 0;
        }

        if (State == ProgramRunnerState.PauseRequested)
        {
            State = ProgramRunnerState.Paused;
            return;
        }

        if (State == ProgramRunnerState.StopRequested)
        {
            ResetToIdle();
        }
    }

    public void Fail(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ErrorMessage = message;
        _currentExecutionId = null;
        CurrentAction = null;
        State = ProgramRunnerState.Error;
    }

    private bool StartNextMacro(PartyAutomation automation)
    {
        while (true)
        {
            if (_programStepIndex >= automation.Program.Steps.Count)
            {
                _programIteration++;
                if (!ShouldRepeat(automation.Program.Repeat, _programIteration, _programElapsedSeconds))
                {
                    return false;
                }

                _programStepIndex = 0;
                _stepIteration = 0;
                _stepElapsedSeconds = 0;
            }

            var step = automation.Program.Steps[_programStepIndex];
            if (!ShouldRepeat(step.Repeat, _stepIteration, _stepElapsedSeconds))
            {
                _programStepIndex++;
                _stepIteration = 0;
                _stepElapsedSeconds = 0;
                continue;
            }

            var macro = automation.TryGetMacro(step.MacroId);
            if (macro is null)
            {
                Fail($"Macro not found: {step.MacroId}.");
                return false;
            }

            _currentMacroActions = macro.Actions.ToArray();
            _macroActionIndex = 0;
            _actionIteration = 0;
            _actionElapsedSeconds = 0;
            _stepIteration++;

            if (_currentMacroActions.Count == 0)
            {
                _programStepIndex++;
                _stepIteration = 0;
                _stepElapsedSeconds = 0;
                continue;
            }

            return true;
        }
    }

    private bool ShouldRepeatCurrentAction()
    {
        if (_macroActionIndex >= _currentMacroActions.Count)
        {
            return false;
        }

        var action = _currentMacroActions[_macroActionIndex];
        if (action is not GatherMacroAction gather)
        {
            return false;
        }

        return ShouldRepeat(gather.Repeat, _actionIteration, _actionElapsedSeconds);
    }

    private static bool ShouldRepeat(RepeatPolicy repeat, int completedIterations, float elapsedSeconds)
    {
        return repeat.Mode switch
        {
            RepeatMode.Once => completedIterations < 1,
            RepeatMode.Count => completedIterations < repeat.RepeatCount!.Value,
            RepeatMode.Forever => true,
            RepeatMode.Duration => elapsedSeconds < repeat.Seconds!.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(repeat), "Unknown repeat mode.")
        };
    }

    private void ResetToIdle()
    {
        _programStepIndex = 0;
        _macroActionIndex = 0;
        _stepIteration = 0;
        _programIteration = 0;
        _actionIteration = 0;
        _programElapsedSeconds = 0;
        _stepElapsedSeconds = 0;
        _actionElapsedSeconds = 0;
        _currentMacroActions = Array.Empty<MacroAction>();
        _currentExecutionId = null;
        CurrentAction = null;
        State = ProgramRunnerState.Idle;
    }
}
