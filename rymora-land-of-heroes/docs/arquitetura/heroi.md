# Heroi - Rymora Land of Heroes

Data de criacao: 2026-05-23
Regras de negocio: `docs/regras/biblia-rpg.md` (secao Herois, Atributos, Pericias, Propriedades, Equipamentos, Armas, Armaduras, Progressao)
Arquitetura geral: `docs/arquitetura/visao-geral.md`

---

## 1. Proposito

O dominio **Hero** cobre criaturas (herois e monstros), atributos, pericias, propriedades, equipamentos e progressao por uso.

---

## 2. Creature

```csharp
public sealed class Creature
{
    public string Name { get; }
    public int Level { get; } // calculado de atributos + pericias
    public float Life { get; set; }
    public float MaxLife { get; }
    public Attributes Attributes { get; }
    public Skills Skills { get; }
    public Properties Properties { get; }
    public Equipment Equipment { get; }
    public SpriteReference Sprite { get; } // identificador do sprite, nao a texture
}
```

### 2.1 Level

```
Level = totalPontosAtributos + totalPontosPericias / algumDivisor
```

Formula exata: adiada (decisao pendente).

### 2.2 MaxLife

```
MaxLife = 100 + Vitality.GetValue(0) * 10
```

---

## 3. Atributos

```csharp
public sealed class Attributes
{
    public AttributeInstance Strength { get; }
    public AttributeInstance Agility { get; }
    public AttributeInstance Vitality { get; }
    public AttributeInstance Wisdom { get; }
    public AttributeInstance Intuition { get; }
    public AttributeInstance Charisma { get; }
}
```

### 3.1 AttributeInstance

```csharp
public sealed class AttributeInstance
{
    public float Points { get; } // pontos brutos
    public float GetValue(int challengeLevel); // valor efetivo

    public void AddBonus(Bonus bonus);
    public void RemoveBonus(Bonus bonus);
    public bool TryGain(float challengeLevel); // progressao por uso
}
```

- Comeca com 5 pontos.
- Divisor: 5 (valor efetivo = Points / 5).
- Bonus por tipo: Innate, Magic, Equipment, Food, Furniture.

### 3.2 Papeis dos atributos

| Atributo | Papel |
|----------|-------|
| Strength | Dano armas Cutting e Thrown |
| Agility | Acerto armas Piercing/Ranged/Thrown, evasao |
| Vitality | MaxLife, acerto armas None e Smashing |
| Wisdom | Dano Catalyst |
| Intuition | Acerto Catalyst |
| Charisma | Mercantilism, futuro bardo |

---

## 4. Pericias

### 4.1 SkillInstance

```csharp
public sealed class SkillInstance
{
    public float Points { get; }
    public float GetValue(int challengeLevel);

    public void AddBonus(Bonus bonus);
    public void RemoveBonus(Bonus bonus);
    public bool TryGain(float challengeLevel);
}
```

- Comeca com 5 pontos.
- Divisor: 5.
- Progressao por uso.

### 4.2 Lista de pericias (31)

Alchemy, Anatomy, AnimalTaming, Archery, Armorcrafting, Armslore, Awareness, Bowcrafting, Carpentery, Fencing, Gathering, Healing, Heavyweaponship, Jewelcrafting, Leatherworking, Lore, Lumberjacking, Magery, Mercantilism, Military, Mining, Parry, Reflex, ResistSpells, Skinning, Stealth, Swordmanship, SpiritSpeaking, Tactics, Tailoring, Wrestling.

### 4.3 Categorias

A definir (adiado): combate, coleta, fabricacao, magia, sobrevivencia, social, conhecimento.

---

## 5. Propriedades

### 5.1 PropertyInstance

```csharp
public sealed class PropertyInstance
{
    public float Points { get; }
    public float GetValue(); // Points / 1 (divisor 1)
}
```

- Comeca com 0 pontos.
- Normalmente sobem apenas por bonus/equipamentos.

### 5.2 Lista de propriedades

Critical, Resiliense, Hit, PowerAttack, Evasion, PowerDefense, SpiritPoints, Life, ValiantPoints, Protection, Fortitude, AttackDamage, PowerDamage, AttackSpeed, CastingSpeed, CriticalDamage, ArmorPenetration, Reaction, Counter, Threat.

Funcoes de PowerAttack, PowerDefense, SpiritPoints, ValiantPoints, Reaction, Life: adiadas.

### 5.3 Papeis confirmados

| Propriedade | Papel |
|-------------|-------|
| Critical | Chance de critico |
| Resiliense | Reduz chance de receber critico |
| Hit | Chance de acertar |
| Evasion | Chance de evitar ataques |
| Protection | Reduz dano |
| Fortitude | Reduz dano (resistencia fisica) |
| AttackDamage | Aumenta dano |
| AttackSpeed | Acelera cooldown |
| CriticalDamage | Multiplicador de dano critico |
| ArmorPenetration | Reduz protecao efetiva do alvo |
| Counter | Chance de contra-ataque |
| Threat | Aggro (ser alvo) |

---

## 6. Equipamento

```csharp
public sealed class Equipment
{
    public WeaponTemplate MainHand { get; set; }
    public WeaponTemplate Offhand { get; set; }
    public ArmorTemplate Chest { get; set; }
    // Head, Neck, Wrist, Hand, FingerLeft, FingerRight, Waist, Feet, Extra: adiados
}
```

### 6.1 Armas

```csharp
public enum WeaponSize { Light, Medium, Heavy }
public enum WeaponDamageCategory { Smashing, Piercing, Cutting, Catalyst, None, Ranged, Thrown }

public sealed class WeaponTemplate
{
    public string Name;
    public int Level;
    public WeaponSize Size;
    public WeaponDamageCategory DamageCategory;
    public float AttackSpeed;
    public float BaseDamage;
    public float SizeMultiplier;
    public float HitModifier;
    public float Penetration;
    public float CounterPotential;
}
```

Mapeamento arma -> pericia + atributo:

| Categoria | Pericia acerto | Atributo acerto | Atributo dano |
|-----------|---------------|-----------------|---------------|
| Cutting | Swordmanship | Strength | Strength |
| Piercing | Fencing | Agility | Agility |
| Ranged | Archery | Agility | Agility |
| Thrown | Archery | Agility | Strength |
| Catalyst | Magery | Intuition | Wisdom |
| None | Wrestling | Vitality | Vitality |
| Smashing | Heavyweaponship | Vitality | Strength |

### 6.2 Armaduras

```csharp
public enum ArmorCategory { Light, Medium, Heavy }

public sealed class ArmorTemplate
{
    public string Name;
    public int Level;
    public ArmorCategory Category;
    public float Protection;
    public float Evasion;
}
```

| Categoria | Protecao | Evasao |
|-----------|----------|--------|
| Light | Menor | Maior |
| Medium | Intermediaria | Intermediaria |
| Heavy | Maior | Menor |

---

## 7. Progressao por uso

Atributos e pericias podem ganhar pontos ao serem usados contra um desafio relevante.

```
chanceGanho = f(pontosAtuais, dificuldade)
```

- Chance maior quando pontos sao baixos (mais facil aprender).
- Chance menor quando pontos sao altos (mais dificil evoluir).
- `TryGain(challengeLevel)` e chamado apos cada roll contra desafio.

Formula exata: adiada.

---

## 8. Morte e fantasma

1. Heroi morre -> `Creature.Life = 0`.
2. Party morta -> retorna ao safe spot.
3. Heroi vira fantasma:
   - nao pode agir no mapa.
   - nao pode coletar.
   - nao pode entrar em combate.
   - precisa ir ate uma igreja (City) para reviver.
4. Corpo fica no local da morte por tempo limitado:
   - outra party pode ir ate o local para recuperar itens.
   - itens nao recuperados a tempo: definicao adiada.
5. Ao reviver na igreja, heroi recupera vida e volta ao normal.
