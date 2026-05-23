using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;
using RymoraLandOfHeroes.Core.Content;
using RymoraLandOfHeroes.Core.Hero;

namespace RymoraLandOfHeroes.Core.Combat;

public sealed class CombatInstance
{
    private readonly CombatConfig _config;
    private readonly IRandomSource _randomSource;

    public CombatInstance(
        IEnumerable<Creature> heroes,
        IEnumerable<Creature> monsters,
        CombatConfig config,
        IRandomSource? randomSource = null)
    {
        _config = config;
        _config.HitRollRange.Validate();
        _config.EvadeRollRange.Validate();
        _randomSource = randomSource ?? new SystemRandomSource();
        Heroes = heroes.Select(hero => new Combatant(hero)).ToArray();
        Monsters = monsters.Select(monster => new Combatant(monster)).ToArray();
        UpdateState();
    }

    public IReadOnlyList<Combatant> Heroes { get; }
    public IReadOnlyList<Combatant> Monsters { get; }
    public CombatState State { get; private set; }
    public event Action<CombatEvent>? OnEvent;

    public void RunTurn(float deltaTime)
    {
        if (deltaTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time cannot be negative.");
        }

        if (State != CombatState.Ongoing)
        {
            return;
        }

        RunSide(Heroes, Monsters, deltaTime);
        if (UpdateState() != CombatState.Ongoing)
        {
            return;
        }

        RunSide(Monsters, Heroes, deltaTime);
        UpdateState();
    }

    private void RunSide(IReadOnlyList<Combatant> attackers, IReadOnlyList<Combatant> defenders, float deltaTime)
    {
        foreach (var attacker in attackers.Where(combatant => combatant.Creature.IsAlive))
        {
            foreach (var cooldown in attacker.Cooldowns)
            {
                cooldown.Tick(deltaTime);
                if (!cooldown.IsReady)
                {
                    continue;
                }

                var target = ChooseTarget(defenders);
                if (target is null)
                {
                    return;
                }

                ResolveAttack(attacker, target, cooldown.Weapon, isCounter: false);
                cooldown.Reset();

                if (UpdateState() != CombatState.Ongoing)
                {
                    return;
                }
            }
        }
    }

    private Combatant? ChooseTarget(IEnumerable<Combatant> candidates)
    {
        return candidates
            .Where(combatant => combatant.Creature.IsAlive)
            .OrderByDescending(GetAggro)
            .ThenBy(combatant => combatant.Creature.Name, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private float GetAggro(Combatant combatant)
    {
        var creature = combatant.Creature;
        var missingLifeRatio = creature.MaxLife <= 0 ? 0 : 1 - creature.Life / creature.MaxLife;
        return _config.Targeting.LowLifeWeight * missingLifeRatio
            + _config.Targeting.ThreatWeight * creature.Properties[PropertyType.Threat].GetValue();
    }

    private void ResolveAttack(Combatant source, Combatant target, WeaponTemplate weapon, bool isCounter)
    {
        if (!RollHit(source.Creature, target.Creature, weapon))
        {
            Publish(new CombatEvent(source, target, CombatEventType.Miss, 0, IsCritical: false, IsCounter: isCounter));
            TryCounter(source, target, isCounter);
            return;
        }

        var damage = CalculateDamage(source.Creature, target.Creature, weapon);
        var critical = RollCritical(source.Creature, target.Creature);
        if (critical)
        {
            damage *= _config.BaseCriticalMultiplier + source.Creature.Properties[PropertyType.CriticalDamage].GetValue();
        }

        target.Creature.TakeDamage(damage);
        Publish(new CombatEvent(source, target, critical ? CombatEventType.Crit : CombatEventType.Hit, damage, critical, isCounter));

        if (!target.Creature.IsAlive)
        {
            Publish(new CombatEvent(source, target, CombatEventType.Kill, 0, IsCritical: false, IsCounter: isCounter));
            Publish(new CombatEvent(target, target, CombatEventType.Death, 0, IsCritical: false, IsCounter: isCounter));
        }
    }

    private bool RollHit(Creature source, Creature target, WeaponTemplate weapon)
    {
        var hitRoll = Roll(_config.HitRollRange)
            + GetWeaponSkill(source, weapon).GetValue()
            + GetHitAttribute(source, weapon).GetValue()
            + source.Properties[PropertyType.Hit].GetValue()
            + weapon.HitModifier;

        var evadeRoll = Roll(_config.EvadeRollRange)
            + target.Attributes[AttributeType.Agility].GetValue()
            + target.Skills[SkillType.Tactics].GetValue()
            + target.Properties[PropertyType.Evasion].GetValue()
            + (target.Equipment.Chest?.Evasion ?? 0);

        return hitRoll >= evadeRoll;
    }

    private float CalculateDamage(Creature source, Creature target, WeaponTemplate weapon)
    {
        var damage = weapon.BaseDamage * weapon.SizeMultiplier
            + GetWeaponSkill(source, weapon).GetValue()
            + source.Skills[SkillType.Armslore].GetValue()
            + GetDamageAttribute(source, weapon).GetValue()
            + source.Properties[PropertyType.AttackDamage].GetValue();

        var fortitude = target.Skills[SkillType.Parry].GetValue()
            + target.Attributes[AttributeType.Vitality].GetValue()
            + target.Properties[PropertyType.Fortitude].GetValue();
        var protection = target.Properties[PropertyType.Protection].GetValue() + (target.Equipment.Chest?.Protection ?? 0);
        var penetration = weapon.Penetration + source.Properties[PropertyType.ArmorPenetration].GetValue();
        var effectiveProtection = Math.Max(0, protection - penetration);

        return Math.Max(0, damage - fortitude - effectiveProtection);
    }

    private bool RollCritical(Creature source, Creature target)
    {
        var chance = source.Properties[PropertyType.Critical].GetValue()
            - target.Properties[PropertyType.Resiliense].GetValue();

        return chance > 0 && _randomSource.NextInclusive(1, 100) <= chance;
    }

    private void TryCounter(Combatant source, Combatant target, bool isCounter)
    {
        if (isCounter || !target.Creature.IsAlive || target.Creature.Equipment.MainHand is null)
        {
            return;
        }

        var chance = target.Creature.Properties[PropertyType.Counter].GetValue()
            + target.Creature.Equipment.MainHand.CounterPotential;
        if (chance <= 0 || _randomSource.NextInclusive(1, 100) > chance)
        {
            return;
        }

        Publish(new CombatEvent(target, source, CombatEventType.Counter, 0, IsCritical: false, IsCounter: true));
        ResolveAttack(target, source, target.Creature.Equipment.MainHand, isCounter: true);
    }

    private int Roll(RollRange range) => _randomSource.NextInclusive(range.Min, range.Max);

    private static StatInstance GetWeaponSkill(Creature creature, WeaponTemplate weapon)
    {
        return creature.Skills[weapon.DamageCategory switch
        {
            WeaponDamageCategory.Cutting => SkillType.Swordmanship,
            WeaponDamageCategory.Piercing => SkillType.Fencing,
            WeaponDamageCategory.Ranged => SkillType.Archery,
            WeaponDamageCategory.Thrown => SkillType.Archery,
            WeaponDamageCategory.Catalyst => SkillType.Magery,
            WeaponDamageCategory.None => SkillType.Wrestling,
            WeaponDamageCategory.Smashing => SkillType.Heavyweaponship,
            _ => throw new ArgumentOutOfRangeException(nameof(weapon), "Unknown weapon damage category.")
        }];
    }

    private static StatInstance GetHitAttribute(Creature creature, WeaponTemplate weapon)
    {
        return creature.Attributes[weapon.DamageCategory switch
        {
            WeaponDamageCategory.Cutting => AttributeType.Strength,
            WeaponDamageCategory.Piercing => AttributeType.Agility,
            WeaponDamageCategory.Ranged => AttributeType.Agility,
            WeaponDamageCategory.Thrown => AttributeType.Agility,
            WeaponDamageCategory.Catalyst => AttributeType.Intuition,
            WeaponDamageCategory.None => AttributeType.Vitality,
            WeaponDamageCategory.Smashing => AttributeType.Vitality,
            _ => throw new ArgumentOutOfRangeException(nameof(weapon), "Unknown weapon damage category.")
        }];
    }

    private static StatInstance GetDamageAttribute(Creature creature, WeaponTemplate weapon)
    {
        return creature.Attributes[weapon.DamageCategory switch
        {
            WeaponDamageCategory.Cutting => AttributeType.Strength,
            WeaponDamageCategory.Piercing => AttributeType.Agility,
            WeaponDamageCategory.Ranged => AttributeType.Agility,
            WeaponDamageCategory.Thrown => AttributeType.Strength,
            WeaponDamageCategory.Catalyst => AttributeType.Wisdom,
            WeaponDamageCategory.None => AttributeType.Vitality,
            WeaponDamageCategory.Smashing => AttributeType.Strength,
            _ => throw new ArgumentOutOfRangeException(nameof(weapon), "Unknown weapon damage category.")
        }];
    }

    private CombatState UpdateState()
    {
        var heroesAlive = Heroes.Any(combatant => combatant.Creature.IsAlive);
        var monstersAlive = Monsters.Any(combatant => combatant.Creature.IsAlive);

        State = (heroesAlive, monstersAlive) switch
        {
            (true, true) => CombatState.Ongoing,
            (true, false) => CombatState.HeroesVictory,
            (false, true) => CombatState.MonstersVictory,
            _ => CombatState.MonstersVictory
        };

        return State;
    }

    private void Publish(CombatEvent combatEvent) => OnEvent?.Invoke(combatEvent);
}
