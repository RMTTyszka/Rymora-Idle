using Godot;
using RymoraLandOfHeroes.Core.Party;
using RymoraLandOfHeroes.GodotAdapter.World;

namespace RymoraLandOfHeroes.GodotAdapter.Presentation;

public partial class PartyPresenter : Node2D
{
    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 22, new Color(0.9f, 0.75f, 0.2f));
        DrawCircle(Vector2.Zero, 26, new Color(0.1f, 0.08f, 0.02f), filled: false, width: 4);
    }

    public void Sync(Party party, WorldTileMapAdapter worldAdapter)
    {
        Position = GetVisualPosition(party, worldAdapter);
    }

    private static Vector2 GetVisualPosition(Party party, WorldTileMapAdapter worldAdapter)
    {
        var current = party.ActionQueue.Current;
        if (current?.Request.ActionType != PartyActionType.Travel || current.Request.Path is null)
        {
            return worldAdapter.ToLocalPosition(party.Position);
        }

        var path = current.Request.Path;
        if (current.ExecutedCount >= path.Count)
        {
            return worldAdapter.ToLocalPosition(party.Position);
        }

        var from = worldAdapter.ToLocalPosition(party.Position);
        var to = worldAdapter.ToLocalPosition(path[current.ExecutedCount]);
        var progress = current.Request.TimeToExecute <= 0
            ? 1
            : Mathf.Clamp(current.CurrentTime / current.Request.TimeToExecute, 0, 1);

        return from.Lerp(to, progress);
    }
}
