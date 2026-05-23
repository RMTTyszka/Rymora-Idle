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
        Position = worldAdapter.ToLocalPosition(party.Position);
    }
}
