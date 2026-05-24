using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.Content;

public sealed record WeaponTemplate(
    string Name,
    int Level,
    WeaponSize Size,
    WeaponDamageCategory DamageCategory,
    float AttackSpeed,
    float BaseDamage,
    float SizeMultiplier,
    float HitModifier,
    float Penetration,
    float CounterPotential);

public sealed record ArmorTemplate(
    string Name,
    int Level,
    ArmorCategory Category,
    float Protection,
    float Evasion);

public sealed record CreatureTemplate(
    string CreatureName,
    MonsterClass Class,
    SpriteReference Sprite);

public sealed record EncounterTemplate(
    string Id,
    string Name,
    int Level,
    IReadOnlyList<CreatureTemplate> Monsters);

public sealed record MaterialItem(string Name, int Level, float Weight);
