using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.Automation;

public enum MacroActionKind
{
    MoveTo,
    Mine,
    CutWood
}

public abstract class MacroAction
{
    protected MacroAction(string id, MacroActionKind kind)
    {
        Id = RequireId(id);
        Kind = kind;
    }

    public string Id { get; }
    public MacroActionKind Kind { get; }

    protected static string RequireId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return id;
    }
}

public sealed class MoveToMacroAction : MacroAction
{
    public MoveToMacroAction(string id, TilePosition destination)
        : base(id, MacroActionKind.MoveTo)
    {
        Destination = destination;
    }

    public TilePosition Destination { get; private set; }

    public void SetDestination(TilePosition destination)
    {
        Destination = destination;
    }
}

public sealed class GatherMacroAction : MacroAction
{
    public GatherMacroAction(string id, MacroActionKind kind, string itemName, int itemLevel, float itemWeight, RepeatPolicy repeat)
        : base(RequireId(id), ValidateKind(kind))
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemName);
        if (itemLevel <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemLevel), "Item level must be positive.");
        }

        if (!float.IsFinite(itemWeight) || itemWeight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemWeight), "Item weight cannot be negative.");
        }

        ItemName = itemName;
        ItemLevel = itemLevel;
        ItemWeight = itemWeight;
        Repeat = repeat ?? throw new ArgumentNullException(nameof(repeat));
    }

    public string ItemName { get; }
    public int ItemLevel { get; }
    public float ItemWeight { get; }
    public RepeatPolicy Repeat { get; private set; }

    public void SetRepeat(RepeatPolicy repeat)
    {
        Repeat = repeat ?? throw new ArgumentNullException(nameof(repeat));
    }

    private static MacroActionKind ValidateKind(MacroActionKind kind)
    {
        return kind is MacroActionKind.Mine or MacroActionKind.CutWood
            ? kind
            : throw new ArgumentException("Gather action kind must be Mine or CutWood.", nameof(kind));
    }
}
