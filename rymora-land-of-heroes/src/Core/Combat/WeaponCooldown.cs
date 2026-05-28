using RymoraLandOfHeroes.Core.Content;

namespace RymoraLandOfHeroes.Core.Combat;

public sealed class WeaponCooldown
{
    public WeaponCooldown(WeaponTemplate weapon, float attackSpeedBonus)
        : this(weapon, attackSpeedBonus, totalCooldownOverride: null)
    {
    }

    private WeaponCooldown(WeaponTemplate weapon, float attackSpeedBonus, float? totalCooldownOverride)
    {
        Weapon = weapon;
        TotalCooldown = totalCooldownOverride ?? CalculateTotalCooldown(weapon, attackSpeedBonus);
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

    public static WeaponCooldown Restore(WeaponTemplate weapon, float currentCooldown, float totalCooldown)
    {
        if (!float.IsFinite(currentCooldown) || !float.IsFinite(totalCooldown) || currentCooldown < 0 || totalCooldown < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentCooldown), "Cooldown values cannot be negative.");
        }

        var cooldown = new WeaponCooldown(weapon, attackSpeedBonus: 0, totalCooldownOverride: totalCooldown);
        cooldown.CurrentCooldown = currentCooldown;
        return cooldown;
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
