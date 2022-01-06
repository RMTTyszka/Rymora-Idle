namespace Global
{
    public class CombatActionResult
    {
        public Creature Performer { get; set; }
        public Creature Target { get; set; }
        public CombatActionType ActionType { get; set; }
        public float Value { get; set; }
    }

    public enum CombatActionType
    {
        PhysicalDamage = 0,
        CriticalDamage= 1,
        Evade = 2,
        Heal = 3,
        MagicDamage = 4,
        CounterPhysicalDamage = 5,
        CounterCriticalDamage = 6
        // TODO
    }   
}