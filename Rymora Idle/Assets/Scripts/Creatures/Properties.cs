public class Properties
{
    public PropertyInstance Threat { get; set; } = new(nameof(Threat));
    public PropertyInstance Critical { get; set; } = new(nameof(Critical));
    public PropertyInstance Resiliense { get; set; } = new(nameof(Resiliense));
    public PropertyInstance Attack { get; set; } = new(nameof(Attack));
    public PropertyInstance PowerAttack { get; set; } = new(nameof(PowerAttack));
    public PropertyInstance Evasion { get; set; } = new(nameof(Evasion));
    public PropertyInstance PowerDefense { get; set; } = new(nameof(PowerDefense));
    public PropertyInstance SpiritPoints { get; set; } = new(nameof(SpiritPoints));
    public PropertyInstance Life { get; set; } = new(nameof(Life));
    public PropertyInstance ValiantPoints { get; set; } = new(nameof(ValiantPoints));
    public PropertyInstance Protection { get; set; } = new(nameof(Protection));
    public PropertyInstance Fortitude { get; set; } = new(nameof(Fortitude));
    public PropertyInstance AttackDamage { get; set; } = new(nameof(AttackDamage));
    public PropertyInstance PowerDamage { get; set; } = new(nameof(PowerDamage));
    public PropertyInstance AttackSpeed { get; set; } = new(nameof(AttackSpeed));
    public PropertyInstance CastingSpeed { get; set; } = new(nameof(CastingSpeed));
    public PropertyInstance CriticalDamage { get; set; } = new(nameof(CriticalDamage));
    public PropertyInstance ArmorPenetration { get; set; } = new(nameof(ArmorPenetration));
    public PropertyInstance Reaction { get; set; } = new(nameof(Reaction));
    public PropertyInstance Counter { get; set; } = new(nameof(Counter));
    
    public PropertyInstance Get(Property property)
    {
        return GetType().GetProperty(property.ToString())!.GetValue(this) as PropertyInstance;
    }
}