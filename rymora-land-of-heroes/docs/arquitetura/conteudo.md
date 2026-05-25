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
    public int Level;
    public IReadOnlyList<CreatureTemplate> Monsters;
}
```

`Level` do encontro vem de `assets/data/content/encounters.json`. Regioes apontam para encontros por id agrupados por terreno em `assets/data/world/regions.json`. O nivel de jogo do tile vem de `assets/data/world/zones.json`.

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

Arquivos de conteudo usados no prototipo Godot:
- `assets/data/content/weapons.json`
- `assets/data/content/materials.json`
- `assets/data/content/creatures.json`
- `assets/data/content/encounters.json`
- `assets/data/world/regions.json`
- `assets/data/world/zones.json`
- `assets/data/world/terrain_tiles.json`

`regions.json` define tiles de regiao, safe spot, modificador de chance de encontro e encontros por bioma. `zones.json` define tiles de zona, nivel e modificador de chance. `terrain_tiles.json` define tiles/biomas, regras de terreno e materiais coletaveis.

### 5.1 Validacao de conteudo

`JsonGameContentLoader` valida as referencias cruzadas antes de montar os catalogos runtime. O bootstrap deve falhar cedo com erro claro quando JSON estiver inconsistente.

Validacoes atuais:
- arma referenciada por creature existe em `weapons.json`.
- creature referenciada por encounter existe em `creatures.json`.
- encounter referenciado por region existe em `encounters.json`.
- material referenciado por terrain tile existe em `materials.json`.
- atlas coords de terrain, region e zone nao duplicam.
