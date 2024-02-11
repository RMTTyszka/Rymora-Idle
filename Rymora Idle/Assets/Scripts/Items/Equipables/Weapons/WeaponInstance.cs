public class WeaponInstance : Equipable
{
    public WeaponSize Size { get; set; }
    public WeaponsDamageCategory DamageCategory { get; set; }
    // public Material Material { get; set; } change level
    // public Gem Gem { get; set; } change power
    public static WeaponInstance FromTemplate(WeaponTemplate template, int level)
    {
        return new WeaponInstance
        {
            Size = template.size,
            DamageCategory = template.damageCategory,
            Level = level,
            Name = template.name,
            Slot = Slot.MainHand
        };
    }
}
