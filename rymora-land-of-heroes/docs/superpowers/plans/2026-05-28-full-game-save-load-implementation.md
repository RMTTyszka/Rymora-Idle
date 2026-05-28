# Full Game Save/Load Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add full save/load for current playable Core state: parties, heroes, inventory, action queue progress, automation, active combat, selected party, screen, and play time.

**Architecture:** Core owns pure save DTOs plus snapshot/restore logic under `src/Core/Data`; Godot owns file IO under `src/Godot/Data`. JSON serialization never enters Core. Restore validates first, then builds a fresh `GameApplication` so invalid saves cannot leave half-applied state.

**Tech Stack:** Godot 4.6 C#, .NET 8, xUnit, `System.Text.Json`, existing Core/Application/Party/Automation/Combat domains.

---

## Communication Contract

All agents, subagents, and review agents must use caveman ultra style. Subagent prompt prefix:

```text
/caveman ultra
Use terse caveman ultra. Full technical accuracy. No fluff. Follow this plan exactly. TDD: failing test first.
```

No git commit unless user explicitly asks. If a task says checkpoint, run `git status --short` and report changed files only.

## Scope Check

Spec is one feature with sequential dependencies. Do not split into separate specs. Build in layers: DTO/validation, runtime restore hooks, snapshot, restore, app facade, Godot file store, bootstrap, docs.

## File Structure

Create:

- `src/Core/Data/SaveData.cs`: versioned DTO records only.
- `src/Core/Data/SaveValidation.cs`: pure validation, throws clear `InvalidOperationException`.
- `src/Core/Data/SaveSnapshotBuilder.cs`: `GameApplication` -> `SaveData`.
- `src/Core/Data/SaveRestorer.cs`: `SaveData` -> new `GameApplication`.
- `src/Godot/Data/JsonSaveStore.cs`: JSON file read/write with temp-file replace.
- `src/Tests/Data/SaveValidationTests.cs`: validation tests.
- `src/Tests/Data/SaveSnapshotBuilderTests.cs`: snapshot tests.
- `src/Tests/Data/SaveRestorerTests.cs`: restore tests.
- `src/Tests/Godot/Data/JsonSaveStoreTests.cs`: JSON round-trip tests using temp folder.

Modify:

- `src/Core/Application/GameApplication.cs`: `PlayTimeSeconds`, save facade, restore combat hook.
- `src/Core/Party/PartyAction.cs`: expose pending requests and restore current/pending queue.
- `src/Core/Hero/StatBlock.cs`: restore stat blocks from saved points/divisors.
- `src/Core/Hero/Creature.cs`: restore exact `Life` and `MaxLife`.
- `src/Core/Automation/MacroRecordingSession.cs`: restore active recording and next action number.
- `src/Core/Automation/PartyProgram.cs`: restore program repeat, steps, next step number.
- `src/Core/Automation/PartyAutomation.cs`: restore macros, recording, program, runner.
- `src/Core/Automation/ProgramRunner.cs`: export/restore full runner state.
- `src/Core/Combat/WeaponCooldown.cs`: restore current cooldown.
- `src/Core/Combat/Combatant.cs`: restore combatants with saved cooldowns.
- `src/Core/Combat/CombatInstance.cs`: restore active combat from saved combatants.
- `src/Core/Configuration/GameConfig.cs`: add `SaveConfig` with autosave interval.
- `src/Godot/Content/JsonGameContentLoader.cs`: parse `save.autoSaveIntervalSeconds`.
- `assets/data/game_config.json`: add `save` section.
- `src/Godot/Bootstrap/Bootstrap.cs`: load on start, autosave, save on exit.
- `src/Godot/Bootstrap/BootstrapCoreFactory.cs`: expose game-new creation separate from save restore.
- `docs/arquitetura/dados.md`: document actual save format/flow.
- `docs/arquitetura/aplicacao.md`: document bootstrap load/save.
- `docs/arquitetura/party.md`: document queue/automation persistence.
- `docs/arquitetura/combate.md`: document combat persistence.
- `docs/proximos-passos.md`: mark full save/load implemented and list limits.

---

### Task 1: Save DTOs And Validation

**Files:**
- Create: `src/Core/Data/SaveData.cs`
- Create: `src/Core/Data/SaveValidation.cs`
- Test: `src/Tests/Data/SaveValidationTests.cs`

- [ ] **Step 1: Write failing validation tests**

Create `src/Tests/Data/SaveValidationTests.cs`:

```csharp
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Data;

namespace RymoraLandOfHeroes.Core.Tests.Data;

public sealed class SaveValidationTests
{
    [Fact]
    public void Validate_rejects_unknown_save_version()
    {
        var save = EmptySave() with { SaveVersion = "999" };

        var error = Assert.Throws<InvalidOperationException>(() => SaveValidation.Validate(save));

        Assert.Contains("Unsupported save version", error.Message);
    }

    [Fact]
    public void Validate_rejects_missing_selected_party()
    {
        var save = EmptySave() with { SelectedPartyId = "missing" };

        var error = Assert.Throws<InvalidOperationException>(() => SaveValidation.Validate(save));

        Assert.Contains("SelectedPartyId", error.Message);
    }

    [Fact]
    public void Validate_rejects_incomplete_travel_request()
    {
        var request = new PartyActionRequestSaveData(
            ActionType: "Travel",
            EndType: "ByCount",
            TimeToExecute: 1,
            LimitCount: 1,
            EndTime: null,
            ItemName: null,
            ItemLevel: null,
            ItemWeight: null,
            Quantity: null,
            TargetPartyId: null,
            Destination: null,
            Path: Array.Empty<TilePositionSaveData>(),
            AutomationActionId: null);
        var party = EmptyParty() with
        {
            ActionQueue = new ActionQueueSaveData(
                Current: new PartyActionStateSaveData(request, 0, 0, 0, Started: true),
                Pending: Array.Empty<PartyActionRequestSaveData>())
        };
        var save = EmptySave() with { Parties = new[] { party } };

        var error = Assert.Throws<InvalidOperationException>(() => SaveValidation.Validate(save));

        Assert.Contains("Destination", error.Message);
    }

    private static SaveData EmptySave()
    {
        return new SaveData(
            SaveVersion: SaveData.CurrentVersion,
            SavedAtUtc: DateTimeOffset.UnixEpoch,
            PlayTimeSeconds: 0,
            SelectedPartyId: "party-1",
            CurrentScreen: nameof(Screen.Map),
            Parties: new[] { EmptyParty() },
            ActiveCombats: Array.Empty<CombatSaveData>());
    }

    private static PartySaveData EmptyParty()
    {
        return new PartySaveData(
            PartyId: "party-1",
            Position: new TilePositionSaveData(0, 0),
            IsInCombat: false,
            Members: Array.Empty<CreatureSaveData>(),
            InventoryItems: Array.Empty<ItemSaveData>(),
            ActionQueue: new ActionQueueSaveData(null, Array.Empty<PartyActionRequestSaveData>()),
            Automation: AutomationSaveData.Empty);
    }
}
```

- [ ] **Step 2: Run tests and verify RED**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~SaveValidationTests`

Expected: FAIL compile errors for missing `RymoraLandOfHeroes.Core.Data` and save DTO types.

- [ ] **Step 3: Add DTOs**

Create `src/Core/Data/SaveData.cs` with public records using primitive/string enum names for stable JSON:

```csharp
using RymoraLandOfHeroes.Core.Content;

namespace RymoraLandOfHeroes.Core.Data;

public sealed record SaveData(
    string SaveVersion,
    DateTimeOffset SavedAtUtc,
    float PlayTimeSeconds,
    string? SelectedPartyId,
    string CurrentScreen,
    IReadOnlyList<PartySaveData> Parties,
    IReadOnlyList<CombatSaveData> ActiveCombats)
{
    public const string CurrentVersion = "1";
}

public sealed record TilePositionSaveData(int X, int Y);

public sealed record PartySaveData(
    string PartyId,
    TilePositionSaveData Position,
    bool IsInCombat,
    IReadOnlyList<CreatureSaveData> Members,
    IReadOnlyList<ItemSaveData> InventoryItems,
    ActionQueueSaveData ActionQueue,
    AutomationSaveData Automation);

public sealed record CreatureSaveData(
    string Name,
    float Life,
    float MaxLife,
    string SpriteId,
    IReadOnlyDictionary<string, StatSaveData> Attributes,
    IReadOnlyDictionary<string, StatSaveData> Skills,
    IReadOnlyDictionary<string, StatSaveData> Properties,
    EquipmentSaveData Equipment);

public sealed record StatSaveData(float Points, float ValueDivisor);

public sealed record EquipmentSaveData(
    WeaponTemplate? MainHand,
    WeaponTemplate? Offhand,
    ArmorTemplate? Chest)
{
    public static EquipmentSaveData Empty { get; } = new(null, null, null);
}

public sealed record ItemSaveData(string Name, int Level, float Weight, int Quantity);

public sealed record ActionQueueSaveData(
    PartyActionStateSaveData? Current,
    IReadOnlyList<PartyActionRequestSaveData> Pending);

public sealed record PartyActionStateSaveData(
    PartyActionRequestSaveData Request,
    float CurrentTime,
    float PassedTime,
    int ExecutedCount,
    bool Started);

public sealed record PartyActionRequestSaveData(
    string ActionType,
    string EndType,
    float TimeToExecute,
    int? LimitCount,
    float? EndTime,
    string? ItemName,
    int? ItemLevel,
    float? ItemWeight,
    int? Quantity,
    string? TargetPartyId,
    TilePositionSaveData? Destination,
    IReadOnlyList<TilePositionSaveData> Path,
    string? AutomationActionId);

public sealed record AutomationSaveData(
    MacroRecordingSaveData? Recording,
    IReadOnlyList<PartyMacroSaveData> Macros,
    PartyProgramSaveData Program,
    ProgramRunnerSaveData Runner)
{
    public static AutomationSaveData Empty { get; } = new(
        Recording: null,
        Macros: Array.Empty<PartyMacroSaveData>(),
        Program: new PartyProgramSaveData("Once", 1, null, 0, Array.Empty<ProgramStepSaveData>()),
        Runner: ProgramRunnerSaveData.Idle);
}

public sealed record MacroRecordingSaveData(
    string Id,
    int NextActionNumber,
    IReadOnlyList<MacroActionSaveData> Actions);

public sealed record PartyMacroSaveData(
    string Id,
    string Name,
    IReadOnlyList<MacroActionSaveData> Actions);

public sealed record MacroActionSaveData(
    string Id,
    string Kind,
    TilePositionSaveData? Destination,
    string? ItemName,
    int? ItemLevel,
    float? ItemWeight,
    RepeatPolicySaveData? Repeat);

public sealed record RepeatPolicySaveData(string Mode, int? RepeatCount, float? Seconds);

public sealed record PartyProgramSaveData(
    string RepeatMode,
    int? RepeatCount,
    float? RepeatSeconds,
    int NextStepNumber,
    IReadOnlyList<ProgramStepSaveData> Steps);

public sealed record ProgramStepSaveData(
    string Id,
    string MacroId,
    RepeatPolicySaveData Repeat);

public sealed record ProgramRunnerSaveData(
    string State,
    string? ErrorMessage,
    IReadOnlyList<MacroActionSaveData> CurrentMacroActions,
    MacroActionSaveData? CurrentAction,
    int ProgramStepIndex,
    int MacroActionIndex,
    int StepIteration,
    int ProgramIteration,
    int ActionIteration,
    float ProgramElapsedSeconds,
    float StepElapsedSeconds,
    float ActionElapsedSeconds,
    string? CurrentExecutionId)
{
    public static ProgramRunnerSaveData Idle { get; } = new(
        State: "Idle",
        ErrorMessage: null,
        CurrentMacroActions: Array.Empty<MacroActionSaveData>(),
        CurrentAction: null,
        ProgramStepIndex: 0,
        MacroActionIndex: 0,
        StepIteration: 0,
        ProgramIteration: 0,
        ActionIteration: 0,
        ProgramElapsedSeconds: 0,
        StepElapsedSeconds: 0,
        ActionElapsedSeconds: 0,
        CurrentExecutionId: null);
}

public sealed record CombatSaveData(
    string PartyId,
    string State,
    IReadOnlyList<HeroCombatantSaveData> Heroes,
    IReadOnlyList<MonsterCombatantSaveData> Monsters);

public sealed record HeroCombatantSaveData(
    int MemberIndex,
    IReadOnlyList<WeaponCooldownSaveData> Cooldowns);

public sealed record MonsterCombatantSaveData(
    CreatureSaveData Creature,
    IReadOnlyList<WeaponCooldownSaveData> Cooldowns);

public sealed record WeaponCooldownSaveData(
    string Slot,
    WeaponTemplate Weapon,
    float CurrentCooldown,
    float TotalCooldown);
```

- [ ] **Step 4: Add validation**

Create `src/Core/Data/SaveValidation.cs`:

```csharp
namespace RymoraLandOfHeroes.Core.Data;

public static class SaveValidation
{
    public static void Validate(SaveData save)
    {
        if (save.SaveVersion != SaveData.CurrentVersion)
        {
            throw new InvalidOperationException($"Unsupported save version: {save.SaveVersion}.");
        }

        var partyIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var party in save.Parties)
        {
            if (!partyIds.Add(party.PartyId))
            {
                throw new InvalidOperationException($"Duplicate PartyId in save: {party.PartyId}.");
            }

            ValidateQueue(party.PartyId, party.ActionQueue);
            ValidateAutomation(party.PartyId, party.Automation);
            ValidateInventory(party.PartyId, party.InventoryItems);
        }

        if (save.SelectedPartyId is not null && !partyIds.Contains(save.SelectedPartyId))
        {
            throw new InvalidOperationException($"SelectedPartyId not found in save: {save.SelectedPartyId}.");
        }

        foreach (var combat in save.ActiveCombats)
        {
            if (!partyIds.Contains(combat.PartyId))
            {
                throw new InvalidOperationException($"ActiveCombat.PartyId not found in save: {combat.PartyId}.");
            }
        }
    }

    private static void ValidateQueue(string partyId, ActionQueueSaveData queue)
    {
        if (queue.Current is not null)
        {
            ValidateRequest(partyId, queue.Current.Request);
        }

        foreach (var pending in queue.Pending)
        {
            ValidateRequest(partyId, pending);
        }
    }

    private static void ValidateRequest(string partyId, PartyActionRequestSaveData request)
    {
        if (request.TimeToExecute < 0)
        {
            throw new InvalidOperationException($"Party {partyId} action TimeToExecute cannot be negative.");
        }

        if (request.ActionType == "Travel" && request.Destination is null)
        {
            throw new InvalidOperationException($"Party {partyId} Travel action missing Destination.");
        }

        if ((request.ActionType == "Mine" || request.ActionType == "CutWood")
            && (string.IsNullOrWhiteSpace(request.ItemName) || request.ItemLevel is null || request.ItemWeight is null))
        {
            throw new InvalidOperationException($"Party {partyId} collection action missing item fields.");
        }

        if (request.ActionType == "TransferItem"
            && (string.IsNullOrWhiteSpace(request.TargetPartyId) || string.IsNullOrWhiteSpace(request.ItemName) || request.ItemLevel is null || request.Quantity is null))
        {
            throw new InvalidOperationException($"Party {partyId} TransferItem action missing transfer fields.");
        }
    }

    private static void ValidateAutomation(string partyId, AutomationSaveData automation)
    {
        var macroIds = new HashSet<string>(automation.Macros.Select(macro => macro.Id), StringComparer.Ordinal);
        foreach (var step in automation.Program.Steps)
        {
            if (!macroIds.Contains(step.MacroId))
            {
                throw new InvalidOperationException($"Party {partyId} Program step references missing Macro: {step.MacroId}.");
            }
        }
    }

    private static void ValidateInventory(string partyId, IReadOnlyList<ItemSaveData> items)
    {
        foreach (var item in items)
        {
            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException($"Party {partyId} inventory item has invalid quantity: {item.Name}.");
            }
        }
    }
}
```

- [ ] **Step 5: Run validation tests and verify GREEN**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~SaveValidationTests`

Expected: PASS.

- [ ] **Step 6: Checkpoint**

Run: `git status --short`

Expected: new `src/Core/Data/*`, new `src/Tests/Data/SaveValidationTests.cs`. No commit.

---

### Task 2: Runtime Restore Hooks

**Files:**
- Modify: `src/Core/Party/PartyAction.cs`
- Modify: `src/Core/Hero/StatBlock.cs`
- Modify: `src/Core/Hero/Creature.cs`
- Modify: `src/Core/Automation/MacroRecordingSession.cs`
- Modify: `src/Core/Automation/PartyProgram.cs`
- Modify: `src/Core/Automation/PartyAutomation.cs`
- Modify: `src/Core/Automation/ProgramRunner.cs`
- Modify: `src/Core/Combat/WeaponCooldown.cs`
- Modify: `src/Core/Combat/Combatant.cs`
- Modify: `src/Core/Combat/CombatInstance.cs`
- Test: `src/Tests/Data/RuntimeRestoreHookTests.cs`

- [ ] **Step 1: Write failing runtime hook tests**

Create `src/Tests/Data/RuntimeRestoreHookTests.cs`:

```csharp
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;
using RymoraLandOfHeroes.Core.Hero;
using RymoraLandOfHeroes.Core.Party;
using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Data;

public sealed class RuntimeRestoreHookTests
{
    [Fact]
    public void PartyActionQueue_restore_preserves_current_progress_and_pending_order()
    {
        var currentRequest = new PartyActionRequest(PartyActionType.Mine, PartyActionEndType.ByCount, 5, LimitCount: 2, ItemName: "Iron", ItemLevel: 1, ItemWeight: 3);
        var current = PartyActionState.Restore(currentRequest, currentTime: 2, passedTime: 7, executedCount: 1, started: true);
        var pending = new[]
        {
            new PartyActionRequest(PartyActionType.CutWood, PartyActionEndType.ByCount, 3, LimitCount: 1, ItemName: "Oak", ItemLevel: 1, ItemWeight: 1)
        };

        var queue = new PartyActionQueue();
        queue.Restore(current, pending);

        Assert.Equal(2, queue.Current!.CurrentTime);
        Assert.Equal(7, queue.Current.PassedTime);
        Assert.Equal(1, queue.Current.ExecutedCount);
        Assert.Equal(PartyActionType.CutWood, queue.PendingRequests.Single().ActionType);
    }

    [Fact]
    public void Creature_restore_preserves_life_and_max_life()
    {
        var creature = Creature.Restore(
            name: "Hero",
            attributes: StatBlock<AttributeType>.Create(5, 5),
            skills: StatBlock<SkillType>.Create(5, 5),
            properties: StatBlock<PropertyType>.Create(0, 1),
            equipment: new Equipment(),
            sprite: default,
            life: 12,
            maxLife: 34);

        Assert.Equal(12, creature.Life);
        Assert.Equal(34, creature.MaxLife);
    }

    [Fact]
    public void ProgramRunner_restore_preserves_error_state()
    {
        var runner = new ProgramRunner();
        var state = new ProgramRunnerRuntimeState(
            State: ProgramRunnerState.Error,
            ErrorMessage: "bad macro",
            CurrentMacroActions: Array.Empty<MacroAction>(),
            CurrentAction: null,
            ProgramStepIndex: 0,
            MacroActionIndex: 0,
            StepIteration: 0,
            ProgramIteration: 0,
            ActionIteration: 0,
            ProgramElapsedSeconds: 0,
            StepElapsedSeconds: 0,
            ActionElapsedSeconds: 0,
            CurrentExecutionId: null);

        runner.Restore(state);

        Assert.Equal(ProgramRunnerState.Error, runner.State);
        Assert.Equal("bad macro", runner.ErrorMessage);
    }
}
```

- [ ] **Step 2: Run tests and verify RED**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~RuntimeRestoreHookTests`

Expected: FAIL compile errors for missing restore APIs.

- [ ] **Step 3: Add queue restore APIs**

In `src/Core/Party/PartyAction.cs`:

```csharp
public static PartyActionState Restore(
    PartyActionRequest request,
    float currentTime,
    float passedTime,
    int executedCount,
    bool started)
{
    if (currentTime < 0 || passedTime < 0 || executedCount < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(currentTime), "Action state values cannot be negative.");
    }

    return new PartyActionState(request)
    {
        CurrentTime = currentTime,
        PassedTime = passedTime,
        ExecutedCount = executedCount,
        Started = started
    };
}
```

Change `CurrentTime`, `PassedTime`, `ExecutedCount`, `Started` setters from `private set` to `private protected set` is not valid for sealed class use. Use private constructor or make setters `private set` and place `Restore` inside `PartyActionState` so object initializer can set private setters.

Add to `PartyActionQueue`:

```csharp
public IReadOnlyList<PartyActionRequest> PendingRequests => _pending.ToArray();

public void Restore(PartyActionState? current, IEnumerable<PartyActionRequest> pending)
{
    _pending.Clear();
    Current = current;
    foreach (var request in pending)
    {
        _pending.Enqueue(request);
    }
}
```

- [ ] **Step 4: Add creature/stat restore APIs**

In `src/Core/Hero/StatBlock.cs`, add:

```csharp
public static StatBlock<TStat> FromInstances(IReadOnlyDictionary<TStat, StatInstance> stats)
{
    var restored = Enum.GetValues<TStat>()
        .ToDictionary(stat => stat, stat => stats[stat]);
    return new StatBlock<TStat>(restored);
}
```

In `src/Core/Hero/Creature.cs`, add static restore factory:

```csharp
public static Creature Restore(
    string name,
    StatBlock<AttributeType> attributes,
    StatBlock<SkillType> skills,
    StatBlock<PropertyType> properties,
    Equipment equipment,
    SpriteReference sprite,
    float life,
    float maxLife)
{
    if (maxLife < 0 || life < 0 || life > maxLife)
    {
        throw new ArgumentOutOfRangeException(nameof(life), "Saved life must be between zero and max life.");
    }

    return new Creature(name, attributes, skills, properties, equipment, new LifeConfig(maxLife, 0), sprite, life, maxLife);
}
```

Add private constructor overload to support exact values:

```csharp
private Creature(
    string name,
    StatBlock<AttributeType> attributes,
    StatBlock<SkillType> skills,
    StatBlock<PropertyType> properties,
    Equipment equipment,
    LifeConfig lifeConfig,
    SpriteReference sprite,
    float? life,
    float? maxLife)
```

Make public constructor call private overload with `life: null, maxLife: null`.

- [ ] **Step 5: Add automation restore APIs**

In `MacroRecordingSession`, add public `NextActionNumber` and restore constructor:

```csharp
public int NextActionNumber => _nextActionNumber;

public MacroRecordingSession(string id, IEnumerable<MacroAction> actions, int nextActionNumber)
    : this(id)
{
    if (nextActionNumber < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(nextActionNumber), "Next action number cannot be negative.");
    }

    _actions.AddRange(actions);
    _nextActionNumber = nextActionNumber;
}
```

In `PartyProgram`, add `NextStepNumber` and restore method:

```csharp
public int NextStepNumber => _nextStepNumber;

public void Restore(RepeatPolicy repeat, IEnumerable<ProgramStep> steps, int nextStepNumber)
{
    if (nextStepNumber < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(nextStepNumber), "Next step number cannot be negative.");
    }

    _steps.Clear();
    _steps.AddRange(steps);
    Repeat = repeat;
    _nextStepNumber = nextStepNumber;
}
```

In `PartyAutomation`, add:

```csharp
public void Restore(
    IEnumerable<PartyMacro> macros,
    MacroRecordingSession? recording,
    RepeatPolicy programRepeat,
    IEnumerable<ProgramStep> programSteps,
    int nextProgramStepNumber,
    ProgramRunnerRuntimeState runnerState)
{
    _macros.Clear();
    foreach (var macro in macros)
    {
        AddMacro(macro);
    }

    Recording = recording;
    Program.Restore(programRepeat, programSteps, nextProgramStepNumber);
    Runner.Restore(runnerState);
}
```

In `ProgramRunner`, add public runtime state record and restore/export:

```csharp
public sealed record ProgramRunnerRuntimeState(
    ProgramRunnerState State,
    string? ErrorMessage,
    IReadOnlyList<MacroAction> CurrentMacroActions,
    MacroAction? CurrentAction,
    int ProgramStepIndex,
    int MacroActionIndex,
    int StepIteration,
    int ProgramIteration,
    int ActionIteration,
    float ProgramElapsedSeconds,
    float StepElapsedSeconds,
    float ActionElapsedSeconds,
    string? CurrentExecutionId);
```

Add `CaptureState()` and `Restore(ProgramRunnerRuntimeState state)` that copies every private field exactly.

- [ ] **Step 6: Add combat restore APIs**

In `WeaponCooldown`, add factory:

```csharp
public static WeaponCooldown Restore(WeaponTemplate weapon, float currentCooldown, float totalCooldown)
{
    if (currentCooldown < 0 || totalCooldown < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(currentCooldown), "Cooldown values cannot be negative.");
    }

    var cooldown = new WeaponCooldown(weapon, attackSpeedBonus: 0, totalCooldownOverride: totalCooldown);
    cooldown.CurrentCooldown = currentCooldown;
    return cooldown;
}
```

Use a private constructor overload with `float? totalCooldownOverride` so existing constructor behavior stays unchanged.

In `Combatant`, add restore constructor:

```csharp
public static Combatant Restore(Creature creature, WeaponCooldown mainHandCooldown, WeaponCooldown? offhandCooldown)
{
    return new Combatant(creature, mainHandCooldown, offhandCooldown);
}
```

In `CombatInstance`, add restore factory:

```csharp
public static CombatInstance Restore(
    IEnumerable<Combatant> heroes,
    IEnumerable<Combatant> monsters,
    CombatConfig config,
    IRandomSource? randomSource = null)
{
    return new CombatInstance(heroes, monsters, config, randomSource);
}
```

Add private constructor taking `IEnumerable<Combatant>` directly. `State` remains derived by `UpdateState()` from creature life.

- [ ] **Step 7: Run hook tests and verify GREEN**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~RuntimeRestoreHookTests`

Expected: PASS.

---

### Task 3: Snapshot Builder For Party, Queue, Automation, Combat

**Files:**
- Create: `src/Core/Data/SaveSnapshotBuilder.cs`
- Modify: `src/Core/Application/GameApplication.cs`
- Test: `src/Tests/Data/SaveSnapshotBuilderTests.cs`

- [ ] **Step 1: Write failing snapshot tests**

Create `src/Tests/Data/SaveSnapshotBuilderTests.cs`:

```csharp
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Data;
using RymoraLandOfHeroes.Core.Party;
using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Data;

public sealed class SaveSnapshotBuilderTests
{
    [Fact]
    public void CreateSaveData_saves_party_position_inventory_and_member_life()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMineActionQueued();
        scenario.InputApplication.SelectParty("party-1");
        scenario.InputApplication.Update(1);
        var hero = scenario.AssertParty.Leader!;
        hero.TakeDamage(10);

        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var party = Assert.Single(save.Parties);
        Assert.Equal(new TilePositionSaveData(0, 0), party.Position);
        Assert.Equal(1, party.InventoryItems.Single().Quantity);
        Assert.Equal(hero.Life, party.Members.Single().Life);
        Assert.Equal("party-1", save.SelectedPartyId);
    }

    [Fact]
    public void CreateSaveData_saves_current_queue_progress_and_pending_actions()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMineActionQueued();
        scenario.InputApplication.EnqueueAction("party-1", new PartyActionRequest(
            PartyActionType.CutWood,
            PartyActionEndType.ByCount,
            TimeToExecute: 3,
            LimitCount: 1,
            ItemName: "Oak",
            ItemLevel: 1,
            ItemWeight: 1));
        scenario.InputApplication.Update(0.5f);

        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var queue = save.Parties.Single().ActionQueue;
        Assert.Equal(0.5f, queue.Current!.CurrentTime);
        Assert.Equal("CutWood", queue.Pending.Single().ActionType);
    }

    [Fact]
    public void CreateSaveData_saves_automation_macros_program_and_runner_error()
    {
        var scenario = ApplicationObjectMother.ApplicationWithInvalidMiningProgram();
        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(1);

        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var automation = save.Parties.Single().Automation;
        Assert.Equal("macro-1", automation.Macros.Single().Id);
        Assert.Equal("macro-1", automation.Program.Steps.Single().MacroId);
        Assert.Equal("Error", automation.Runner.State);
        Assert.Contains("cannot mine", automation.Runner.ErrorMessage);
    }

    [Fact]
    public void CreateSaveData_saves_active_combat()
    {
        var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued(encounterProbability: 100);
        scenario.InputApplication.Update(1);

        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var combat = Assert.Single(save.ActiveCombats);
        Assert.Equal("party-1", combat.PartyId);
        Assert.NotEmpty(combat.Heroes);
        Assert.NotEmpty(combat.Monsters);
    }
}
```

- [ ] **Step 2: Run tests and verify RED**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~SaveSnapshotBuilderTests`

Expected: FAIL compile error for missing `CreateSaveData` and `SaveSnapshotBuilder`.

- [ ] **Step 3: Add GameApplication facade**

In `GameApplication.cs`, add:

```csharp
public float PlayTimeSeconds { get; private set; }

public SaveData CreateSaveData(DateTimeOffset savedAtUtc)
{
    return SaveSnapshotBuilder.Create(this, savedAtUtc);
}
```

Increment `PlayTimeSeconds += deltaTime;` in `Update` after negative delta validation.

- [ ] **Step 4: Implement SaveSnapshotBuilder converters**

Create `src/Core/Data/SaveSnapshotBuilder.cs` with methods:

```csharp
public static class SaveSnapshotBuilder
{
    public static SaveData Create(GameApplication application, DateTimeOffset savedAtUtc)
}
```

Required conversion rules:

- Enum values saved with `.ToString()`.
- `TilePosition` -> `TilePositionSaveData`.
- `Inventory.Items` -> `ItemSaveData[]`.
- `PartyActionQueue.Current` and `PendingRequests` -> `ActionQueueSaveData`.
- `RepeatPolicy` -> `RepeatPolicySaveData`.
- `MacroAction` maps by runtime type: `MoveToMacroAction` saves `Destination`; `GatherMacroAction` saves item fields and repeat.
- `ProgramRunner.CaptureState()` -> `ProgramRunnerSaveData`.
- `CombatInstance` maps hero combatants by member index in the owning party.
- Monster combatants save full creature state.

Use private helpers in same file. Do not add Godot refs.

- [ ] **Step 5: Run snapshot tests and verify GREEN**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~SaveSnapshotBuilderTests`

Expected: PASS.

---

### Task 4: Save Restorer For Parties, Queue, Automation, Combat

**Files:**
- Create: `src/Core/Data/SaveRestorer.cs`
- Modify: `src/Core/Application/GameApplication.cs`
- Test: `src/Tests/Data/SaveRestorerTests.cs`

- [ ] **Step 1: Write failing restore tests**

Create `src/Tests/Data/SaveRestorerTests.cs`:

```csharp
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Data;
using RymoraLandOfHeroes.Core.Party;
using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Data;

public sealed class SaveRestorerTests
{
    [Fact]
    public void Restore_round_trips_party_inventory_queue_and_selected_party()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMineActionQueued();
        scenario.InputApplication.SelectParty("party-1");
        scenario.InputApplication.Update(0.5f);
        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var restored = SaveRestorer.Restore(
            save,
            scenario.InputApplication.World,
            scenario.InputApplication.Config,
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        var party = restored.Parties.Get("party-1");
        Assert.Equal("party-1", restored.UI.SelectedPartyId);
        Assert.Equal(0.5f, party.ActionQueue.Current!.CurrentTime);
    }

    [Fact]
    public void Restore_round_trips_automation_runner_error()
    {
        var scenario = ApplicationObjectMother.ApplicationWithInvalidMiningProgram();
        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(1);
        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var restored = SaveRestorer.Restore(
            save,
            scenario.InputApplication.World,
            scenario.InputApplication.Config,
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        var runner = restored.Parties.Get("party-1").Automation.Runner;
        Assert.Equal(ProgramRunnerState.Error, runner.State);
        Assert.Contains("cannot mine", runner.ErrorMessage);
    }

    [Fact]
    public void Restore_round_trips_active_combat()
    {
        var scenario = ApplicationObjectMother.ApplicationWithTravelActionQueued(encounterProbability: 100);
        scenario.InputApplication.Update(1);
        var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

        var restored = SaveRestorer.Restore(
            save,
            scenario.InputApplication.World,
            scenario.InputApplication.Config,
            TestObjectMother.CreateMonster,
            new SequenceRandomSource(1));

        Assert.True(restored.Parties.Get("party-1").IsInCombat);
        Assert.True(restored.ActiveCombats.ContainsKey("party-1"));
    }
}
```

- [ ] **Step 2: Run tests and verify RED**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~SaveRestorerTests`

Expected: FAIL compile errors for missing `SaveRestorer`.

- [ ] **Step 3: Add GameApplication restore hook**

In `GameApplication.cs`, add constructor optional playtime:

```csharp
float playTimeSeconds = 0
```

Set `PlayTimeSeconds = playTimeSeconds;` after validation that it is finite and non-negative.

Add method:

```csharp
public void RestoreActiveCombat(string partyId, CombatInstance combat)
{
    var party = Parties.Get(partyId);
    _combats[partyId] = combat;
    party.IsInCombat = true;
    SyncScreen();
}
```

- [ ] **Step 4: Implement SaveRestorer**

Create `src/Core/Data/SaveRestorer.cs`:

```csharp
public static class SaveRestorer
{
    public static GameApplication Restore(
        SaveData save,
        WorldState world,
        GameConfig config,
        Func<CreatureTemplate, Creature> monsterFactory,
        IRandomSource? randomSource = null)
}
```

Required conversion rules:

- Call `SaveValidation.Validate(save)` first.
- Recreate parties with `new Party.Party(partyId, position, inventory)`.
- Restore members before restoring active combats.
- Restore `Creature` via `Creature.Restore`.
- Restore stats by parsing enum names and using `StatBlock<T>.FromInstances`.
- Restore equipment by saved full templates.
- Restore queue with `PartyActionQueue.Restore`.
- Restore macros, recording, program, and runner with Task 2 APIs.
- Create `GameApplication` with restored parties and `playTimeSeconds: save.PlayTimeSeconds`.
- If `SelectedPartyId` is not null, call `SelectParty`.
- Restore active combats after application creation using `RestoreActiveCombat`.

- [ ] **Step 5: Run restore tests and verify GREEN**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~SaveRestorerTests`

Expected: PASS.

---

### Task 5: JSON Save Store

**Files:**
- Create: `src/Godot/Data/JsonSaveStore.cs`
- Test: `src/Tests/Godot/Data/JsonSaveStoreTests.cs`

- [ ] **Step 1: Write failing JSON store tests**

Create `src/Tests/Godot/Data/JsonSaveStoreTests.cs`:

```csharp
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Data;
using RymoraLandOfHeroes.GodotAdapter.Data;

namespace RymoraLandOfHeroes.Core.Tests.Godot.Data;

public sealed class JsonSaveStoreTests
{
    [Fact]
    public void Save_then_load_round_trips_save_data()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"rymora-save-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "save-1.json");
        var store = new JsonSaveStore(path);
        var save = new SaveData(
            SaveData.CurrentVersion,
            DateTimeOffset.UnixEpoch,
            12,
            "party-1",
            nameof(Screen.Map),
            new[]
            {
                new PartySaveData(
                    "party-1",
                    new TilePositionSaveData(1, 2),
                    IsInCombat: false,
                    Members: Array.Empty<CreatureSaveData>(),
                    InventoryItems: Array.Empty<ItemSaveData>(),
                    ActionQueue: new ActionQueueSaveData(null, Array.Empty<PartyActionRequestSaveData>()),
                    Automation: AutomationSaveData.Empty)
            },
            Array.Empty<CombatSaveData>());

        store.Save(save);
        var loaded = store.TryLoad();

        Assert.NotNull(loaded);
        Assert.Equal(12, loaded.PlayTimeSeconds);
        Assert.Equal("party-1", loaded.SelectedPartyId);
    }

    [Fact]
    public void TryLoad_returns_null_when_file_missing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"rymora-missing-{Guid.NewGuid():N}", "save-1.json");
        var store = new JsonSaveStore(path);

        var loaded = store.TryLoad();

        Assert.Null(loaded);
    }
}
```

- [ ] **Step 2: Run tests and verify RED**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~JsonSaveStoreTests`

Expected: FAIL compile error for missing `JsonSaveStore`.

- [ ] **Step 3: Implement JsonSaveStore**

Create `src/Godot/Data/JsonSaveStore.cs`:

```csharp
using System.Text.Json;
using RymoraLandOfHeroes.Core.Data;

namespace RymoraLandOfHeroes.GodotAdapter.Data;

public sealed class JsonSaveStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _path;

    public JsonSaveStore(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
    }

    public SaveData? TryLoad()
    {
        if (!File.Exists(_path))
        {
            return null;
        }

        var json = File.ReadAllText(_path);
        var save = JsonSerializer.Deserialize<SaveData>(json, Options)
            ?? throw new InvalidOperationException($"Save file is empty or invalid: {_path}.");
        SaveValidation.Validate(save);
        return save;
    }

    public void Save(SaveData save)
    {
        SaveValidation.Validate(save);
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{_path}.tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(save, Options));
        if (File.Exists(_path))
        {
            File.Replace(tempPath, _path, destinationBackupFileName: null);
            return;
        }

        File.Move(tempPath, _path);
    }
}
```

- [ ] **Step 4: Run JSON store tests and verify GREEN**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~JsonSaveStoreTests`

Expected: PASS.

---

### Task 6: Config And Bootstrap Integration

**Files:**
- Modify: `src/Core/Configuration/GameConfig.cs`
- Modify: `assets/data/game_config.json`
- Modify: `src/Godot/Content/JsonGameContentLoader.cs`
- Modify: `src/Godot/Bootstrap/BootstrapCoreFactory.cs`
- Modify: `src/Godot/Bootstrap/Bootstrap.cs`

- [ ] **Step 1: Write failing config/load behavior test**

Add to `src/Tests/Data/SaveRestorerTests.cs`:

```csharp
[Fact]
public void Restore_preserves_play_time()
{
    var scenario = ApplicationObjectMother.ApplicationWithMineActionQueued();
    scenario.InputApplication.Update(1.5f);
    var save = scenario.InputApplication.CreateSaveData(DateTimeOffset.UnixEpoch);

    var restored = SaveRestorer.Restore(
        save,
        scenario.InputApplication.World,
        scenario.InputApplication.Config,
        TestObjectMother.CreateMonster,
        new SequenceRandomSource(1));

    Assert.Equal(1.5f, restored.PlayTimeSeconds);
}
```

- [ ] **Step 2: Run test and verify RED**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~Restore_preserves_play_time`

Expected: FAIL until `PlayTimeSeconds` is restored by `SaveRestorer`.

- [ ] **Step 3: Add SaveConfig**

In `GameConfig.cs`, add `SaveConfig Save` to `GameConfig` record and add:

```csharp
public sealed record SaveConfig(float AutoSaveIntervalSeconds);
```

Update every `GameConfig` constructor call site, including `TestObjectMother.CreateGameConfig`, with `new SaveConfig(30)`.

In `assets/data/game_config.json`, add:

```json
  "save": {
    "autoSaveIntervalSeconds": 30
  },
```

In `JsonGameContentLoader`, add DTO property and parse it into `SaveConfig`.

- [ ] **Step 4: Integrate Bootstrap load/save**

In `Bootstrap.cs`:

- Add field `private JsonSaveStore? _saveStore;`
- Add field `private float _saveElapsed;`
- Add using `RymoraLandOfHeroes.GodotAdapter.Data;`
- After `_content = JsonGameContentLoader.LoadDefault();`, create store:

```csharp
var savePath = ProjectSettings.GlobalizePath("user://saves/save-1.json");
_saveStore = new JsonSaveStore(savePath);
```

- Replace direct application creation with:

```csharp
var save = _saveStore.TryLoad();
_application = save is null
    ? BootstrapCoreFactory.CreateApplication(world, startPosition, _content)
    : SaveRestorer.Restore(save, world, _content.Config, _content.Creatures.CreateCreature);
if (_application.UI.SelectedPartyId is null)
{
    _application.SelectParty(BootstrapCoreFactory.PartyId);
}
```

- Only enqueue startup mining action when `save is null`, so loaded state is not overwritten.
- In `_Process`, after `_application.Update`, autosave when `_saveElapsed >= _application.Config.Save.AutoSaveIntervalSeconds`.
- Add override `_Notification(int what)` and save on `NotificationWMCloseRequest`.
- Add private `SaveCurrentGame()` that calls `_saveStore.Save(_application.CreateSaveData(DateTimeOffset.UtcNow));`.
- On load failure, let exception abort bootstrap; do not create new game and overwrite save.

- [ ] **Step 5: Run targeted tests and build**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore --filter FullyQualifiedName~SaveRestorerTests`

Expected: PASS.

Run: `dotnet build RymoraLandOfHeroes.sln`

Expected: build succeeds.

---

### Task 7: Documentation Update

**Files:**
- Modify: `docs/arquitetura/dados.md`
- Modify: `docs/arquitetura/aplicacao.md`
- Modify: `docs/arquitetura/party.md`
- Modify: `docs/arquitetura/combate.md`
- Modify: `docs/proximos-passos.md`

- [ ] **Step 1: Update data architecture doc**

In `docs/arquitetura/dados.md`, update save section to state:

```markdown
Implementacao atual:

- Core define DTOs versionados em `src/Core/Data/SaveData.cs`.
- Core cria snapshot com `SaveSnapshotBuilder` e restaura com `SaveRestorer`.
- Godot persiste JSON via `src/Godot/Data/JsonSaveStore.cs`.
- Caminho default: `user://saves/save-1.json`.
- Save invalido aborta load com erro claro e nao sobrescreve arquivo existente.
```

- [ ] **Step 2: Update application doc**

In `docs/arquitetura/aplicacao.md`, update bootstrap flow:

```markdown
Bootstrap tenta carregar `user://saves/save-1.json` depois de carregar config/conteudo e criar `WorldState`. Se save existe e valida, `SaveRestorer` cria `GameApplication`. Se save nao existe, fluxo de jogo novo cria party inicial. Autosave usa `GameConfig.Save.AutoSaveIntervalSeconds` e tambem salva ao fechar.
```

- [ ] **Step 3: Update party/combat/proximos docs**

Add to `docs/arquitetura/party.md`:

```markdown
Persistencia atual:

- Position, membros, inventario, fila atual, progresso parcial, pendentes e automacao da party sao salvos.
- Automation salva recording, Macros, Program e Runner.
```

Add to `docs/arquitetura/combate.md`:

```markdown
Persistencia atual:

- Combate ativo e salvo por Party.
- Herois sao referenciados pelo indice do membro na Party.
- Monstros sao salvos completos.
- Cooldowns atuais das armas sao salvos para continuar o combate apos load.
```

Update `docs/proximos-passos.md`:

- Move save/load from pending list to current implementation.
- Keep limitations: no multiple slots, no cloud save, no RNG state serialization.

- [ ] **Step 4: Check docs diff**

Run: `git diff -- docs/arquitetura/dados.md docs/arquitetura/aplicacao.md docs/arquitetura/party.md docs/arquitetura/combate.md docs/proximos-passos.md`

Expected: docs match implemented behavior.

---

### Task 8: Full Verification

**Files:** all changed files.

- [ ] **Step 1: Run full build**

Run: `dotnet build RymoraLandOfHeroes.sln`

Expected: `Build succeeded.`

- [ ] **Step 2: Run full test suite**

Run: `dotnet test RymoraLandOfHeroes.sln --no-restore`

Expected: all tests pass.

- [ ] **Step 3: Run whitespace check**

Run: `git diff --check`

Expected: no output.

- [ ] **Step 4: Godot smoke availability**

Check if documented Godot executable exists:

```powershell
Test-Path -LiteralPath "C:\Users\rmttyszka\AppData\Local\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe"
```

Expected on current machine may be `False`. If `True`, run documented headless smoke. If `False`, record limitation in final report only.

- [ ] **Step 5: Final status**

Run: `git status --short`

Expected: only intended save/load and docs files changed. No commit unless user explicitly asks.

---

## Self-Review Notes

Spec coverage:

- Save version/data/playtime: Tasks 1, 3, 6.
- Selected party/screen: Tasks 1, 3, 4.
- Parties, position, members, inventory: Tasks 2, 3, 4.
- Queue current/pending/progress: Tasks 2, 3, 4.
- Automation recording/macros/program/runner: Tasks 2, 3, 4.
- Combat active/cooldowns: Tasks 2, 3, 4.
- Godot JSON store/load/autosave/quit save: Tasks 5, 6.
- Docs update: Task 7.
- Verification: Task 8.

Type consistency:

- Save DTO names use `*SaveData`.
- Runtime runner helper uses `ProgramRunnerRuntimeState` in Automation domain.
- JSON store is public because test project instantiates it.
- No Godot type enters Core DTOs.

Known limits preserved from spec:

- RNG internal state not serialized.
- No multiple slots.
- No manual save UI.
- No cloud save, encryption, compression, migration chain.
