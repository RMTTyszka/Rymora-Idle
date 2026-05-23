# Combate - Rymora Land of Heroes

Data de criacao: 2026-05-23
Regras de negocio: `docs/regras/biblia-rpg.md` (secao Combate, Escolha de Alvo, Acerto e Evasao, Dano, Critico, Contra-Ataque)
Arquitetura geral: `docs/arquitetura/visao-geral.md`

---

## 1. Proposito

O dominio **Combat** cobre instancia de combate, participantes, cooldowns, escolha de alvo, resolucao de ataques, eventos de resultado e condicoes de vitoria/derrota.

Combat nao possui:
- corpos visuais na tela
- texto flutuante
- cameras
- botoes ou paineis

---

## 2. Inicio do combate

1. Durante viagem, World detecta encontro.
2. Application cria `CombatInstance` com os participantes (herois da Party + monstros do encontro).
3. Party marca `IsInCombat = true`.
4. Adapter Godot instancia corpos visuais se a Party for a selecionada.

---

## 3. CombatInstance

```csharp
public sealed class CombatInstance
{
    public IReadOnlyList<Combatant> Heroes { get; }
    public IReadOnlyList<Combatant> Monsters { get; }
    public CombatState State { get; } // Ongoing, HeroesVictory, MonstersVictory
    public event Action<CombatEvent> OnEvent;
}
```

`Combatant` envolve um heroi ou monstro com estado de combate:

```csharp
public sealed class Combatant
{
    public Creature Creature { get; }
    public WeaponCooldown MainHandCooldown { get; }
    public WeaponCooldown? OffhandCooldown { get; }
}

public sealed class WeaponCooldown
{
    public WeaponTemplate Weapon { get; }
    public float CurrentCooldown { get; set; }
    public float TotalCooldown { get; }
}
```

---

## 4. Turno

Combate automatico por cooldown. `CombatInstance.RunTurn(float deltaTime)`:

1. Herois: para cada heroi vivo, reduz cooldown de cada arma por `deltaTime`. Se cooldown <= 0, ataca com aquela arma.
2. Monstros: mesmo processo.
3. Verifica condicoes de vitoria/derrota.

Cada combatente ataca com arma principal. Se houver arma secundaria, ela possui `WeaponCooldown` proprio e resolve ataque independente.

### 4.1 Cooldown

`TotalCooldown = weapon.AttackSpeed / (1 + combatant.Properties.AttackSpeed)`

O cooldown e reduzido a cada tick por `deltaTime`. `AttackSpeed` acelera ataque diminuindo `TotalCooldown`, nao multiplicando duas vezes o avanco do cooldown.

---

## 5. Escolha de alvo

Alvo = inimigo com maior aggro.

```
aggro = targetingConfig.LowLifeWeight * (1 - currentLife / maxLife)
      + targetingConfig.ThreatWeight * combatant.Properties.Threat
```

Quanto menos vida, maior o aggro se `LowLifeWeight` for positivo. Peso final fica em configuracao ate validacao da regra de alvo.

---

## 6. Resolucao de ataque

### 6.1 Acerto

```
hitRoll = combatRandom.Roll(combatConfig.HitRollRange) + weaponSkill + weaponAttribute + Hit + weaponSizeHitMod
evadeRoll = combatRandom.Roll(combatConfig.EvadeRollRange) + Agility + Tactics + Evasion + armorEvasion

acertou = hitRoll >= evadeRoll
```

### 6.2 Dano

```
danoBase = weaponDamage * weaponSizeMultiplier
danoSkill = weaponSkillValue + Armslore
danoAtributo = damageAttributeValue
danoBonus = AttackDamage

danoBruto = danoBase + danoSkill + danoAtributo + danoBonus

fortitude = Parry + Vitality + Fortitude
protecao = Protection + armorProtection
penetracao = weaponPenetration + ArmorPenetration
protecaoEfetiva = max(0, protecao - penetracao)

danoFinal = max(0, danoBruto - fortitude - protecaoEfetiva)
```

### 6.3 Critico

```
chanceCritico = Critical - target.Resiliense + armaVsArmorModifier
se critico: danoFinal *= (combatConfig.BaseCriticalMultiplier + CriticalDamage)
```

Modificador arma vs armadura:
- Light vs Light: bonus
- Heavy vs Heavy: bonus
- Medium: bonus menor
- outras combinacoes: penalidade

### 6.4 Contra-ataque

Se ataque erra e nao foi contra-ataque:

```
chanceContra = Counter + weaponCounterPotential + armaVsArmorCounterMod
se passar: defensor ataca atacante
```

- Light vs Heavy: bonus de contra-ataque
- Heavy vs Light: bonus de contra-ataque

---

## 7. Eventos de combate

`CombatEvent` carrega resultado de cada acao:

```csharp
public sealed record CombatEvent(
    Combatant Source,
    Combatant Target,
    EventType Type, // Hit, Miss, Crit, Counter, Kill, Death
    float Damage,
    bool IsCritical,
    bool IsCounter
)
```

Eventos sao publicados para adaptadores Godot renderizarem (floating text, animacoes).

---

## 8. Fim do combate

- Todos monstros mortos -> `HeroesVictory`. Limpa instancia, sem loot implementado ainda.
- Todos herois mortos -> `MonstersVictory`. Party morre, acao de morte inicia.

---

## 9. Efeitos e condicoes

Placeholder na v1. Estrutura prevista:

```csharp
public enum Condition { Dead, Stunned, Poisoned, Frozen, Scared }
```

Efeitos: beneficos ou prejudiciais, acumulaveis, com duracao maxima e contagem regressiva.
