namespace Items.Equipables.Weapons
{
    public class WeaponInstance : Equipable
    {
        public WeaponSize Size { get; set; }
        public WeaponsDamageCategory DamageCategory { get; set; }
        // public Material Material { get; set; } change level
        // public Gem Gem { get; set; } change power
    }
}