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

    public MacroRecordingSession(string id, IEnumerable<MacroAction> actions, int nextActionNumber)
        : this(id)
    {
        if (nextActionNumber < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nextActionNumber), "Next action number cannot be negative.");
        }

        _actions.AddRange(actions);
        _nextActionNumber = nextActionNumber;
    }

    public string Id { get; }
    public int NextActionNumber => _nextActionNumber;
    public IReadOnlyList<MacroAction> Actions => _actions;

    public void RecordMove(TilePosition target)
    {
        _actions.Add(new MoveToMacroAction(NextActionId("move"), target));
    }

    public void RecordGather(TilePosition target, MacroActionKind kind, string itemName, int itemLevel, float itemWeight)
    {
        var nextActionNumber = _nextActionNumber;
        var move = new MoveToMacroAction($"{Id}-move-{nextActionNumber + 1}", target);
        var gather = new GatherMacroAction($"{Id}-{(kind == MacroActionKind.Mine ? "mine" : "cutwood")}-{nextActionNumber + 2}", kind, itemName, itemLevel, itemWeight, RepeatPolicy.Once);

        _nextActionNumber = nextActionNumber + 2;
        _actions.Add(move);
        _actions.Add(gather);
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
