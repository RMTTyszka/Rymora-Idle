# Conteudo - Rymora Land of Heroes

Data de criacao: 2026-05-23
Regras de negocio: `docs/regras/biblia-rpg.md` (secao Itens, Armas, Armaduras, Materiais, Monstros, Encontros, Qualidade)
Arquitetura geral: `docs/arquitetura/visao-geral.md`

---

## 1. Proposito

O dominio **Content** cobre catalogos de itens, armas, armaduras, monstros, encontros, terrenos e a conversao de assets Godot para dados consumiveis pelo Core.

Content nao possui:
- loop de gameplay
- regras de UI
- controle de cena

---

## 2. Catalogos

O Core define interfaces para acesso a dados de conteudo. Adaptadores Godot implementam essas interfaces carregando assets do Godot.

```csharp
public interface IWeaponCatalog
{
    WeaponTemplate GetWeapon(string name);
    IReadOnlyList<WeaponTemplate> GetAllWeapons();
    WeaponTemplate GetRandomWeapon();
}

public interface IArmorCatalog
{
    ArmorTemplate GetArmor(string name);
    IReadOnlyList<ArmorTemplate> GetAllArmors();
    ArmorTemplate GetRandomArmor();
}

public interface IMonsterCatalog
{
    CreatureTemplate GetMonster(string name);
    IReadOnlyList<CreatureTemplate> GetMonstersByEncounter(string encounterId);
}

public interface IEncounterCatalog
{
    EncounterTemplate GetEncounter(string id);
    IReadOnlyList<EncounterTemplate> GetEncountersByRegion(string regionName);
}
```

---

## 3. Templates

### 3.1 WeaponTemplate

Repetido de heroi.md para centralizacao:

```csharp
public sealed class WeaponTemplate
{
    public string Name;
    public int Level;
    public WeaponSize Size; // Light, Medium, Heavy
    public WeaponDamageCategory DamageCategory;
    public float AttackSpeed;
    public float BaseDamage;
    public float SizeMultiplier;
    public float HitModifier;
    public float Penetration;
    public float CounterPotential;
}
```

### 3.2 ArmorTemplate

```csharp
public sealed class ArmorTemplate
{
    public string Name;
    public int Level;
    public ArmorCategory Category; // Light, Medium, Heavy
    public float Protection;
    public float Evasion;
}
```

### 3.3 CreatureTemplate

```csharp
public enum MonsterClass { Combatant, Caster, Agile }

public sealed class CreatureTemplate
{
    public string CreatureName;
    public MonsterClass Class;
    public SpriteReference Sprite;
}
```

### 3.4 EncounterTemplate

```csharp
public sealed class EncounterTemplate
{
    public string Id;
    public string Name;
    public IReadOnlyList<CreatureTemplate> Monsters;
}
```

### 3.5 Material

```csharp
public sealed class MaterialItem
{
    public string Name;
    public int Level;
    public float Weight;
}
```

Metais: Iron (1), Bronze (2), Copper (3), Silver (4), Gold (5), Mythril (6)
Madeiras: Oak (1)

### 3.6 Item (generico)

```csharp
public sealed class Item
{
    public string Name;
    public int Level;
    public float Weight;
    public int Quantity; // para agrupamento
}
```

---

## 4. Conversao de assets

Adaptadores Godot implementam os catalogos carregando dados de arquivos de recurso Godot (`.tres`, `.json`, ou cenas).

Exemplo:

```csharp
public sealed class GodotWeaponCatalog : IWeaponCatalog
{
    public WeaponTemplate GetWeapon(string name)
    {
        var resource = ResourceLoader.Load<WeaponResource>($"res://assets/data/weapons/{name}.tres");
        return new WeaponTemplate
        {
            Name = resource.Name,
            Level = resource.Level,
            Size = resource.Size,
            // ...
        };
    }
}
```

---

## 5. Conteudo inicial (v1)

Baseado no legado:

| Tipo | Itens |
|------|-------|
| Armas | Spear, None-Light, None-Medium, None-Heavy |
| Armaduras | Leather |
| Monstros | Goblin Warrior |
| Encontros | MontainEncounter, PlaainEncounter |
| Metais | Iron, Bronze, Copper, Silver, Gold, Mythril |
| Madeiras | Oak |
