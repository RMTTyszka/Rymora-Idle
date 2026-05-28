using RymoraLandOfHeroes.Core.Content;
using RymoraLandOfHeroes.Core.Hero;

namespace RymoraLandOfHeroes.Core.Combat;

public sealed class Combatant
{
	public Combatant(Creature creature)
	{
		Creature = creature;

		var attackSpeed = creature.Properties[PropertyType.AttackSpeed].GetValue();
		MainHandCooldown = CreateCooldown(creature.Equipment.MainHand, attackSpeed, nameof(creature.Equipment.MainHand));
		OffhandCooldown = creature.Equipment.Offhand is null
			? null
			: new WeaponCooldown(creature.Equipment.Offhand, attackSpeed);
	}

	private Combatant(Creature creature, WeaponCooldown mainHandCooldown, WeaponCooldown? offhandCooldown)
	{
		Creature = creature;
		MainHandCooldown = mainHandCooldown;
		OffhandCooldown = offhandCooldown;
	}

	public Creature Creature { get; }
	public WeaponCooldown MainHandCooldown { get; }
	public WeaponCooldown? OffhandCooldown { get; }

	public IEnumerable<WeaponCooldown> Cooldowns
	{
		get
		{
			yield return MainHandCooldown;
			if (OffhandCooldown is not null)
			{
				yield return OffhandCooldown;
			}
		}
	}

	public static Combatant Restore(Creature creature, WeaponCooldown mainHandCooldown, WeaponCooldown? offhandCooldown)
	{
		return new Combatant(creature, mainHandCooldown, offhandCooldown);
	}

	private static WeaponCooldown CreateCooldown(WeaponTemplate? weapon, float attackSpeed, string slotName)
	{
		if (weapon is null)
		{
			throw new InvalidOperationException($"Combatant requires weapon in {slotName}.");
		}

		return new WeaponCooldown(weapon, attackSpeed);
	}
}
