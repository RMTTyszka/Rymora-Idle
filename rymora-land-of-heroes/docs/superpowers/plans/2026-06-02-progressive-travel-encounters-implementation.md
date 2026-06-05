# Progressive Travel Encounters Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar encontros de viagem com chance base baixa, modificadores de terreno/regiao/zona e bonus linear por tempo de viagem, com reset apos encontro e ao entrar em safe spot/cidade.

**Architecture:** O Core continua tile-based. `GameApplication` acumula tempo de viagem por party e continua fazendo o check por tile em `ExecuteTravel`, enquanto `WorldState` segue como autoridade para combinar modificadores do tile e fazer o roll final. O estado de acumulacao entra no save/load da party e os novos parametros de rampa saem de `game_config.json` sem criar dependencia Godot no Core.

**Tech Stack:** Godot 4.6, C#/.NET, xUnit

---

## File Map

- Create: `src/Core/Party/PartyEncounterState.cs` - estado runtime da acumulacao de risco de encontro por party.
- Modify: `src/Core/Party/Party.cs` - expor `EncounterState` na party.
- Modify: `src/Core/Configuration/GameConfig.cs` - adicionar `EncounterRampPerSecond` e `EncounterRampMaxBonus`.
- Modify: `src/Core/Application/GameApplication.cs` - acumular tempo de viagem, resetar em encontro/safe spot/cidade e passar chance base enriquecida para `WorldState`.
- Modify: `src/Core/Data/SaveData.cs` - persistir `TravelSecondsSinceEncounterReset` e subir `SaveVersion`.
- Modify: `src/Core/Data/SaveSnapshotBuilder.cs` - salvar acumulacao por party.
- Modify: `src/Core/Data/SaveRestorer.cs` - restaurar acumulacao por party.
- Modify: `src/Core/Data/SaveValidation.cs` - rejeitar acumulacao negativa.
- Modify: `src/Core/World/WorldState.cs` - manter regra do tile e aceitar chance base ja acrescida do bonus temporal.
- Modify: `src/Godot/Content/JsonGameContentLoader.cs` - ler novos campos de config.
- Modify: `assets/data/game_config.json` - definir chance base baixa e parametros iniciais da rampa.
- Modify: `src/Tests/ObjectMothers/TestObjectMother.cs` - suportar config de rampa em testes.
- Modify: `src/Tests/ObjectMothers/ApplicationObjectMother.cs` - cenarios de travel mais flexiveis para testes de rampa/reset.
- Create: `src/Tests/Party/PartyEncounterStateTests.cs` - testes unitarios do novo estado.
- Modify: `src/Tests/Application/GameApplicationTests.cs` - testes de acumulacao, encontro por rampa e reset.
- Modify: `src/Tests/World/WorldStateTests.cs` - manter cobertura de bloqueio/combinao de chance em tile.
- Modify: `src/Tests/Data/SaveSnapshotBuilderTests.cs` - garantir que save inclui acumulacao.
- Modify: `src/Tests/Data/SaveRestorerTests.cs` - garantir que load preserva acumulacao.
- Modify: `src/Tests/Data/SaveValidationTests.cs` - rejeitar acumulacao negativa.
- Modify: `docs/proximos-passos.md` - registrar regra final do slice.
- Modify: `docs/arquitetura/dados.md` - documentar novo campo persistido e novos parametros de config.
- Modify: `docs/arquitetura/mundo.md` - documentar chance progressiva por tempo com check por tile.
- Modify: `docs/arquitetura/aplicacao.md` - documentar acumulacao/reset no fluxo de viagem.

### Task 1: Add party encounter state in the Core

**Files:**
- Create: `src/Core/Party/PartyEncounterState.cs`
- Modify: `src/Core/Party/Party.cs`
- Test: `src/Tests/Party/PartyEncounterStateTests.cs`

- [ ] **Step 1: Write the failing unit tests for the new party encounter state**

```csharp
namespace RymoraLandOfHeroes.Core.Tests.Party;

public sealed class PartyEncounterStateTests
{
    [Fact]
    public void AddTravelTime_accumulates_time()
    {
        var state = new PartyEncounterState();

        state.AddTravelTime(1.5f);
        state.AddTravelTime(0.5f);

        Assert.Equal(2f, state.TravelSecondsSinceReset);
    }

    [Fact]
    public void Reset_clears_accumulated_time()
    {
        var state = new PartyEncounterState();
        state.AddTravelTime(2f);

        state.Reset();

        Assert.Equal(0f, state.TravelSecondsSinceReset);
    }

    [Fact]
    public void Restore_sets_accumulated_time()
    {
        var state = new PartyEncounterState();

        state.Restore(3.25f);

        Assert.Equal(3.25f, state.TravelSecondsSinceReset);
    }
}
```

- [ ] **Step 2: Run the new test file and verify it fails**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter "FullyQualifiedName~PartyEncounterStateTests"`

Expected: FAIL with missing `PartyEncounterState` type and missing `Restore` method.

- [ ] **Step 3: Implement the minimal encounter state and expose it on Party**

```csharp
namespace RymoraLandOfHeroes.Core.Party;

public sealed class PartyEncounterState
{
    public float TravelSecondsSinceReset { get; private set; }

    public void AddTravelTime(float deltaTime)
    {
        if (!float.IsFinite(deltaTime) || deltaTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "Travel time must be finite and non-negative.");
        }

        TravelSecondsSinceReset += deltaTime;
    }

    public void Reset()
    {
        TravelSecondsSinceReset = 0;
    }

    public void Restore(float travelSecondsSinceReset)
    {
        if (!float.IsFinite(travelSecondsSinceReset) || travelSecondsSinceReset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(travelSecondsSinceReset), "Encounter travel time must be finite and non-negative.");
        }

        TravelSecondsSinceReset = travelSecondsSinceReset;
    }
}
```

```csharp
public sealed class Party
{
    public PartyEncounterState EncounterState { get; } = new();
}
```

- [ ] **Step 4: Re-run the targeted tests and verify they pass**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter "FullyQualifiedName~PartyEncounterStateTests"`

Expected: PASS with 3 passing tests.

- [ ] **Step 5: Commit checkpoint if commits are enabled for this execution**

```bash
git add src/Core/Party/PartyEncounterState.cs src/Core/Party/Party.cs src/Tests/Party/PartyEncounterStateTests.cs
git commit -m "feat: add party encounter state"
```

### Task 2: Persist encounter accumulation in save/load

**Files:**
- Modify: `src/Core/Data/SaveData.cs`
- Modify: `src/Core/Data/SaveSnapshotBuilder.cs`
- Modify: `src/Core/Data/SaveRestorer.cs`
- Modify: `src/Core/Data/SaveValidation.cs`
- Test: `src/Tests/Data/SaveSnapshotBuilderTests.cs`
- Test: `src/Tests/Data/SaveRestorerTests.cs`
- Test: `src/Tests/Data/SaveValidationTests.cs`

- [ ] **Step 1: Write the failing save/load tests**

```csharp
[Fact]
public void CreateSaveData_saves_party_encounter_travel_seconds()
{
    var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued();
    scenario.AssertParty.EncounterState.AddTravelTime(2.5f);

    var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

    Assert.Equal(2.5f, save.Parties.Single().TravelSecondsSinceEncounterReset);
}

[Fact]
public void Restore_round_trips_party_encounter_travel_seconds()
{
    var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued();
    scenario.AssertParty.EncounterState.AddTravelTime(1.75f);
    var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

    var restored = SaveRestorer.Restore(
        save,
        scenario.InputApplication.World,
        scenario.InputApplication.Config,
        TestObjectMother.CreateMonster,
        new SequenceRandomSource(1));

    Assert.Equal(1.75f, restored.Parties.Get("party-1").EncounterState.TravelSecondsSinceReset);
}

[Fact]
public void Validate_rejects_negative_party_encounter_travel_seconds()
{
    var save = EmptySave() with
    {
        Parties = new[] { EmptyParty() with { TravelSecondsSinceEncounterReset = -1 } }
    };

    var error = Assert.Throws<InvalidOperationException>(() => SaveValidation.Validate(save));

    Assert.Contains("TravelSecondsSinceEncounterReset", error.Message);
}
```

- [ ] **Step 2: Run the targeted save tests and verify they fail**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter "FullyQualifiedName~SaveSnapshotBuilderTests|FullyQualifiedName~SaveRestorerTests|FullyQualifiedName~SaveValidationTests"`

Expected: FAIL because `PartySaveData` does not expose `TravelSecondsSinceEncounterReset` and save code does not round-trip the value.

- [ ] **Step 3: Extend the save schema and wire snapshot/restore/validation**

```csharp
// SaveData.cs
public const string CurrentVersion = "2";

public sealed record PartySaveData(
    string PartyId,
    TilePositionSaveData Position,
    bool IsInCombat,
    IReadOnlyList<CreatureSaveData> Members,
    IReadOnlyList<ItemSaveData> InventoryItems,
    ActionQueueSaveData ActionQueue,
    AutomationSaveData Automation,
    float TravelSecondsSinceEncounterReset = 0);
```

```csharp
private static PartySaveData CreateParty(GameParty party)
{
    return new PartySaveData(
        party.Id,
        CreatePosition(party.Position),
        party.IsInCombat,
        party.Members.Select(CreateCreature).ToArray(),
        party.Inventory.Items.Select(CreateItem).ToArray(),
        CreateQueue(party.ActionQueue),
        CreateAutomation(party.Automation),
        party.EncounterState.TravelSecondsSinceReset);
}
```

```csharp
// SaveRestorer.RestoreParty(...) - insert before return party;
party.EncounterState.Restore(save.TravelSecondsSinceEncounterReset);
```

```csharp
if (!float.IsFinite(party.TravelSecondsSinceEncounterReset) || party.TravelSecondsSinceEncounterReset < 0)
{
    throw new InvalidOperationException($"Party {party.PartyId} TravelSecondsSinceEncounterReset must be finite and non-negative.");
}
```

- [ ] **Step 4: Re-run the targeted save tests and verify they pass**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter "FullyQualifiedName~SaveSnapshotBuilderTests|FullyQualifiedName~SaveRestorerTests|FullyQualifiedName~SaveValidationTests"`

Expected: PASS, including the three new tests plus the existing save tests.

- [ ] **Step 5: Commit checkpoint if commits are enabled for this execution**

```bash
git add src/Core/Data/SaveData.cs src/Core/Data/SaveSnapshotBuilder.cs src/Core/Data/SaveRestorer.cs src/Core/Data/SaveValidation.cs src/Tests/Data/SaveSnapshotBuilderTests.cs src/Tests/Data/SaveRestorerTests.cs src/Tests/Data/SaveValidationTests.cs
git commit -m "feat: persist encounter accumulation"
```

### Task 3: Implement progressive encounter chance during travel

**Files:**
- Modify: `src/Core/Configuration/GameConfig.cs`
- Modify: `src/Godot/Content/JsonGameContentLoader.cs`
- Modify: `assets/data/game_config.json`
- Modify: `src/Tests/ObjectMothers/TestObjectMother.cs`
- Modify: `src/Tests/ObjectMothers/ApplicationObjectMother.cs`
- Modify: `src/Core/Application/GameApplication.cs`
- Modify: `src/Core/World/WorldState.cs`
- Test: `src/Tests/Application/GameApplicationTests.cs`
- Test: `src/Tests/World/WorldStateTests.cs`

- [ ] **Step 1: Write the failing travel-encounter tests first**

```csharp
[Fact]
public void Update_travel_accumulates_encounter_time_when_no_encounter_happens()
{
    var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued(
        encounterProbability: 5,
        encounterRampPerSecond: 10,
        encounterRampMaxBonus: 40,
        worldRandomRolls: new[] { 90, 90 },
        combatRandomRolls: new[] { 1, 1 },
        destination: new TilePosition(2, 0));

    scenario.InputApplication.Update(1);

    Assert.Equal(1f, scenario.AssertParty.EncounterState.TravelSecondsSinceReset);
    Assert.False(scenario.AssertParty.IsInCombat);
}

[Fact]
public void Update_travel_starts_combat_after_ramp_increases_chance()
{
    var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued(
        encounterProbability: 5,
        encounterRampPerSecond: 10,
        encounterRampMaxBonus: 40,
        worldRandomRolls: new[] { 16, 20, 0 },
        combatRandomRolls: new[] { 1, 1 },
        destination: new TilePosition(2, 0));

    scenario.InputApplication.Update(1);
    scenario.InputApplication.Update(1);

    Assert.True(scenario.AssertParty.IsInCombat);
    Assert.Equal(0f, scenario.AssertParty.EncounterState.TravelSecondsSinceReset);
}

[Fact]
public void Update_travel_resets_encounter_time_on_safe_spot()
{
    var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued(
        encounterProbability: 0,
        encounterRampPerSecond: 0,
        encounterRampMaxBonus: 0,
        worldRandomRolls: new[] { 100, 100, 100 },
        combatRandomRolls: new[] { 1, 1 },
        destination: new TilePosition(3, 0));
    scenario.AssertParty.EncounterState.AddTravelTime(4f);

    scenario.InputApplication.Update(1);
    scenario.InputApplication.Update(1);
    scenario.InputApplication.Update(1);

    Assert.Equal(new TilePosition(3, 0), scenario.AssertParty.Position);
    Assert.Equal(0f, scenario.AssertParty.EncounterState.TravelSecondsSinceReset);
}

[Fact]
public void ShouldTriggerEncounter_clamps_large_probability_after_modifiers()
{
    var scenario = WorldObjectMother.WorldWithEncounterModifier();

    var result = scenario.InputWorld.ShouldTriggerEncounter(
        scenario.InputPosition,
        baseProbability: 150,
        scenario.InputPolicy);

    Assert.True(result);
}
```

- [ ] **Step 2: Run the travel/world tests and verify they fail**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter "FullyQualifiedName~GameApplicationTests|FullyQualifiedName~WorldStateTests"`

Expected: FAIL because `GameConfig` lacks ramp fields, `ApplicationObjectMother` lacks new parameters, and `GameApplication` does not accumulate/reset encounter time.

- [ ] **Step 3: Implement the config fields, object mothers, and travel logic**

```csharp
public sealed record GameConfig(
    float EncounterProbability,
    float EncounterInterval,
    EncounterPolicy EncounterPolicy,
    float EncounterRampPerSecond,
    float EncounterRampMaxBonus,
    float CorpseDecayTime,
    ProgressionConfig Progression,
    LifeConfig Life,
    CollectionConfig Collection,
    TravelConfig Travel,
    CombatConfig Combat,
    SaveConfig Save);
```

```csharp
private static GameConfig LoadConfig(string path)
{
    var config = ReadJson<GameConfigDto>(path);
    return new GameConfig(
        config.EncounterProbability,
        config.EncounterInterval,
        new EncounterPolicy(ParseEnum<EncounterModifierMode>(config.EncounterPolicy.TerrainModifierMode)),
        config.EncounterRampPerSecond,
        config.EncounterRampMaxBonus,
        config.CorpseDecayTime,
        new ProgressionConfig(
            config.Progression.InitialAttributePoints,
            config.Progression.InitialSkillPoints,
            config.Progression.AttributeValueDivisor,
            config.Progression.SkillValueDivisor),
        new LifeConfig(config.Life.BaseLife, config.Life.VitalityLifeMultiplier),
        new CollectionConfig(
            config.Collection.DifficultyBase,
            config.Collection.DifficultyPerMaterialLevel,
            config.Collection.MiningActionTime,
            config.Collection.WoodcuttingActionTime),
        new TravelConfig(config.Travel.ActionTime),
        new CombatConfig(
            new RollRange(config.Combat.HitRollRange.Min, config.Combat.HitRollRange.Max),
            new RollRange(config.Combat.EvadeRollRange.Min, config.Combat.EvadeRollRange.Max),
            config.Combat.BaseCriticalMultiplier,
            new TargetingConfig(config.Combat.Targeting.LowLifeWeight, config.Combat.Targeting.ThreatWeight)),
        new SaveConfig(config.Save.AutoSaveIntervalSeconds));
}

private sealed class GameConfigDto
{
    public float EncounterProbability { get; set; }
    public float EncounterInterval { get; set; }
    public float EncounterRampPerSecond { get; set; }
    public float EncounterRampMaxBonus { get; set; }
    public EncounterPolicyDto EncounterPolicy { get; set; } = new();
    public float CorpseDecayTime { get; set; }
    public ProgressionConfigDto Progression { get; set; } = new();
    public LifeConfigDto Life { get; set; } = new();
    public CollectionConfigDto Collection { get; set; } = new();
    public TravelConfigDto Travel { get; set; } = new();
    public CombatConfigDto Combat { get; set; } = new();
    public SaveConfigDto Save { get; set; } = new();
}
```

```json
{
  "encounterProbability": 4,
  "encounterInterval": 1,
  "encounterRampPerSecond": 3,
  "encounterRampMaxBonus": 24,
  "encounterPolicy": {
    "terrainModifierMode": "None"
  }
}
```

```csharp
// GameApplication.RunPartyActions(...) - insert before current.AddProgress(deltaTime);
if (current.Request.ActionType == PartyActionType.Travel && !party.IsInCombat)
{
    party.EncounterState.AddTravelTime(deltaTime);
}

private void ExecuteTravel(GameParty party, PartyActionState state)
{
    var path = state.Request.Path ?? Array.Empty<TilePosition>();
    if (state.ExecutedCount >= path.Count)
    {
        var completed = party.ActionQueue.CompleteCurrentIfFinished();
        NotifyAutomationActionCompleted(party, completed);
        return;
    }

    party.Position = path[state.ExecutedCount];
    state.MarkExecuted();
    var travelCompleted = party.ActionQueue.CompleteCurrentIfFinished();
    NotifyAutomationActionCompleted(party, travelCompleted);

    if (IsSafeEncounterTile(party.Position))
    {
        party.EncounterState.Reset();
        return;
    }

    if (World.ShouldTriggerEncounter(party.Position, GetEncounterBaseChance(party), Config.EncounterPolicy))
    {
        party.EncounterState.Reset();
        StartCombat(party.Id, World.SelectRandomEncounter(party.Position));
    }
}

private bool IsSafeEncounterTile(TilePosition position)
{
    var terrain = World.GetTerrain(position);
    return terrain.IsCity || World.GetRegion(position).IsSafeSpot;
}

private float GetEncounterBaseChance(GameParty party)
{
    var temporalBonus = Math.Min(
        party.EncounterState.TravelSecondsSinceReset * Config.EncounterRampPerSecond,
        Config.EncounterRampMaxBonus);
    return Config.EncounterProbability + temporalBonus;
}
```

```csharp
public static GameConfig CreateGameConfig(
    float encounterProbability = 0,
    float encounterRampPerSecond = 0,
    float encounterRampMaxBonus = 0)
{
    return new GameConfig(
        EncounterProbability: encounterProbability,
        EncounterInterval: 1,
        EncounterPolicy: new EncounterPolicy(EncounterModifierMode.None),
        EncounterRampPerSecond: encounterRampPerSecond,
        EncounterRampMaxBonus: encounterRampMaxBonus,
        CorpseDecayTime: 60,
        Progression: new ProgressionConfig(5, 5, 5, 5),
        Life: new LifeConfig(100, 10),
        Collection: new CollectionConfig(
            DifficultyBase: 0,
            DifficultyPerMaterialLevel: 0,
            MiningActionTime: 1,
            WoodcuttingActionTime: 1),
        Travel: new TravelConfig(ActionTime: 1),
        Combat: new CombatConfig(
            HitRollRange: new RollRange(1, 1),
            EvadeRollRange: new RollRange(1, 1),
            BaseCriticalMultiplier: 1.5f,
            Targeting: new TargetingConfig(LowLifeWeight: 100, ThreatWeight: 1)),
        Save: new SaveConfig(AutoSaveIntervalSeconds: 30));
}
```

```csharp
public static TravelUpdateScenario ApplicationWithTravelActionQueued(
    float encounterProbability = 0,
    float encounterRampPerSecond = 0,
    float encounterRampMaxBonus = 0,
    int[]? worldRandomRolls = null,
    int[]? combatRandomRolls = null,
    TilePosition? destination = null)
{
    var party = new GameParty("party-1", new TilePosition(0, 0));
    var hero = TestObjectMother.CreateCreature("Hero");
    hero.Equipment.MainHand = TestObjectMother.CreateWeapon("Greatsword", attackSpeed: 1, baseDamage: 100);
    party.AddMember(hero);

    var application = new GameApplication(
        TestObjectMother.CreateWorld(random: new SequenceRandomSource(worldRandomRolls ?? new[] { 1, 0 })),
        new[] { party },
        TestObjectMother.CreateGameConfig(encounterProbability, encounterRampPerSecond, encounterRampMaxBonus),
        _ => TestObjectMother.CreateCreature("Goblin", new LifeConfig(1, 0)),
        new SequenceRandomSource(combatRandomRolls ?? new[] { 1, 1 }));

    application.SelectParty("party-1");
    application.EnqueueAction("party-1", new PartyActionRequest(
        PartyActionType.Travel,
        PartyActionEndType.ByCount,
        TimeToExecute: 1,
        Destination: destination ?? new TilePosition(1, 0)));

    return new TravelUpdateScenario(
        InputApplication: application,
        InputDeltaTime: 1,
        AssertParty: party,
        ExpectedPosition: destination ?? new TilePosition(1, 0),
        ExpectedIsInCombat: encounterProbability > 0);
}
```

- [ ] **Step 4: Re-run the targeted travel/world tests and verify they pass**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter "FullyQualifiedName~GameApplicationTests|FullyQualifiedName~WorldStateTests"`

Expected: PASS, including the new accumulation/ramp/reset tests.

- [ ] **Step 5: Commit checkpoint if commits are enabled for this execution**

```bash
git add src/Core/Configuration/GameConfig.cs src/Godot/Content/JsonGameContentLoader.cs assets/data/game_config.json src/Tests/ObjectMothers/TestObjectMother.cs src/Tests/ObjectMothers/ApplicationObjectMother.cs src/Core/Application/GameApplication.cs src/Core/World/WorldState.cs src/Tests/Application/GameApplicationTests.cs src/Tests/World/WorldStateTests.cs
git commit -m "feat: add progressive travel encounters"
```

### Task 4: Update docs and run full verification

**Files:**
- Modify: `docs/proximos-passos.md`
- Modify: `docs/arquitetura/dados.md`
- Modify: `docs/arquitetura/mundo.md`
- Modify: `docs/arquitetura/aplicacao.md`

- [ ] **Step 1: Update the docs to match the implemented behavior**

```md
- `docs/proximos-passos.md`: trocar a nota de encontro generico por regra fechada de chance base baixa + rampa temporal linear com teto + reset em encontro/safe spot/cidade.
- `docs/arquitetura/dados.md`: adicionar `EncounterRampPerSecond`, `EncounterRampMaxBonus` e `TravelSecondsSinceEncounterReset` no save/config.
- `docs/arquitetura/mundo.md`: registrar que o roll continua por tile, mas a chance efetiva inclui bonus por tempo de viagem.
- `docs/arquitetura/aplicacao.md`: documentar que `GameApplication` acumula tempo de viagem e reseta risco em encontro e entrada em area segura.
```

- [ ] **Step 2: Run the full automated verification**

Run: `dotnet build RymoraLandOfHeroes.sln`

Expected: `Build succeeded.`

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore`

Expected: all tests PASS.

Run: `git diff --check`

Expected: no output.

- [ ] **Step 3: Inspect the final diff before handing back**

Run: `git diff -- docs/proximos-passos.md docs/arquitetura/dados.md docs/arquitetura/mundo.md docs/arquitetura/aplicacao.md src/Core/Party/PartyEncounterState.cs src/Core/Party/Party.cs src/Core/Configuration/GameConfig.cs src/Core/Application/GameApplication.cs src/Core/World/WorldState.cs src/Core/Data/SaveData.cs src/Core/Data/SaveSnapshotBuilder.cs src/Core/Data/SaveRestorer.cs src/Core/Data/SaveValidation.cs src/Godot/Content/JsonGameContentLoader.cs assets/data/game_config.json src/Tests/Party/PartyEncounterStateTests.cs src/Tests/Application/GameApplicationTests.cs src/Tests/World/WorldStateTests.cs src/Tests/Data/SaveSnapshotBuilderTests.cs src/Tests/Data/SaveRestorerTests.cs src/Tests/Data/SaveValidationTests.cs src/Tests/ObjectMothers/TestObjectMother.cs src/Tests/ObjectMothers/ApplicationObjectMother.cs`

Expected: only intended encounter-ramp changes.

- [ ] **Step 4: Commit checkpoint if commits are enabled for this execution**

```bash
git add docs/proximos-passos.md docs/arquitetura/dados.md docs/arquitetura/mundo.md docs/arquitetura/aplicacao.md
git commit -m "docs: document progressive travel encounters"
```

## Self-Review Checklist

- Spec coverage: party runtime state, config fields, tile-based check, time ramp, reset rules, save/load, docs, and tests each have explicit tasks above.
- Placeholder scan: no TBD/TODO markers remain; every task has concrete files, snippets, and commands.
- Type consistency: plan consistently uses `EncounterRampPerSecond`, `EncounterRampMaxBonus`, and `TravelSecondsSinceEncounterReset`.
