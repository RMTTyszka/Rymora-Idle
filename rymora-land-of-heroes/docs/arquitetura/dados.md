# Dados - Rymora Land of Heroes

Data de criacao: 2026-05-23
Regras de negocio: `docs/regras/biblia-rpg.md`
Arquitetura geral: `docs/arquitetura/visao-geral.md`

---

## 1. Proposito

O dominio **Data** cobre save/load, configuracao do jogo e progressao persistente.

---

## 2. Save

### 2.1 Formato

Serializacao em JSON. 1 arquivo de save por jogador na v1.

Implementacao atual:

- Core define DTOs versionados em `src/Core/Data/SaveData.cs`.
- Core cria snapshot com `SaveSnapshotBuilder` e restaura com `SaveRestorer`.
- Godot persiste JSON via `src/Godot/Data/JsonSaveStore.cs`.
- Caminho default: `user://saves/save-1.json`.
- Save invalido aborta load com erro claro e nao sobrescreve arquivo existente.
- O Core nao le arquivo e nao conhece JSON.

### 2.2 Conteudo do save

```csharp
public sealed record SaveData(
    string SaveVersion,
    DateTimeOffset SavedAtUtc,
    float PlayTimeSeconds,
    string? SelectedPartyId,
    string CurrentScreen,
    IReadOnlyList<PartySaveData> Parties,
    IReadOnlyList<CombatSaveData> ActiveCombats);
```

```csharp
public sealed record PartySaveData(
    string PartyId,
    TilePositionSaveData Position,
    bool IsInCombat,
    IReadOnlyList<CreatureSaveData> Members,
    IReadOnlyList<ItemSaveData> InventoryItems,
    ActionQueueSaveData ActionQueue,
    AutomationSaveData Automation);
```

`TilePosition` serializa coordenadas logicas do Core. Save nao grava tipos Godot como `Vector2I`.

O save atual cobre parties, membros, vida atual/maxima, stats, equipamento, inventario, fila atual, progresso parcial, acoes pendentes, Macros, Program, Runner e combates ativos. RNG interno, multiplos slots e cloud save ficam fora da v1.

### 2.3 Quando salvar

- Automatico: a cada N segundos (configuravel).
- Manual: adiado.
- Ao fechar o jogo.

---

## 3. Config

### 3.1 Game config (global, nao salva por jogador)

```csharp
public sealed record GameConfig
{
    public float EncounterProbability; // chance base de encontro
    public float EncounterInterval; // tempo entre checks de encontro
    public EncounterPolicy EncounterPolicy; // aplica modificador de terreno sem hardcode no World
    public float CorpseDecayTime; // tempo ate corpo desaparecer
    public ProgressionConfig Progression;
    public LifeConfig Life;
    public CollectionConfig Collection;
    public TravelConfig Travel;
    public CombatConfig Combat;
    public SaveConfig Save;
}

public sealed record ProgressionConfig
{
    public float InitialAttributePoints;
    public float InitialSkillPoints;
    public float AttributeValueDivisor;
    public float SkillValueDivisor;
}

public sealed record LifeConfig
{
    public float BaseLife;
    public float VitalityLifeMultiplier;
}

public sealed record EncounterPolicy
{
    public EncounterModifierMode TerrainModifierMode; // decisao pendente: valor default
}

public enum EncounterModifierMode
{
    None,
    AddToProbability,
    SubtractFromProbability
}

public sealed record CollectionConfig
{
    public float DifficultyBase;
    public float DifficultyPerMaterialLevel;
    public float MiningActionTime;
    public float WoodcuttingActionTime;
}

public sealed record TravelConfig
{
    public float ActionTime;
}

public sealed record SaveConfig
{
    public float AutoSaveIntervalSeconds;
}

public sealed record CombatConfig
{
    public RollRange HitRollRange;
    public RollRange EvadeRollRange;
    public float BaseCriticalMultiplier;
    public TargetingConfig Targeting;
}

public sealed record RollRange
{
    public int Min;
    public int Max;
}

public sealed record TargetingConfig
{
    public float LowLifeWeight;
    public float ThreatWeight;
}
```

Carregado de `assets/data/game_config.json` pelo adapter Godot.

Valores numericos pendentes na biblia entram em `GameConfig` como balanceamento provisorio, nao como regra fixa no Core.

### 3.2 Content runtime

Conteudo inicial carregado por JSON:

- `assets/data/content/weapons.json`
- `assets/data/content/materials.json`
- `assets/data/content/creatures.json`
- `assets/data/content/encounters.json`
- `assets/data/world/regions.json`
- `assets/data/world/zones.json`
- `assets/data/world/terrain_tiles.json`

O adapter Godot monta catalogos runtime (`IWeaponCatalog`, `IMaterialCatalog`, `IEncounterCatalog`) e injeta factories no `GameApplication`. O Core recebe templates e config ja parseados; nao le arquivo nem referencia Godot.

Antes de montar os catalogos runtime, o adapter valida referencias cruzadas do conteudo JSON para falhar cedo: weapons usadas por creatures, creatures usadas por encounters, encounters usados por regions, materiais usados por terrain tiles e atlas coords duplicadas em terrain/region/zone.

`regions.json` define tiles de regiao, safe spot, modificador de chance de encontro e encontros por bioma. `zones.json` define tiles de zona, nivel e modificador de chance. `terrain_tiles.json` mapeia atlas coords para `TerrainData`, bioma visual, cor provisoria do tile e material coletavel por acao.

Chance de encontro durante viagem deve ser balanceavel por dados:
- chance global em `assets/data/game_config.json` (`encounterProbability`).
- modificador por terreno/bioma em `assets/data/world/terrain_tiles.json` (`encounterRateModifier`).
- modificador por regiao em `assets/data/world/regions.json` (`encounterProbabilityModifier`).
- modificador por zona em `assets/data/world/zones.json` (`encounterProbabilityModifier`).

### 3.3 Player settings (preferencias)

- Idioma (futuro).
- Volume.
- Resolucao (futuro).
