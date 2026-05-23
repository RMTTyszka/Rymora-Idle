namespace RymoraLandOfHeroes.Core.Combat;

public enum CombatEventType
{
    Hit,
    Miss,
    Crit,
    Counter,
    Kill,
    Death
}

public sealed record CombatEvent(
    Combatant Source,
    Combatant Target,
    CombatEventType Type,
    float Damage,
    bool IsCritical,
    bool IsCounter);
