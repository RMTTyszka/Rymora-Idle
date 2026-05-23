using RymoraLandOfHeroes.Core.Content;

namespace RymoraLandOfHeroes.Core.Hero;

public sealed class Equipment
{
    public WeaponTemplate? MainHand { get; set; }
    public WeaponTemplate? Offhand { get; set; }
    public ArmorTemplate? Chest { get; set; }
}
