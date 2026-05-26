namespace RymoraLandOfHeroes.Core.Automation;

public sealed class PartyProgram
{
    private readonly List<ProgramStep> _steps = new();
    private int _nextStepNumber;

    public IReadOnlyList<ProgramStep> Steps => _steps;
    public RepeatPolicy Repeat { get; private set; } = RepeatPolicy.Once;

    public ProgramStep AddStep(string macroId, RepeatPolicy repeat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(macroId);
        var step = new ProgramStep(NextStepId(), macroId, repeat);
        _steps.Add(step);
        return step;
    }

    public void RemoveStep(string stepId)
    {
        var index = _steps.FindIndex(step => step.Id == stepId);
        if (index < 0)
        {
            throw new InvalidOperationException($"Program step not found: {stepId}.");
        }

        _steps.RemoveAt(index);
    }

    public void MoveStep(string stepId, int newIndex)
    {
        if (newIndex < 0 || newIndex >= _steps.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(newIndex), "Program step index is outside the Program step list.");
        }

        var index = _steps.FindIndex(step => step.Id == stepId);
        if (index < 0)
        {
            throw new InvalidOperationException($"Program step not found: {stepId}.");
        }

        var step = _steps[index];
        _steps.RemoveAt(index);
        _steps.Insert(newIndex, step);
    }

    public void SetProgramRepeat(RepeatPolicy repeat)
    {
        Repeat = repeat ?? throw new ArgumentNullException(nameof(repeat));
    }

    public void SetStepRepeat(string stepId, RepeatPolicy repeat)
    {
        var index = _steps.FindIndex(step => step.Id == stepId);
        if (index < 0)
        {
            throw new InvalidOperationException($"Program step not found: {stepId}.");
        }

        _steps[index].SetRepeat(repeat);
    }

    private string NextStepId()
    {
        _nextStepNumber++;
        return $"step-{_nextStepNumber}";
    }
}

public sealed class ProgramStep
{
    public ProgramStep(string id, string macroId, RepeatPolicy repeat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(macroId);

        Id = id;
        MacroId = macroId;
        Repeat = repeat ?? throw new ArgumentNullException(nameof(repeat));
    }

    public string Id { get; }
    public string MacroId { get; }
    public RepeatPolicy Repeat { get; private set; }

    public void SetRepeat(RepeatPolicy repeat)
    {
        Repeat = repeat ?? throw new ArgumentNullException(nameof(repeat));
    }
}
