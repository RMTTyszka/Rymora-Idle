using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.Automation;

public sealed class MacroRecordingSession
{
    private readonly List<MacroAction> _actions = new();
    private int _nextActionNumber;

    public MacroRecordingSession(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
    }

    public string Id { get; }
    public IReadOnlyList<MacroAction> Actions => _actions;

    public void RecordMove(TilePosition target)
    {
        _actions.Add(new MoveToMacroAction(NextActionId("move"), target));
    }

    public void RecordGather(TilePosition target, MacroActionKind kind, string itemName, int itemLevel, float itemWeight)
    {
        RecordMove(target);
        _actions.Add(new GatherMacroAction(NextActionId(kind == MacroActionKind.Mine ? "mine" : "cutwood"), kind, itemName, itemLevel, itemWeight, RepeatPolicy.Once));
    }

    public PartyMacro Save(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new PartyMacro(Id, name, _actions.ToArray());
    }

    private string NextActionId(string prefix)
    {
        _nextActionNumber++;
        return $"{Id}-{prefix}-{_nextActionNumber}";
    }
}
