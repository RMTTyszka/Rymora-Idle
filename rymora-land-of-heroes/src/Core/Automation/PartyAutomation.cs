namespace RymoraLandOfHeroes.Core.Automation;

public sealed class PartyAutomation
{
    private readonly List<PartyMacro> _macros = new();

    public IReadOnlyList<PartyMacro> Macros => _macros;
    public PartyProgram Program { get; } = new();
    public MacroRecordingSession? Recording { get; private set; }

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
