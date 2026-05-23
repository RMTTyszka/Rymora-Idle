using RymoraLandOfHeroes.Core.Content;

namespace RymoraLandOfHeroes.Core.Combat;

public sealed class WeaponCooldown
{
    public WeaponCooldown(WeaponTemplate weapon, float attackSpeedBonus)
    {
        Weapon = weapon;
        TotalCooldown = CalculateTotalCooldown(weapon, attackSpeedBonus);
        CurrentCooldown = TotalCooldown;
    }

    public WeaponTemplate Weapon { get; }
    public float CurrentCooldown { get; private set; }
    public float TotalCooldown { get; }
    public bool IsReady => CurrentCooldown <= 0;

    public void Tick(float deltaTime)
    {
        if (deltaTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time cannot be negative.");
        }

        CurrentCooldown = Math.Max(0, CurrentCooldown - deltaTime);
    }

    public void Reset()
    {
        CurrentCooldown = TotalCooldown;
    }

    public static float CalculateTotalCooldown(WeaponTemplate weapon, float attackSpeedBonus)
    {
        var divisor = 1 + attackSpeedBonus;
        if (divisor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(attackSpeedBonus), "AttackSpeed bonus cannot reduce cooldown divisor to zero or below.");
        }

        return weapon.AttackSpeed / divisor;
    }
}
