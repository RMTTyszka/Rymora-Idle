namespace RymoraLandOfHeroes.Core.Automation;

public sealed class PartyAutomation
{
    private readonly List<PartyMacro> _macros = new();

    public IReadOnlyList<PartyMacro> Macros => _macros;
    public PartyProgram Program { get; } = new();
    public ProgramRunner Runner { get; } = new();
    public MacroRecordingSession? Recording { get; private set; }

    public void AddMacro(PartyMacro macro)
    {
        if (macro is null)
        {
            throw new ArgumentNullException(nameof(macro));
        }

        if (TryGetMacro(macro.Id) is not null)
        {
            throw new InvalidOperationException($"Macro already exists: {macro.Id}.");
        }

        _macros.Add(macro);
    }

    public MacroRecordingSession StartRecording(string recordingId)
    {
        if (Recording is not null)
        {
            throw new InvalidOperationException("A Macro recording session is already active.");
        }

        Recording = new MacroRecordingSession(recordingId);
        return Recording;
    }

    public PartyMacro SaveRecording(string name)
    {
        if (Recording is null)
        {
            throw new InvalidOperationException("No Macro recording session is active.");
        }

        var macro = Recording.Save(name);
        if (TryGetMacro(macro.Id) is not null)
        {
            throw new InvalidOperationException($"Macro already exists: {macro.Id}.");
        }

        _macros.Add(macro);
        Recording = null;
        return macro;
    }

    public void CancelRecording()
    {
        Recording = null;
    }

    public void Restore(
        IEnumerable<PartyMacro> macros,
        MacroRecordingSession? recording,
        RepeatPolicy programRepeat,
        IEnumerable<ProgramStep> programSteps,
        int nextProgramStepNumber,
        ProgramRunnerRuntimeState runnerState)
    {
        _macros.Clear();
        foreach (var macro in macros)
        {
            AddMacro(macro);
        }

        Recording = recording;
        Program.Restore(programRepeat, programSteps, nextProgramStepNumber);
        Runner.Restore(runnerState);
    }

    public PartyMacro GetMacro(string macroId)
    {
        return TryGetMacro(macroId)
            ?? throw new InvalidOperationException($"Macro not found: {macroId}.");
    }

    public PartyMacro? TryGetMacro(string macroId)
    {
        return _macros.FirstOrDefault(macro => macro.Id == macroId);
    }
}
