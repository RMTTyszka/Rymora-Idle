namespace RymoraLandOfHeroes.Core.Automation;

public sealed class PartyMacro
{
    private readonly List<MacroAction> _actions = new();

    public PartyMacro(string id, string name, IEnumerable<MacroAction>? actions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        if (actions is not null)
        {
            _actions.AddRange(actions);
        }
    }

    public string Id { get; }
    public string Name { get; private set; }
    public IReadOnlyList<MacroAction> Actions => _actions;

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public void AddAction(MacroAction action)
    {
        _actions.Add(action ?? throw new ArgumentNullException(nameof(action)));
    }

    public void RemoveAction(string actionId)
    {
        var index = _actions.FindIndex(action => action.Id == actionId);
        if (index < 0)
        {
            throw new InvalidOperationException($"Macro action not found: {actionId}.");
        }

        _actions.RemoveAt(index);
    }

    public void MoveAction(string actionId, int newIndex)
    {
        if (newIndex < 0 || newIndex >= _actions.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(newIndex), "Action index is outside the Macro action list.");
        }

        var index = _actions.FindIndex(action => action.Id == actionId);
        if (index < 0)
        {
            throw new InvalidOperationException($"Macro action not found: {actionId}.");
        }

        var action = _actions[index];
        _actions.RemoveAt(index);
        _actions.Insert(newIndex, action);
    }

    public void SetGatherActionRepeat(string actionId, RepeatPolicy repeat)
    {
        var action = _actions.FirstOrDefault(action => action.Id == actionId);
        if (action is null)
        {
            throw new InvalidOperationException($"Macro action not found: {actionId}.");
        }

        if (action is not GatherMacroAction gather)
        {
            throw new InvalidOperationException($"Macro action does not support repeat: {actionId}.");
        }

        gather.SetRepeat(repeat);
    }
}
