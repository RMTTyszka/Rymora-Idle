using System.Collections.Generic;
using Godot;
using RymoraLandOfHeroes.Core.Combat;

namespace RymoraLandOfHeroes.GodotAdapter.Presentation;

public partial class CombatPresenter : Node2D
{
    private const int MaxHistoryLines = 8;

    private readonly Queue<string> _history = new();
    private CombatInstance? _combat;

    public void Sync(CombatInstance? combat)
    {
        _combat = combat;
        Visible = combat is not null;
        QueueRedraw();
    }

    public void ClearHistory()
    {
        _history.Clear();
        QueueRedraw();
    }

    public void AddEvent(CombatEvent combatEvent)
    {
        _history.Enqueue(FormatEvent(combatEvent));
        while (_history.Count > MaxHistoryLines)
        {
            _history.Dequeue();
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_combat is null)
        {
            return;
        }

        DrawRect(new Rect2(20, 20, 600, 260), new Color(0.02f, 0.02f, 0.03f, 0.88f));
        DrawRect(new Rect2(20, 20, 600, 260), new Color(0.85f, 0.70f, 0.35f), filled: false, width: 3);

        DrawSide(_combat.Heroes, startX: 90, new Color(0.20f, 0.58f, 0.95f));
        DrawSide(_combat.Monsters, startX: 430, new Color(0.85f, 0.20f, 0.18f));
        DrawHistory();
    }

    private void DrawSide(IReadOnlyList<Combatant> combatants, float startX, Color color)
    {
        for (var index = 0; index < combatants.Count; index++)
        {
            var combatant = combatants[index];
            var y = 80 + index * 80;
            var lifeRatio = combatant.Creature.MaxLife <= 0
                ? 0
                : Mathf.Clamp(combatant.Creature.Life / combatant.Creature.MaxLife, 0, 1);

            DrawCircle(new Vector2(startX, y), 24, color);
            DrawCircle(new Vector2(startX, y), 28, Colors.Black, filled: false, width: 3);
            DrawRect(new Rect2(startX - 45, y + 36, 90, 12), new Color(0.18f, 0.03f, 0.03f));
            DrawRect(new Rect2(startX - 45, y + 36, 90 * lifeRatio, 12), new Color(0.15f, 0.75f, 0.20f));
            DrawRect(new Rect2(startX - 45, y + 36, 90, 12), Colors.Black, filled: false, width: 2);
        }
    }

    private void DrawHistory()
    {
        DrawRect(new Rect2(640, 20, 430, 260), new Color(0.01f, 0.01f, 0.015f, 0.88f));
        DrawRect(new Rect2(640, 20, 430, 260), new Color(0.45f, 0.35f, 0.18f), filled: false, width: 2);
        DrawString(ThemeDB.FallbackFont, new Vector2(660, 50), "Combat history", HorizontalAlignment.Left, -1, 18, Colors.White);

        var y = 82;
        foreach (var entry in _history)
        {
            DrawString(ThemeDB.FallbackFont, new Vector2(660, y), entry, HorizontalAlignment.Left, -1, 14, new Color(0.92f, 0.88f, 0.72f));
            y += 24;
        }
    }

    private static string FormatEvent(CombatEvent combatEvent)
    {
        return combatEvent.Type switch
        {
            CombatEventType.Hit => $"{combatEvent.Source.Creature.Name} hits {combatEvent.Target.Creature.Name} for {combatEvent.Damage:0.0}",
            CombatEventType.Crit => $"{combatEvent.Source.Creature.Name} crits {combatEvent.Target.Creature.Name} for {combatEvent.Damage:0.0}",
            CombatEventType.Miss => $"{combatEvent.Source.Creature.Name} misses {combatEvent.Target.Creature.Name}",
            CombatEventType.Counter => $"{combatEvent.Source.Creature.Name} counters {combatEvent.Target.Creature.Name}",
            CombatEventType.Kill => $"{combatEvent.Source.Creature.Name} kills {combatEvent.Target.Creature.Name}",
            CombatEventType.Death => $"{combatEvent.Target.Creature.Name} dies",
            _ => $"{combatEvent.Type}: {combatEvent.Source.Creature.Name} -> {combatEvent.Target.Creature.Name}"
        };
    }
}
