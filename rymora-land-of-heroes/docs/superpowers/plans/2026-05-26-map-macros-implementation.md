# Map Macros Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build party-specific Macros and one active Program per party for map action automation.

**Architecture:** Automation rules live in `src/Core/Automation` and are owned by each `Party`. `GameApplication` is the orchestration boundary that turns Macro actions into existing `PartyActionRequest` travel/collect actions. Godot creates and edits automation through presenters and input adapters, without duplicating rule validation.

**Tech Stack:** Godot 4.6, C#/.NET 8, xUnit, existing Core/Application/Godot adapter split.

---

## File Map

Create Core files:

- `src/Core/Automation/RepeatPolicy.cs`: repeat mode and validation for once, forever, count and duration.
- `src/Core/Automation/MacroAction.cs`: `MoveTo`, `Mine`, `CutWood` Macro actions.
- `src/Core/Automation/PartyMacro.cs`: named Macro with ordered editable actions.
- `src/Core/Automation/MacroRecordingSession.cs`: in-progress Record Macro state.
- `src/Core/Automation/PartyAutomation.cs`: Macro catalog, active Program, recording session and runner owned by a party.
- `src/Core/Automation/PartyProgram.cs`: linear Program sequence and editable Program Steps.
- `src/Core/Automation/ProgramRunner.cs`: runtime state machine for Play/Pause/Stop/Error and Macro execution pointers.
- `src/Core/Automation/MacroActionExecution.cs`: action emitted by the runner for Application execution.

Modify Core files:

- `src/Core/Party/Party.cs`: add `Automation` property.
- `src/Core/Party/PartyAction.cs`: add automation action id to requests and expose completion from queue.
- `src/Core/Application/GameApplication.cs`: add automation commands and run automation before normal action execution.
- `src/Core/Application/PlayerIntent.cs`: add automation player intents routed by `HandleInput`.

Create test files:

- `src/Tests/Automation/RepeatPolicyTests.cs`
- `src/Tests/Automation/PartyMacroTests.cs`
- `src/Tests/Automation/MacroRecordingSessionTests.cs`
- `src/Tests/Automation/PartyProgramTests.cs`
- `src/Tests/Automation/ProgramRunnerTests.cs`
- `src/Tests/Application/GameApplicationAutomationTests.cs`

Modify test helpers:

- `src/Tests/ObjectMothers/PartyObjectMother.cs`: add automation scenarios.
- `src/Tests/ObjectMothers/ApplicationObjectMother.cs`: add application scenarios for program execution.

Create Godot files:

- `src/Godot/Presentation/MacrosPresenter.cs`: tab content for Macro list, Program list and Record Macro controls.
- `src/Godot/Presentation/MacroEditorPresenter.cs`: larger editor window for Macro actions.
- `src/Godot/Presentation/ProgramEditorPresenter.cs`: larger editor window for Program steps and Program repeat.

Modify Godot files:

- `src/Godot/Bootstrap/Bootstrap.cs`: route map clicks to recording when Record Macro is active, wire presenters to Application.
- `src/Godot/Presentation/HudPresenter.cs`: show status, current action, next action, Play/Pause/Stop controls and tabs.
- `scenes/bootstrap.tscn`: add UI nodes for `MacrosPresenter`, editor windows and control buttons.

Documentation updates after code:

- `docs/arquitetura/party.md`: add final Macro/Program model summary.
- `docs/arquitetura/ui.md`: replace provisional notes with implemented UI flow.
- `docs/proximos-passos.md`: mark Macro design as current implementation and keep save/load, TransferItem and Dungeon outside this slice.

---

## Implementation Sequence

### Task 1: Repeat Policy And Macro Actions

**Files:**
- Create: `src/Core/Automation/RepeatPolicy.cs`
- Create: `src/Core/Automation/MacroAction.cs`
- Test: `src/Tests/Automation/RepeatPolicyTests.cs`
- Test: `src/Tests/Automation/PartyMacroTests.cs`

- [ ] **Step 1: Write failing repeat policy tests**

Create `src/Tests/Automation/RepeatPolicyTests.cs`:

```csharp
using RymoraLandOfHeroes.Core.Automation;

namespace RymoraLandOfHeroes.Core.Tests.Automation;

public sealed class RepeatPolicyTests
{
    [Fact]
    public void Count_requires_positive_count()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => RepeatPolicy.Count(0));

        Assert.Equal("count", error.ParamName);
    }

    [Fact]
    public void Duration_requires_positive_seconds()
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => RepeatPolicy.Duration(0));

        Assert.Equal("seconds", error.ParamName);
    }

    [Fact]
    public void Once_has_expected_mode()
    {
        Assert.Equal(RepeatMode.Once, RepeatPolicy.Once.Mode);
    }
}
```

- [ ] **Step 2: Write failing Macro action tests**

Create `src/Tests/Automation/PartyMacroTests.cs`:

```csharp
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.Tests.Automation;

public sealed class PartyMacroTests
{
    [Fact]
    public void MoveTo_action_has_absolute_destination()
    {
        var action = new MoveToMacroAction("move-1", new TilePosition(4, -2));

        Assert.Equal(MacroActionKind.MoveTo, action.Kind);
        Assert.Equal(new TilePosition(4, -2), action.Destination);
    }

    [Fact]
    public void Gather_action_rejects_move_kind()
    {
        var error = Assert.Throws<ArgumentException>(() => new GatherMacroAction(
            "gather-1",
            MacroActionKind.MoveTo,
            "Iron",
            itemLevel: 1,
            itemWeight: 3,
            RepeatPolicy.Once));

        Assert.Contains("Gather action kind must be Mine or CutWood", error.Message);
    }
}
```

- [ ] **Step 3: Run tests and verify they fail**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~Automation
```

Expected: build fails because `RymoraLandOfHeroes.Core.Automation`, `RepeatPolicy`, `MoveToMacroAction` and `GatherMacroAction` do not exist.

- [ ] **Step 4: Implement repeat policy**

Create `src/Core/Automation/RepeatPolicy.cs`:

```csharp
namespace RymoraLandOfHeroes.Core.Automation;

public enum RepeatMode
{
    Once,
    Forever,
    Count,
    Duration
}

public sealed record RepeatPolicy
{
    private RepeatPolicy(RepeatMode mode, int? count, float? seconds)
    {
        Mode = mode;
        RepeatCount = count;
        Seconds = seconds;
    }

    public RepeatMode Mode { get; }
    public int? RepeatCount { get; }
    public float? Seconds { get; }

    public static RepeatPolicy Once { get; } = new(RepeatMode.Once, count: 1, seconds: null);
    public static RepeatPolicy Forever { get; } = new(RepeatMode.Forever, count: null, seconds: null);

    public static RepeatPolicy Count(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Repeat count must be positive.");
        }

        return new RepeatPolicy(RepeatMode.Count, count, seconds: null);
    }

    public static RepeatPolicy Duration(float seconds)
    {
        if (seconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), "Repeat duration must be positive.");
        }

        return new RepeatPolicy(RepeatMode.Duration, count: null, seconds);
    }
}
```

- [ ] **Step 5: Implement Macro actions**

Create `src/Core/Automation/MacroAction.cs`:

```csharp
using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.Automation;

public enum MacroActionKind
{
    MoveTo,
    Mine,
    CutWood
}

public abstract class MacroAction
{
    protected MacroAction(string id, MacroActionKind kind)
    {
        Id = RequireId(id);
        Kind = kind;
    }

    public string Id { get; }
    public MacroActionKind Kind { get; }

    protected static string RequireId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return id;
    }
}

public sealed class MoveToMacroAction : MacroAction
{
    public MoveToMacroAction(string id, TilePosition destination)
        : base(id, MacroActionKind.MoveTo)
    {
        Destination = destination;
    }

    public TilePosition Destination { get; }
}

public sealed class GatherMacroAction : MacroAction
{
    public GatherMacroAction(string id, MacroActionKind kind, string itemName, int itemLevel, float itemWeight, RepeatPolicy repeat)
        : base(RequireId(id), ValidateKind(kind))
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemName);
        if (itemLevel <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemLevel), "Item level must be positive.");
        }

        if (itemWeight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemWeight), "Item weight cannot be negative.");
        }

        ItemName = itemName;
        ItemLevel = itemLevel;
        ItemWeight = itemWeight;
        Repeat = repeat ?? throw new ArgumentNullException(nameof(repeat));
    }

    public string ItemName { get; }
    public int ItemLevel { get; }
    public float ItemWeight { get; }
    public RepeatPolicy Repeat { get; private set; }

    public void SetRepeat(RepeatPolicy repeat)
    {
        Repeat = repeat ?? throw new ArgumentNullException(nameof(repeat));
    }

    private static MacroActionKind ValidateKind(MacroActionKind kind)
    {
        return kind is MacroActionKind.Mine or MacroActionKind.CutWood
            ? kind
            : throw new ArgumentException("Gather action kind must be Mine or CutWood.", nameof(kind));
    }
}
```

- [ ] **Step 6: Run tests and verify they pass**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~Automation
```

Expected: the three repeat policy tests and two Macro action tests pass.

- [ ] **Step 7: Commit**

```powershell
git add src/Core/Automation/RepeatPolicy.cs src/Core/Automation/MacroAction.cs src/Tests/Automation/RepeatPolicyTests.cs src/Tests/Automation/PartyMacroTests.cs
git commit -m "feat: add macro action primitives"
```

### Task 2: Editable Party Macros And Recording

**Files:**
- Create: `src/Core/Automation/PartyMacro.cs`
- Create: `src/Core/Automation/MacroRecordingSession.cs`
- Create: `src/Core/Automation/PartyAutomation.cs`
- Modify: `src/Core/Party/Party.cs`
- Test: `src/Tests/Automation/MacroRecordingSessionTests.cs`
- Test: `src/Tests/Automation/PartyMacroTests.cs`

- [ ] **Step 1: Add failing tests for Macro editing**

Append to `src/Tests/Automation/PartyMacroTests.cs`:

```csharp
[Fact]
public void Macro_can_remove_and_reorder_actions()
{
    var macro = new PartyMacro("macro-1", "Mining Run");
    macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(1, 0)));
    macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));
    macro.AddAction(new MoveToMacroAction("move-2", new TilePosition(2, 0)));

    macro.RemoveAction("mine-1");
    macro.MoveAction("move-2", newIndex: 0);

    Assert.Collection(
        macro.Actions,
        action => Assert.Equal("move-2", action.Id),
        action => Assert.Equal("move-1", action.Id));
}

[Fact]
public void Rename_requires_non_empty_name()
{
    var macro = new PartyMacro("macro-1", "Mining Run");

    Assert.Throws<ArgumentException>(() => macro.Rename(""));
}

[Fact]
public void Macro_updates_gather_action_repeat()
{
    var macro = new PartyMacro("macro-1", "Mining Run");
    macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));

    macro.SetGatherActionRepeat("mine-1", RepeatPolicy.Count(5));

    var action = Assert.IsType<GatherMacroAction>(macro.Actions[0]);
    Assert.Equal(5, action.Repeat.RepeatCount);
}
```

- [ ] **Step 2: Add failing tests for Record Macro**

Create `src/Tests/Automation/MacroRecordingSessionTests.cs`:

```csharp
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.Core.Tests.Automation;

public sealed class MacroRecordingSessionTests
{
    [Fact]
    public void Record_gather_adds_move_to_before_mine()
    {
        var recording = new MacroRecordingSession("recording-1");

        recording.RecordGather(
            target: new TilePosition(2, 0),
            kind: MacroActionKind.Mine,
            itemName: "Iron",
            itemLevel: 1,
            itemWeight: 3);

        Assert.Collection(
            recording.Actions,
            action => Assert.IsType<MoveToMacroAction>(action),
            action => Assert.IsType<GatherMacroAction>(action));
        Assert.Equal(new TilePosition(2, 0), ((MoveToMacroAction)recording.Actions[0]).Destination);
    }

    [Fact]
    public void Save_requires_macro_name()
    {
        var recording = new MacroRecordingSession("recording-1");

        Assert.Throws<ArgumentException>(() => recording.Save(""));
    }

    [Fact]
    public void Party_owns_automation_state()
    {
        var party = new Party("party-1", new TilePosition(0, 0));

        Assert.NotNull(party.Automation);
        Assert.Empty(party.Automation.Macros);
    }
}
```

- [ ] **Step 3: Run tests and verify they fail**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~Automation
```

Expected: build fails because `PartyMacro`, `MacroRecordingSession` and `Party.Automation` do not exist.

- [ ] **Step 4: Implement PartyMacro**

Create `src/Core/Automation/PartyMacro.cs`:

```csharp
namespace RymoraLandOfHeroes.Core.Automation;

public sealed class PartyMacro
{
    private readonly List<MacroAction> _actions = new();

    public PartyMacro(string id, string name, IEnumerable<MacroAction>? actions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        if (actions is not null)
        {
            _actions.AddRange(actions);
        }
    }

    public string Id { get; }
    public string Name { get; private set; }
    public IReadOnlyList<MacroAction> Actions => _actions;

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public void AddAction(MacroAction action)
    {
        _actions.Add(action ?? throw new ArgumentNullException(nameof(action)));
    }

    public void RemoveAction(string actionId)
    {
        var index = _actions.FindIndex(action => action.Id == actionId);
        if (index < 0)
        {
            throw new InvalidOperationException($"Macro action not found: {actionId}.");
        }

        _actions.RemoveAt(index);
    }

    public void MoveAction(string actionId, int newIndex)
    {
        if (newIndex < 0 || newIndex >= _actions.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(newIndex), "Action index is outside the Macro action list.");
        }

        var index = _actions.FindIndex(action => action.Id == actionId);
        if (index < 0)
        {
            throw new InvalidOperationException($"Macro action not found: {actionId}.");
        }

        var action = _actions[index];
        _actions.RemoveAt(index);
        _actions.Insert(newIndex, action);
    }

    public void SetGatherActionRepeat(string actionId, RepeatPolicy repeat)
    {
        var action = _actions.FirstOrDefault(action => action.Id == actionId);
        if (action is null)
        {
            throw new InvalidOperationException($"Macro action not found: {actionId}.");
        }

        if (action is not GatherMacroAction gather)
        {
            throw new InvalidOperationException($"Macro action does not support repeat: {actionId}.");
        }

        gather.SetRepeat(repeat);
    }
}
```

- [ ] **Step 5: Implement recording session and party automation**

Create `src/Core/Automation/MacroRecordingSession.cs`:

```csharp
using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.Automation;

public sealed class MacroRecordingSession
{
    private readonly List<MacroAction> _actions = new();
    private int _nextActionNumber;

    public MacroRecordingSession(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id;
    }

    public string Id { get; }
    public IReadOnlyList<MacroAction> Actions => _actions;

    public void RecordMove(TilePosition target)
    {
        _actions.Add(new MoveToMacroAction(NextActionId("move"), target));
    }

    public void RecordGather(TilePosition target, MacroActionKind kind, string itemName, int itemLevel, float itemWeight)
    {
        RecordMove(target);
        _actions.Add(new GatherMacroAction(NextActionId(kind == MacroActionKind.Mine ? "mine" : "cutwood"), kind, itemName, itemLevel, itemWeight, RepeatPolicy.Once));
    }

    public PartyMacro Save(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new PartyMacro(Id, name, _actions.ToArray());
    }

    private string NextActionId(string prefix)
    {
        _nextActionNumber++;
        return $"{Id}-{prefix}-{_nextActionNumber}";
    }
}
```

Create `src/Core/Automation/PartyAutomation.cs`:

```csharp
namespace RymoraLandOfHeroes.Core.Automation;

public sealed class PartyAutomation
{
    private readonly List<PartyMacro> _macros = new();

    public IReadOnlyList<PartyMacro> Macros => _macros;
    public MacroRecordingSession? Recording { get; private set; }

    public MacroRecordingSession StartRecording(string recordingId)
    {
        if (Recording is not null)
        {
            throw new InvalidOperationException("A Macro recording session is already active.");
        }

        Recording = new MacroRecordingSession(recordingId);
        return Recording;
    }

    public PartyMacro SaveRecording(string name)
    {
        if (Recording is null)
        {
            throw new InvalidOperationException("No Macro recording session is active.");
        }

        var macro = Recording.Save(name);
        _macros.Add(macro);
        Recording = null;
        return macro;
    }

    public void CancelRecording()
    {
        Recording = null;
    }

    public PartyMacro GetMacro(string macroId)
    {
        return TryGetMacro(macroId)
            ?? throw new InvalidOperationException($"Macro not found: {macroId}.");
    }

    public PartyMacro? TryGetMacro(string macroId)
    {
        return _macros.FirstOrDefault(macro => macro.Id == macroId);
    }
}
```

Modify `src/Core/Party/Party.cs`:

```csharp
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Hero;
```

Add property near `Inventory`:

```csharp
public PartyAutomation Automation { get; } = new();
```

- [ ] **Step 6: Run tests and verify they pass**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~Automation
```

Expected: automation tests pass.

- [ ] **Step 7: Run all Core tests**

Run:

```powershell
dotnet test RymoraLandOfHeroes.sln --no-restore
```

Expected: all tests pass.

- [ ] **Step 8: Commit**

```powershell
git add src/Core/Automation/PartyMacro.cs src/Core/Automation/MacroRecordingSession.cs src/Core/Automation/PartyAutomation.cs src/Core/Party/Party.cs src/Tests/Automation/MacroRecordingSessionTests.cs src/Tests/Automation/PartyMacroTests.cs
git commit -m "feat: add party macro recording"
```

### Task 3: Party Program Editing Model

**Files:**
- Create: `src/Core/Automation/PartyProgram.cs`
- Modify: `src/Core/Automation/PartyAutomation.cs`
- Test: `src/Tests/Automation/PartyProgramTests.cs`

- [ ] **Step 1: Write failing Program model tests**

Create `src/Tests/Automation/PartyProgramTests.cs`:

```csharp
using RymoraLandOfHeroes.Core.Automation;

namespace RymoraLandOfHeroes.Core.Tests.Automation;

public sealed class PartyProgramTests
{
    [Fact]
    public void Program_adds_macro_reference_as_step()
    {
        var program = new PartyProgram();

        var step = program.AddStep("macro-1", RepeatPolicy.Count(3));

        Assert.Equal("macro-1", step.MacroId);
        Assert.Equal(RepeatMode.Count, step.Repeat.Mode);
        Assert.Equal(3, step.Repeat.RepeatCount);
    }

    [Fact]
    public void Program_reorders_steps()
    {
        var program = new PartyProgram();
        var first = program.AddStep("macro-1", RepeatPolicy.Once);
        var second = program.AddStep("macro-2", RepeatPolicy.Once);

        program.MoveStep(second.Id, newIndex: 0);

        Assert.Collection(
            program.Steps,
            step => Assert.Equal(second.Id, step.Id),
            step => Assert.Equal(first.Id, step.Id));
    }

    [Fact]
    public void Party_automation_has_active_program()
    {
        var automation = new PartyAutomation();

        Assert.NotNull(automation.Program);
        Assert.Empty(automation.Program.Steps);
    }
}
```

- [ ] **Step 2: Run tests and verify they fail**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~PartyProgramTests
```

Expected: build fails because `PartyProgram` and `ProgramStep` do not exist.

- [ ] **Step 3: Implement Program model**

Create `src/Core/Automation/PartyProgram.cs`:

```csharp
namespace RymoraLandOfHeroes.Core.Automation;

public sealed class PartyProgram
{
    private readonly List<ProgramStep> _steps = new();
    private int _nextStepNumber;

    public IReadOnlyList<ProgramStep> Steps => _steps;
    public RepeatPolicy Repeat { get; private set; } = RepeatPolicy.Once;

    public ProgramStep AddStep(string macroId, RepeatPolicy repeat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(macroId);
        var step = new ProgramStep(NextStepId(), macroId, repeat);
        _steps.Add(step);
        return step;
    }

    public void RemoveStep(string stepId)
    {
        var index = _steps.FindIndex(step => step.Id == stepId);
        if (index < 0)
        {
            throw new InvalidOperationException($"Program step not found: {stepId}.");
        }

        _steps.RemoveAt(index);
    }

    public void MoveStep(string stepId, int newIndex)
    {
        if (newIndex < 0 || newIndex >= _steps.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(newIndex), "Program step index is outside the Program step list.");
        }

        var index = _steps.FindIndex(step => step.Id == stepId);
        if (index < 0)
        {
            throw new InvalidOperationException($"Program step not found: {stepId}.");
        }

        var step = _steps[index];
        _steps.RemoveAt(index);
        _steps.Insert(newIndex, step);
    }

    public void SetProgramRepeat(RepeatPolicy repeat)
    {
        Repeat = repeat ?? throw new ArgumentNullException(nameof(repeat));
    }

    public void SetStepRepeat(string stepId, RepeatPolicy repeat)
    {
        var index = _steps.FindIndex(step => step.Id == stepId);
        if (index < 0)
        {
            throw new InvalidOperationException($"Program step not found: {stepId}.");
        }

        _steps[index].SetRepeat(repeat);
    }

    private string NextStepId()
    {
        _nextStepNumber++;
        return $"step-{_nextStepNumber}";
    }
}

public sealed class ProgramStep
{
    public ProgramStep(string id, string macroId, RepeatPolicy repeat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(macroId);

        Id = id;
        MacroId = macroId;
        Repeat = repeat ?? throw new ArgumentNullException(nameof(repeat));
    }

    public string Id { get; }
    public string MacroId { get; }
    public RepeatPolicy Repeat { get; private set; }

    public void SetRepeat(RepeatPolicy repeat)
    {
        Repeat = repeat ?? throw new ArgumentNullException(nameof(repeat));
    }
}
```

- [ ] **Step 4: Add Program to PartyAutomation**

Modify `src/Core/Automation/PartyAutomation.cs`:

```csharp
public PartyProgram Program { get; } = new();
```

- [ ] **Step 5: Run tests and verify they pass**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~PartyProgramTests
```

Expected: three Program model tests pass.

- [ ] **Step 6: Run all tests and commit**

Run:

```powershell
dotnet test RymoraLandOfHeroes.sln --no-restore
```

Expected: all tests pass.

Commit:

```powershell
git add src/Core/Automation/PartyProgram.cs src/Core/Automation/PartyAutomation.cs src/Tests/Automation/PartyProgramTests.cs
git commit -m "feat: add party program model"
```

### Task 4: Program Runner State Machine

**Files:**
- Create: `src/Core/Automation/MacroActionExecution.cs`
- Create: `src/Core/Automation/ProgramRunner.cs`
- Modify: `src/Core/Automation/PartyAutomation.cs`
- Test: `src/Tests/Automation/ProgramRunnerTests.cs`

- [ ] **Step 1: Write failing runner tests**

Create `src/Tests/Automation/ProgramRunnerTests.cs`:

```csharp
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.Tests.Automation;

public sealed class ProgramRunnerTests
{
    [Fact]
    public void Play_emits_first_action_from_first_program_macro()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        automation.Runner.Play();
        var execution = automation.Runner.TryStartNextAction(automation);

        Assert.Equal(ProgramRunnerState.Running, automation.Runner.State);
        Assert.NotNull(execution);
        Assert.IsType<MoveToMacroAction>(execution!.Action);
    }

    [Fact]
    public void Edited_macro_applies_next_time_macro_starts()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Count(2));

        automation.Runner.Play();
        var firstMove = automation.Runner.TryStartNextAction(automation)!;
        automation.GetMacro("macro-1").AddAction(new MoveToMacroAction("move-late", new TilePosition(3, 0)));

        automation.Runner.CompleteAction(firstMove.ExecutionId, elapsedSeconds: 1);
        var firstMine = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.CompleteAction(firstMine.ExecutionId, elapsedSeconds: 1);
        var secondMove = automation.Runner.TryStartNextAction(automation)!;

        Assert.Equal("move-1", secondMove.Action.Id);
    }

    [Fact]
    public void Gather_action_repeat_count_repeats_action_before_next_macro_action()
    {
        var automation = new PartyAutomation();
        var macro = new PartyMacro("macro-1", "Mining");
        macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Count(2)));
        macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(2, 0)));
        automation.AddMacro(macro);
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        automation.Runner.Play();
        var firstMine = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.CompleteAction(firstMine.ExecutionId, elapsedSeconds: 1);
        var secondMine = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.CompleteAction(secondMine.ExecutionId, elapsedSeconds: 1);
        var move = automation.Runner.TryStartNextAction(automation)!;

        Assert.Equal("mine-1", firstMine.Action.Id);
        Assert.Equal("mine-1", secondMine.Action.Id);
        Assert.Equal("move-1", move.Action.Id);
    }

    [Fact]
    public void Program_repeat_count_restarts_sequence()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);
        automation.Program.SetProgramRepeat(RepeatPolicy.Count(2));

        automation.Runner.Play();
        CompleteNext(automation);
        CompleteNext(automation);
        var secondProgramMove = automation.Runner.TryStartNextAction(automation)!;

        Assert.Equal("move-1", secondProgramMove.Action.Id);
    }

    [Fact]
    public void Program_step_duration_repeats_macro_until_elapsed_time_is_reached()
    {
        var automation = new PartyAutomation();
        var macro = new PartyMacro("macro-1", "Patrol");
        macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(1, 0)));
        automation.AddMacro(macro);
        automation.Program.AddStep("macro-1", RepeatPolicy.Duration(2));

        automation.Runner.Play();
        CompleteNext(automation);
        var secondMove = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.CompleteAction(secondMove.ExecutionId, elapsedSeconds: 1);
        var finished = automation.Runner.TryStartNextAction(automation);

        Assert.Equal("move-1", secondMove.Action.Id);
        Assert.Null(finished);
        Assert.Equal(ProgramRunnerState.Idle, automation.Runner.State);
    }

    [Fact]
    public void Stop_waits_for_current_action_then_returns_to_idle()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Forever);

        automation.Runner.Play();
        var execution = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.Stop();
        automation.Runner.CompleteAction(execution.ExecutionId, elapsedSeconds: 1);

        Assert.Equal(ProgramRunnerState.Idle, automation.Runner.State);
    }

    [Fact]
    public void Pause_waits_for_current_action_and_resume_continues_next_action()
    {
        var automation = CreateAutomationWithMiningMacro();
        automation.Program.AddStep("macro-1", RepeatPolicy.Once);

        automation.Runner.Play();
        var move = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.Pause();
        automation.Runner.CompleteAction(move.ExecutionId, elapsedSeconds: 1);

        Assert.Equal(ProgramRunnerState.Paused, automation.Runner.State);

        automation.Runner.Play();
        var mine = automation.Runner.TryStartNextAction(automation)!;

        Assert.Equal("mine-1", mine.Action.Id);
    }

    [Fact]
    public void Fail_sets_error_message()
    {
        var automation = CreateAutomationWithMiningMacro();

        automation.Runner.Fail("No path to destination.");

        Assert.Equal(ProgramRunnerState.Error, automation.Runner.State);
        Assert.Equal("No path to destination.", automation.Runner.ErrorMessage);
    }

    [Fact]
    public void Missing_macro_reference_sets_error()
    {
        var automation = new PartyAutomation();
        automation.Program.AddStep("missing-macro", RepeatPolicy.Once);

        automation.Runner.Play();
        var execution = automation.Runner.TryStartNextAction(automation);

        Assert.Null(execution);
        Assert.Equal(ProgramRunnerState.Error, automation.Runner.State);
        Assert.Equal("Macro not found: missing-macro.", automation.Runner.ErrorMessage);
    }

    private static void CompleteNext(PartyAutomation automation)
    {
        var execution = automation.Runner.TryStartNextAction(automation)!;
        automation.Runner.CompleteAction(execution.ExecutionId, elapsedSeconds: 1);
    }

    private static PartyAutomation CreateAutomationWithMiningMacro()
    {
        var automation = new PartyAutomation();
        var macro = new PartyMacro("macro-1", "Mining");
        macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(1, 0)));
        macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));
        automation.AddMacro(macro);
        return automation;
    }
}
```

- [ ] **Step 2: Run runner tests and verify they fail**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~ProgramRunnerTests
```

Expected: build fails because `ProgramRunner`, `ProgramRunnerState`, `MacroActionExecution`, `PartyAutomation.Runner` and `PartyAutomation.AddMacro` do not exist.

- [ ] **Step 3: Implement MacroActionExecution**

Create `src/Core/Automation/MacroActionExecution.cs`:

```csharp
namespace RymoraLandOfHeroes.Core.Automation;

public sealed record MacroActionExecution(string ExecutionId, MacroAction Action);
```

- [ ] **Step 4: Implement ProgramRunner**

Create `src/Core/Automation/ProgramRunner.cs`:

```csharp
namespace RymoraLandOfHeroes.Core.Automation;

public enum ProgramRunnerState
{
    Idle,
    Running,
    PauseRequested,
    Paused,
    StopRequested,
    Error
}

public sealed class ProgramRunner
{
    private IReadOnlyList<MacroAction> _currentMacroActions = Array.Empty<MacroAction>();
    private int _programStepIndex;
    private int _macroActionIndex;
    private int _stepIteration;
    private int _programIteration;
    private int _actionIteration;
    private float _programElapsedSeconds;
    private float _stepElapsedSeconds;
    private float _actionElapsedSeconds;
    private string? _currentExecutionId;

    public ProgramRunnerState State { get; private set; } = ProgramRunnerState.Idle;
    public string? ErrorMessage { get; private set; }
    public MacroAction? CurrentAction { get; private set; }
    public MacroAction? NextAction => _macroActionIndex < _currentMacroActions.Count ? _currentMacroActions[_macroActionIndex] : null;

    public void Play()
    {
        if (State == ProgramRunnerState.Paused)
        {
            State = ProgramRunnerState.Running;
            return;
        }

        _programStepIndex = 0;
        _macroActionIndex = 0;
        _stepIteration = 0;
        _programIteration = 0;
        _actionIteration = 0;
        _programElapsedSeconds = 0;
        _stepElapsedSeconds = 0;
        _actionElapsedSeconds = 0;
        _currentMacroActions = Array.Empty<MacroAction>();
        _currentExecutionId = null;
        ErrorMessage = null;
        CurrentAction = null;
        State = ProgramRunnerState.Running;
    }

    public void Pause()
    {
        if (State == ProgramRunnerState.Running)
        {
            State = ProgramRunnerState.PauseRequested;
        }
    }

    public void Stop()
    {
        if (State is ProgramRunnerState.Running or ProgramRunnerState.Paused or ProgramRunnerState.PauseRequested)
        {
            State = ProgramRunnerState.StopRequested;
        }
    }

    public MacroActionExecution? TryStartNextAction(PartyAutomation automation)
    {
        if (State != ProgramRunnerState.Running || _currentExecutionId is not null)
        {
            return null;
        }

        if (automation.Program.Steps.Count == 0)
        {
            Fail("Program has no Macro steps.");
            return null;
        }

        if (_currentMacroActions.Count == 0 || _macroActionIndex >= _currentMacroActions.Count)
        {
            if (!StartNextMacro(automation))
            {
                State = ProgramRunnerState.Idle;
                return null;
            }
        }

        var action = _currentMacroActions[_macroActionIndex];
        _actionIteration++;
        _currentExecutionId = $"run-{Guid.NewGuid():N}";
        CurrentAction = action;
        return new MacroActionExecution(_currentExecutionId, action);
    }

    public void CompleteAction(string executionId, float elapsedSeconds)
    {
        if (_currentExecutionId != executionId)
        {
            throw new InvalidOperationException($"Unknown automation execution id: {executionId}.");
        }

        _currentExecutionId = null;
        CurrentAction = null;
        _actionElapsedSeconds += elapsedSeconds;
        _stepElapsedSeconds += elapsedSeconds;
        _programElapsedSeconds += elapsedSeconds;

        var shouldRepeatCurrentAction = ShouldRepeatCurrentAction();
        if (!shouldRepeatCurrentAction)
        {
            _macroActionIndex++;
            _actionIteration = 0;
            _actionElapsedSeconds = 0;
        }

        if (State == ProgramRunnerState.PauseRequested)
        {
            State = ProgramRunnerState.Paused;
            return;
        }

        if (State == ProgramRunnerState.StopRequested)
        {
            ResetToIdle();
            return;
        }
    }

    public void Fail(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ErrorMessage = message;
        _currentExecutionId = null;
        CurrentAction = null;
        State = ProgramRunnerState.Error;
    }

    private bool StartNextMacro(PartyAutomation automation)
    {
        while (true)
        {
            if (_programStepIndex >= automation.Program.Steps.Count)
            {
                _programIteration++;
                if (!ShouldRepeat(automation.Program.Repeat, _programIteration, _programElapsedSeconds))
                {
                    return false;
                }

                _programStepIndex = 0;
                _stepIteration = 0;
                _stepElapsedSeconds = 0;
            }

            var step = automation.Program.Steps[_programStepIndex];
            if (!ShouldRepeat(step.Repeat, _stepIteration, _stepElapsedSeconds))
            {
                _programStepIndex++;
                _stepIteration = 0;
                _stepElapsedSeconds = 0;
                continue;
            }

            var macro = automation.TryGetMacro(step.MacroId);
            if (macro is null)
            {
                Fail($"Macro not found: {step.MacroId}.");
                return false;
            }

            _currentMacroActions = macro.Actions.ToArray();
            _macroActionIndex = 0;
            _actionIteration = 0;
            _actionElapsedSeconds = 0;
            _stepIteration++;

            if (_currentMacroActions.Count == 0)
            {
                _programStepIndex++;
                _stepIteration = 0;
                _stepElapsedSeconds = 0;
                continue;
            }

            return true;
        }
    }

    private bool ShouldRepeatCurrentAction()
    {
        if (_macroActionIndex >= _currentMacroActions.Count)
        {
            return false;
        }

        var action = _currentMacroActions[_macroActionIndex];
        if (action is not GatherMacroAction gather)
        {
            return false;
        }

        return ShouldRepeat(gather.Repeat, _actionIteration, _actionElapsedSeconds);
    }

    private static bool ShouldRepeat(RepeatPolicy repeat, int completedIterations, float elapsedSeconds)
    {
        return repeat.Mode switch
        {
            RepeatMode.Once => completedIterations < 1,
            RepeatMode.Count => completedIterations < repeat.RepeatCount!.Value,
            RepeatMode.Forever => true,
            RepeatMode.Duration => elapsedSeconds < repeat.Seconds!.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(repeat), "Unknown repeat mode.")
        };
    }

    private void ResetToIdle()
    {
        _programStepIndex = 0;
        _macroActionIndex = 0;
        _stepIteration = 0;
        _programIteration = 0;
        _actionIteration = 0;
        _programElapsedSeconds = 0;
        _stepElapsedSeconds = 0;
        _actionElapsedSeconds = 0;
        _currentMacroActions = Array.Empty<MacroAction>();
        _currentExecutionId = null;
        CurrentAction = null;
        State = ProgramRunnerState.Idle;
    }
}
```

- [ ] **Step 5: Add runner and AddMacro to PartyAutomation**

Modify `src/Core/Automation/PartyAutomation.cs`:

```csharp
public ProgramRunner Runner { get; } = new();

public void AddMacro(PartyMacro macro)
{
    _macros.Add(macro ?? throw new ArgumentNullException(nameof(macro)));
}
```

- [ ] **Step 6: Run tests and verify they pass**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~ProgramRunnerTests
```

Expected: Program runner tests pass.

- [ ] **Step 7: Commit**

```powershell
git add src/Core/Automation/MacroActionExecution.cs src/Core/Automation/ProgramRunner.cs src/Core/Automation/PartyAutomation.cs src/Tests/Automation/ProgramRunnerTests.cs
git commit -m "feat: add program runner state"
```

### Task 5: Application Integration For Program Execution

**Files:**
- Modify: `src/Core/Party/PartyAction.cs`
- Modify: `src/Core/Application/GameApplication.cs`
- Test: `src/Tests/Application/GameApplicationAutomationTests.cs`
- Modify: `src/Tests/ObjectMothers/ApplicationObjectMother.cs`

- [ ] **Step 1: Write failing Application automation tests**

Create `src/Tests/Application/GameApplicationAutomationTests.cs`:

```csharp
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Tests.ObjectMothers;

namespace RymoraLandOfHeroes.Core.Tests.Application;

public sealed class GameApplicationAutomationTests
{
    [Fact]
    public void Update_program_moves_then_mines_and_finishes_idle()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();

        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(1);
        scenario.InputApplication.Update(1);
        scenario.InputApplication.Update(0);

        Assert.Equal(new TilePosition(1, 0), scenario.AssertParty.Position);
        Assert.Equal(1, scenario.AssertParty.Inventory.GetQuantity("Iron", 1));
        Assert.Equal(ProgramRunnerState.Idle, scenario.AssertParty.Automation.Runner.State);
    }

    [Fact]
    public void Update_invalid_program_action_sets_runner_error()
    {
        var scenario = ApplicationObjectMother.ApplicationWithInvalidMiningProgram();

        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(1);

        Assert.Equal(ProgramRunnerState.Error, scenario.AssertParty.Automation.Runner.State);
        Assert.Contains("cannot mine", scenario.AssertParty.Automation.Runner.ErrorMessage);
    }

    [Fact]
    public void Stop_program_finishes_current_action_then_resets()
    {
        var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();

        scenario.InputApplication.PlayProgram("party-1");
        scenario.InputApplication.Update(0.5f);
        scenario.InputApplication.StopProgram("party-1");
        scenario.InputApplication.Update(0.5f);

        Assert.Equal(ProgramRunnerState.Idle, scenario.AssertParty.Automation.Runner.State);
    }
}
```

- [ ] **Step 2: Add object mother scenarios**

Append to `src/Tests/ObjectMothers/ApplicationObjectMother.cs`:

```csharp
public static AutomationProgramScenario ApplicationWithMiningProgram()
{
    var party = new GameParty("party-1", new TilePosition(0, 0));
    party.AddMember(TestObjectMother.CreateCreature("Miner"));
    var macro = new PartyMacro("macro-1", "Mine Iron");
    macro.AddAction(new MoveToMacroAction("move-1", new TilePosition(1, 0)));
    macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));
    party.Automation.AddMacro(macro);
    party.Automation.Program.AddStep("macro-1", RepeatPolicy.Once);

    var application = new GameApplication(
        TestObjectMother.CreateWorld(
            random: new SequenceRandomSource(1),
            minePositions: new[] { new TilePosition(1, 0) }),
        new[] { party },
        TestObjectMother.CreateGameConfig(),
        TestObjectMother.CreateMonster,
        new SequenceRandomSource(1));

    return new AutomationProgramScenario(application, party);
}

public static AutomationProgramScenario ApplicationWithInvalidMiningProgram()
{
    var party = new GameParty("party-1", new TilePosition(0, 0));
    party.AddMember(TestObjectMother.CreateCreature("Miner"));
    var macro = new PartyMacro("macro-1", "Invalid Mine");
    macro.AddAction(new GatherMacroAction("mine-1", MacroActionKind.Mine, "Iron", 1, 3, RepeatPolicy.Once));
    party.Automation.AddMacro(macro);
    party.Automation.Program.AddStep("macro-1", RepeatPolicy.Once);

    var application = new GameApplication(
        TestObjectMother.CreateWorld(random: new SequenceRandomSource(1)),
        new[] { party },
        TestObjectMother.CreateGameConfig(),
        TestObjectMother.CreateMonster,
        new SequenceRandomSource(1));

    return new AutomationProgramScenario(application, party);
}
```

Add record near the other scenario records:

```csharp
internal sealed record AutomationProgramScenario(GameApplication InputApplication, GameParty AssertParty);
```

Add `using RymoraLandOfHeroes.Core.Automation;` to the top of the file.

- [ ] **Step 3: Run tests and verify they fail**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~GameApplicationAutomationTests
```

Expected: build fails because `GameApplication.PlayProgram` and `StopProgram` do not exist and `PartyActionRequest` has no automation id.

- [ ] **Step 4: Add automation metadata to PartyActionRequest and queue completion**

Modify `src/Core/Party/PartyAction.cs` record signature:

```csharp
public sealed record PartyActionRequest(
    PartyActionType ActionType,
    PartyActionEndType EndType,
    float TimeToExecute,
    int? LimitCount = null,
    float? EndTime = null,
    string? ItemName = null,
    int? ItemLevel = null,
    float? ItemWeight = null,
    int? Quantity = null,
    string? TargetPartyId = null,
    TilePosition? Destination = null,
    IReadOnlyList<TilePosition>? Path = null,
    string? AutomationActionId = null);
```

Modify `PartyActionQueue`:

```csharp
public bool IsIdle => Current is null && _pending.Count == 0;

public PartyActionState? CompleteCurrentIfFinished(int currentItemQuantity = 0)
{
    if (Current?.IsComplete(currentItemQuantity) != true)
    {
        return null;
    }

    var completed = Current;
    Current = null;
    return completed;
}
```

- [ ] **Step 5: Add Program controls to GameApplication**

Add `using RymoraLandOfHeroes.Core.Automation;` to `src/Core/Application/GameApplication.cs`.

Add public methods near `EnqueueAction`:

```csharp
public void PlayProgram(string partyId)
{
    var party = Parties.Get(partyId);
    party.Automation.Runner.Play();
}

public void PauseProgram(string partyId)
{
    Parties.Get(partyId).Automation.Runner.Pause();
}

public void StopProgram(string partyId)
{
    Parties.Get(partyId).Automation.Runner.Stop();
}
```

- [ ] **Step 6: Run automation before party actions**

In `RunPartyActions`, insert before `StartNextIfIdle()`:

```csharp
if (party.ActionQueue.IsIdle)
{
    QueueNextAutomationAction(party);
}
```

Add helper methods to `GameApplication`:

```csharp
private void QueueNextAutomationAction(GameParty party)
{
    var execution = party.Automation.Runner.TryStartNextAction(party.Automation);
    if (execution is null)
    {
        return;
    }

    var request = CreateRequestForAutomationAction(party, execution);
    if (request is null)
    {
        return;
    }

    party.ActionQueue.Enqueue(request);
}

private PartyActionRequest? CreateRequestForAutomationAction(GameParty party, MacroActionExecution execution)
{
    switch (execution.Action)
    {
        case MoveToMacroAction move:
            var travel = new PartyActionRequest(
                PartyActionType.Travel,
                PartyActionEndType.ByCount,
                Config.Travel.ActionTime,
                Destination: move.Destination,
                AutomationActionId: execution.ExecutionId);
            var prepared = PrepareAction(party, travel);
            if (prepared is null)
            {
                party.Automation.Runner.Fail($"No path to destination ({move.Destination.X}, {move.Destination.Y}).");
            }

            return prepared;

        case GatherMacroAction gather:
            return CreateGatherRequestForAutomationAction(party, gather, execution.ExecutionId);

        default:
            party.Automation.Runner.Fail($"Unsupported Macro action: {execution.Action.Kind}.");
            return null;
    }
}

private PartyActionRequest? CreateGatherRequestForAutomationAction(GameParty party, GatherMacroAction gather, string executionId)
{
    var terrain = World.GetTerrain(party.Position);
    if (gather.Kind == MacroActionKind.Mine && !terrain.AllowsMining)
    {
        party.Automation.Runner.Fail($"Party cannot mine at ({party.Position.X}, {party.Position.Y}).");
        return null;
    }

    if (gather.Kind == MacroActionKind.CutWood && !terrain.AllowsWoodcutting)
    {
        party.Automation.Runner.Fail($"Party cannot cut wood at ({party.Position.X}, {party.Position.Y}).");
        return null;
    }

    return new PartyActionRequest(
        gather.Kind == MacroActionKind.Mine ? PartyActionType.Mine : PartyActionType.CutWood,
        PartyActionEndType.ByCount,
        gather.Kind == MacroActionKind.Mine ? Config.Collection.MiningActionTime : Config.Collection.WoodcuttingActionTime,
        LimitCount: 1,
        ItemName: gather.ItemName,
        ItemLevel: gather.ItemLevel,
        ItemWeight: gather.ItemWeight,
        AutomationActionId: executionId);
}
```

The runner owns repetition for Macro actions. The queued gather request always represents one execution, so `Pause` and `Stop` can finish the current execution before pausing or stopping.

- [ ] **Step 7: Notify runner when an automation action completes**

Add helper:

```csharp
private static void NotifyAutomationActionCompleted(GameParty party, PartyActionState? completed)
{
    if (completed?.Request.AutomationActionId is null)
    {
        return;
    }

    party.Automation.Runner.CompleteAction(completed.Request.AutomationActionId, completed.PassedTime);
}
```

Update places that call `CompleteCurrentIfFinished` after successful execution:

```csharp
var completed = party.ActionQueue.CompleteCurrentIfFinished();
NotifyAutomationActionCompleted(party, completed);
```

Also update the early completion branch in `RunPartyActions` so zero-length `MoveTo` completes and advances automation:

```csharp
if (current.IsComplete(GetCurrentItemQuantity(party, current.Request)))
{
    var completed = party.ActionQueue.CompleteCurrentIfFinished(GetCurrentItemQuantity(party, current.Request));
    NotifyAutomationActionCompleted(party, completed);
    return;
}
```

and for collection:

```csharp
var completed = party.ActionQueue.CompleteCurrentIfFinished(GetCurrentItemQuantity(party, request));
NotifyAutomationActionCompleted(party, completed);
```

- [ ] **Step 8: Convert invalid collection clear into automation error when applicable**

Before each `party.ActionQueue.Clear()` in `ExecuteCollect`, call:

```csharp
FailAutomationIfCurrentActionIsAutomated(party, state, "Collection action failed its current tile requirements.");
```

Add helper:

```csharp
private static void FailAutomationIfCurrentActionIsAutomated(GameParty party, PartyActionState state, string message)
{
    if (state.Request.AutomationActionId is not null)
    {
        party.Automation.Runner.Fail(message);
    }
}
```

- [ ] **Step 9: Run tests and verify they pass**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~GameApplicationAutomationTests
```

Expected: three Application automation tests pass.

- [ ] **Step 10: Run all tests and commit**

Run:

```powershell
dotnet test RymoraLandOfHeroes.sln --no-restore
```

Expected: all tests pass.

Commit:

```powershell
git add src/Core/Party/PartyAction.cs src/Core/Application/GameApplication.cs src/Tests/Application/GameApplicationAutomationTests.cs src/Tests/ObjectMothers/ApplicationObjectMother.cs
git commit -m "feat: run party automation programs"
```

### Task 6: Application Commands For Recording And Program Editing

**Files:**
- Modify: `src/Core/Application/GameApplication.cs`
- Modify: `src/Core/Application/PlayerIntent.cs`
- Test: `src/Tests/Application/GameApplicationAutomationTests.cs`

- [ ] **Step 1: Write failing tests for recording through Application**

Append to `src/Tests/Application/GameApplicationAutomationTests.cs`:

```csharp
[Fact]
public void Application_records_macro_action_without_moving_party()
{
    var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();
    var start = scenario.AssertParty.Position;

    scenario.InputApplication.StartRecordingMacro("party-1");
    scenario.InputApplication.RecordGatherAction(
        "party-1",
        new TilePosition(1, 0),
        MacroActionKind.Mine,
        "Iron",
        itemLevel: 1,
        itemWeight: 3);
    scenario.InputApplication.SaveRecordingMacro("party-1", "Mine Iron");

    Assert.Equal(start, scenario.AssertParty.Position);
    Assert.Contains(scenario.AssertParty.Automation.Macros, macro => macro.Name == "Mine Iron");
}

[Fact]
public void Application_adds_macro_to_program()
{
    var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();

    var step = scenario.InputApplication.AddMacroToProgram("party-1", "macro-1", RepeatPolicy.Count(2));

    Assert.Equal("macro-1", step.MacroId);
    Assert.Equal(2, step.Repeat.RepeatCount);
}

[Fact]
public void Application_edits_macro_and_program_repeat()
{
    var scenario = ApplicationObjectMother.ApplicationWithMiningProgram();
    var step = scenario.AssertParty.Automation.Program.Steps[0];

    scenario.InputApplication.RenameMacro("party-1", "macro-1", "Iron Loop");
    scenario.InputApplication.SetGatherActionRepeat("party-1", "macro-1", "mine-1", RepeatPolicy.Count(4));
    scenario.InputApplication.SetProgramStepRepeat("party-1", step.Id, RepeatPolicy.Count(2));

    var macro = scenario.AssertParty.Automation.GetMacro("macro-1");
    var gather = Assert.IsType<GatherMacroAction>(macro.Actions[1]);
    Assert.Equal("Iron Loop", macro.Name);
    Assert.Equal(4, gather.Repeat.RepeatCount);
    Assert.Equal(2, scenario.AssertParty.Automation.Program.Steps[0].Repeat.RepeatCount);
}
```

- [ ] **Step 2: Run tests and verify they fail**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~GameApplicationAutomationTests
```

Expected: build fails because `StartRecordingMacro`, `RecordGatherAction`, `SaveRecordingMacro` and `AddMacroToProgram` do not exist.

- [ ] **Step 3: Add Application command methods**

Add to `GameApplication`:

```csharp
private int _nextRecordingNumber;

public void StartRecordingMacro(string partyId)
{
    var party = Parties.Get(partyId);
    _nextRecordingNumber++;
    party.Automation.StartRecording($"macro-{_nextRecordingNumber}");
}

public void RecordMoveAction(string partyId, TilePosition target)
{
    var recording = Parties.Get(partyId).Automation.Recording
        ?? throw new InvalidOperationException("No Macro recording session is active.");
    recording.RecordMove(target);
}

public void RecordGatherAction(string partyId, TilePosition target, MacroActionKind kind, string itemName, int itemLevel, float itemWeight)
{
    var recording = Parties.Get(partyId).Automation.Recording
        ?? throw new InvalidOperationException("No Macro recording session is active.");
    recording.RecordGather(target, kind, itemName, itemLevel, itemWeight);
}

public PartyMacro SaveRecordingMacro(string partyId, string name)
{
    return Parties.Get(partyId).Automation.SaveRecording(name);
}

public void CancelRecordingMacro(string partyId)
{
    Parties.Get(partyId).Automation.CancelRecording();
}

public ProgramStep AddMacroToProgram(string partyId, string macroId, RepeatPolicy repeat)
{
    var party = Parties.Get(partyId);
    party.Automation.GetMacro(macroId);
    return party.Automation.Program.AddStep(macroId, repeat);
}

public void MoveProgramStep(string partyId, string stepId, int newIndex)
{
    Parties.Get(partyId).Automation.Program.MoveStep(stepId, newIndex);
}

public void RemoveProgramStep(string partyId, string stepId)
{
    Parties.Get(partyId).Automation.Program.RemoveStep(stepId);
}

public void RenameMacro(string partyId, string macroId, string name)
{
    Parties.Get(partyId).Automation.GetMacro(macroId).Rename(name);
}

public void RemoveMacroAction(string partyId, string macroId, string actionId)
{
    Parties.Get(partyId).Automation.GetMacro(macroId).RemoveAction(actionId);
}

public void MoveMacroAction(string partyId, string macroId, string actionId, int newIndex)
{
    Parties.Get(partyId).Automation.GetMacro(macroId).MoveAction(actionId, newIndex);
}

public void SetGatherActionRepeat(string partyId, string macroId, string actionId, RepeatPolicy repeat)
{
    Parties.Get(partyId).Automation.GetMacro(macroId).SetGatherActionRepeat(actionId, repeat);
}

public void SetProgramRepeat(string partyId, RepeatPolicy repeat)
{
    Parties.Get(partyId).Automation.Program.SetProgramRepeat(repeat);
}

public void SetProgramStepRepeat(string partyId, string stepId, RepeatPolicy repeat)
{
    Parties.Get(partyId).Automation.Program.SetStepRepeat(stepId, repeat);
}
```

- [ ] **Step 4: Add optional player intents**

Modify `src/Core/Application/PlayerIntent.cs`:

```csharp
using RymoraLandOfHeroes.Core.Automation;
```

Append intents:

```csharp
public sealed record StartRecordingMacroIntent(string PartyId) : PlayerIntent;
public sealed record SaveRecordingMacroIntent(string PartyId, string Name) : PlayerIntent;
public sealed record CancelRecordingMacroIntent(string PartyId) : PlayerIntent;
public sealed record RecordGatherActionIntent(string PartyId, TilePosition Target, MacroActionKind Kind, string ItemName, int ItemLevel, float ItemWeight) : PlayerIntent;
public sealed record PlayProgramIntent(string PartyId) : PlayerIntent;
public sealed record PauseProgramIntent(string PartyId) : PlayerIntent;
public sealed record StopProgramIntent(string PartyId) : PlayerIntent;
```

Update `HandleInput` to route these intents to the new methods.

- [ ] **Step 5: Run tests and verify they pass**

Run:

```powershell
dotnet test src/Tests/RymoraLandOfHeroes.Core.Tests.csproj --no-restore --filter FullyQualifiedName~GameApplicationAutomationTests
```

Expected: Application automation tests pass.

- [ ] **Step 6: Commit**

```powershell
git add src/Core/Application/GameApplication.cs src/Core/Application/PlayerIntent.cs src/Tests/Application/GameApplicationAutomationTests.cs
git commit -m "feat: add automation application commands"
```

### Task 7: Godot Recording Input And Compact HUD Controls

**Files:**
- Modify: `src/Godot/Bootstrap/Bootstrap.cs`
- Modify: `src/Godot/Presentation/HudPresenter.cs`

- [ ] **Step 1: Build and observe current compile baseline**

Run:

```powershell
dotnet build RymoraLandOfHeroes.sln --no-restore
```

Expected: build succeeds before UI edits.

- [ ] **Step 2: Add Play/Pause/Stop and Macro status text to HudPresenter**

Modify `HudPresenter.Sync` text block to include runner state and next action:

```csharp
var runner = party.Automation.Runner;
_label.Text = string.Join(
    "\n",
    $"Party: {party.Id}",
    $"Screen: {application.UI.CurrentScreen}",
    $"Position: ({party.Position.X}, {party.Position.Y})",
    $"Program: {runner.State}{(runner.ErrorMessage is null ? string.Empty : $" - {runner.ErrorMessage}")}",
    $"Action: {FormatAction(party.ActionQueue.Current)}",
    $"Next: {FormatMacroAction(runner.NextAction)}",
    $"Queue: {party.ActionQueue.PendingCount} pending",
    $"Inventory: {FormatInventory(party.Inventory)}");
```

Add helper:

```csharp
private static string FormatMacroAction(RymoraLandOfHeroes.Core.Automation.MacroAction? action)
{
    return action switch
    {
        null => "none",
        RymoraLandOfHeroes.Core.Automation.MoveToMacroAction move => $"MoveTo ({move.Destination.X}, {move.Destination.Y})",
        RymoraLandOfHeroes.Core.Automation.GatherMacroAction gather => gather.Kind.ToString(),
        _ => action.Kind.ToString()
    };
}
```

- [ ] **Step 3: Route context menu to Record Macro when recording is active**

Add `using RymoraLandOfHeroes.Core.Automation;` to `Bootstrap.cs`.

In `OnContextMenuIdPressed`, before execution logic, route active recording:

```csharp
var party = _application!.Parties.Get(BootstrapCoreFactory.PartyId);
if (party.Automation.Recording is not null)
{
    RecordContextAction((ContextAction)id);
    return;
}
```

Add method:

```csharp
private void RecordContextAction(ContextAction action)
{
    if (_application is null || _worldAdapter is null || _content is null)
    {
        return;
    }

    switch (action)
    {
        case ContextAction.Move:
            _application.RecordMoveAction(BootstrapCoreFactory.PartyId, _contextMenuTarget);
            GD.Print($"Recorded MoveTo ({_contextMenuTarget.X}, {_contextMenuTarget.Y}).");
            break;
        case ContextAction.Mine:
            RecordGatherAction(MacroActionKind.Mine, PartyActionType.Mine);
            break;
        case ContextAction.CutWood:
            RecordGatherAction(MacroActionKind.CutWood, PartyActionType.CutWood);
            break;
    }
}

private void RecordGatherAction(MacroActionKind macroKind, PartyActionType partyActionType)
{
    if (_application is null || _worldAdapter is null || _content is null)
    {
        return;
    }

    var material = _worldAdapter.GetMaterialForAction(_contextMenuTarget, partyActionType, _content.Materials);
    if (material is null)
    {
        GD.Print($"Cannot record {macroKind}. Tile has no material.");
        return;
    }

    _application.RecordGatherAction(
        BootstrapCoreFactory.PartyId,
        _contextMenuTarget,
        macroKind,
        material.Name,
        material.Level,
        material.Weight);
    GD.Print($"Recorded {macroKind} at ({_contextMenuTarget.X}, {_contextMenuTarget.Y}).");
}
```

- [ ] **Step 4: Prevent left-click travel while recording**

In `_UnhandledInput`, after loading the selected party and before left-click travel, return early when Record Macro is active:

```csharp
var party = _application.Parties.Get(BootstrapCoreFactory.PartyId);
if (party.Automation.Recording is not null && mouseButton.ButtonIndex == MouseButton.Left)
{
    return;
}
```

- [ ] **Step 5: Build and run targeted smoke**

Run:

```powershell
dotnet build RymoraLandOfHeroes.sln --no-restore
```

Expected: build succeeds.

Run the Godot smoke command used by this project:

```powershell
& "C:\Users\rmttyszka\AppData\Local\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe" --headless --path "." --quit-after 1
```

Expected output includes `Rymora Godot bootstrap ready`.

- [ ] **Step 6: Commit**

```powershell
git add src/Godot/Bootstrap/Bootstrap.cs src/Godot/Presentation/HudPresenter.cs
git commit -m "feat: route map input to macro recording"
```

### Task 8: Macros Tab, Program List And Editors

**Files:**
- Create: `src/Godot/Presentation/MacrosPresenter.cs`
- Create: `src/Godot/Presentation/MacroEditorPresenter.cs`
- Create: `src/Godot/Presentation/ProgramEditorPresenter.cs`
- Modify: `src/Godot/Bootstrap/Bootstrap.cs`
- Modify: `src/Godot/Presentation/HudPresenter.cs`
- Modify: `scenes/bootstrap.tscn`

- [ ] **Step 1: Create MacrosPresenter**

Create `src/Godot/Presentation/MacrosPresenter.cs`:

```csharp
using Godot;
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.GodotAdapter.Presentation;

public partial class MacrosPresenter : VBoxContainer
{
    private GameApplication? _application;
    private Party? _party;
    private ItemList? _macros;
    private ItemList? _program;
    private LineEdit? _macroName;
    private MacroEditorPresenter? _macroEditor;
    private ProgramEditorPresenter? _programEditor;
    private readonly List<string> _macroIds = new();
    private readonly List<string> _programStepIds = new();
    private int? _draggedMacroIndex;
    private int? _draggedProgramIndex;

    public override void _Ready()
    {
        _macroName = GetNode<LineEdit>("MacroName");
        _macros = GetNode<ItemList>("MacroList");
        _program = GetNode<ItemList>("ProgramList");
        _macroEditor = GetNode<MacroEditorPresenter>("../../../../MacroEditor");
        _programEditor = GetNode<ProgramEditorPresenter>("../../../../ProgramEditor");
        _macros.GuiInput += OnMacroListGuiInput;
        _program.GuiInput += OnProgramListGuiInput;
        GetNode<Button>("RecordButton").Pressed += OnRecordPressed;
        GetNode<Button>("SaveButton").Pressed += OnSavePressed;
        GetNode<Button>("CancelButton").Pressed += OnCancelPressed;
        GetNode<Button>("AddToProgramButton").Pressed += OnAddToProgramPressed;
        GetNode<Button>("EditMacroButton").Pressed += OnEditMacroPressed;
        GetNode<Button>("EditProgramButton").Pressed += OnEditProgramPressed;
    }

    public void Bind(GameApplication application, Party party)
    {
        _application = application;
        _party = party;
        Refresh();
    }

    public void Refresh()
    {
        if (_party is null || _macros is null || _program is null)
        {
            return;
        }

        _macros.Clear();
        _macroIds.Clear();
        foreach (var macro in _party.Automation.Macros)
        {
            _macroIds.Add(macro.Id);
            _macros.AddItem(macro.Name);
        }

        _program.Clear();
        _programStepIds.Clear();
        foreach (var step in _party.Automation.Program.Steps)
        {
            var macro = _party.Automation.GetMacro(step.MacroId);
            _programStepIds.Add(step.Id);
            _program.AddItem($"{macro.Name} - {step.Repeat.Mode}");
        }
    }

    private void OnRecordPressed()
    {
        if (_application is null || _party is null)
        {
            return;
        }

        _application.StartRecordingMacro(_party.Id);
        Refresh();
    }

    private void OnSavePressed()
    {
        if (_application is null || _party is null || _macroName is null)
        {
            return;
        }

        _application.SaveRecordingMacro(_party.Id, _macroName.Text);
        _macroName.Text = string.Empty;
        Refresh();
    }

    private void OnCancelPressed()
    {
        if (_application is null || _party is null)
        {
            return;
        }

        _application.CancelRecordingMacro(_party.Id);
        Refresh();
    }

    private void OnAddToProgramPressed()
    {
        if (_application is null || _party is null || _macros is null)
        {
            return;
        }

        var selected = _macros.GetSelectedItems();
        if (selected.Length == 0)
        {
            return;
        }

        AddMacroToProgram(selected[0]);
        Refresh();
    }

    private void OnEditMacroPressed()
    {
        if (_application is null || _party is null || _macros is null || _macroEditor is null)
        {
            return;
        }

        var selected = _macros.GetSelectedItems();
        if (selected.Length == 0 || selected[0] >= _macroIds.Count)
        {
            return;
        }

        _macroEditor.Open(_application, _party, _macroIds[selected[0]]);
    }

    private void OnEditProgramPressed()
    {
        if (_application is null || _party is null || _programEditor is null)
        {
            return;
        }

        _programEditor.Open(_application, _party);
    }

    private void OnMacroListGuiInput(InputEvent inputEvent)
    {
        if (_macros is null || inputEvent is not InputEventMouseButton mouse || mouse.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        if (mouse.Pressed)
        {
            var index = _macros.GetItemAtPosition(mouse.Position, exact: true);
            _draggedMacroIndex = index >= 0 ? index : null;
        }
    }

    private void OnProgramListGuiInput(InputEvent inputEvent)
    {
        if (_program is null || inputEvent is not InputEventMouseButton mouse || mouse.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        var index = _program.GetItemAtPosition(mouse.Position, exact: false);
        if (mouse.Pressed)
        {
            _draggedProgramIndex = index >= 0 ? index : null;
            return;
        }

        if (_draggedMacroIndex is not null)
        {
            AddMacroToProgram(_draggedMacroIndex.Value);
            _draggedMacroIndex = null;
            Refresh();
            return;
        }

        if (_draggedProgramIndex is not null && index >= 0)
        {
            MoveProgramStep(_draggedProgramIndex.Value, index);
            _draggedProgramIndex = null;
            Refresh();
        }
    }

    private void AddMacroToProgram(int macroIndex)
    {
        if (_application is null || _party is null || macroIndex < 0 || macroIndex >= _macroIds.Count)
        {
            return;
        }

        _application.AddMacroToProgram(_party.Id, _macroIds[macroIndex], RepeatPolicy.Once);
    }

    private void MoveProgramStep(int fromIndex, int toIndex)
    {
        if (_application is null || _party is null || fromIndex < 0 || fromIndex >= _programStepIds.Count || toIndex < 0 || toIndex >= _programStepIds.Count)
        {
            return;
        }

        _application.MoveProgramStep(_party.Id, _programStepIds[fromIndex], toIndex);
    }
}
```

- [ ] **Step 2: Create editor presenters**

Create `src/Godot/Presentation/MacroEditorPresenter.cs`:

```csharp
using Godot;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.GodotAdapter.Presentation;

public partial class MacroEditorPresenter : Window
{
    private GameApplication? _application;
    private Party? _party;
    private string? _macroId;
    private ItemList? _actions;
    private OptionButton? _repeatMode;
    private LineEdit? _repeatValue;
    private readonly List<string> _actionIds = new();

    public override void _Ready()
    {
        _actions = GetNode<ItemList>("Panel/Actions");
        _repeatMode = GetNode<OptionButton>("Panel/RepeatMode");
        _repeatValue = GetNode<LineEdit>("Panel/RepeatValue");
        _repeatMode.AddItem("Once", (int)RepeatMode.Once);
        _repeatMode.AddItem("Forever", (int)RepeatMode.Forever);
        _repeatMode.AddItem("Count", (int)RepeatMode.Count);
        _repeatMode.AddItem("Duration", (int)RepeatMode.Duration);
        GetNode<Button>("Panel/RemoveActionButton").Pressed += OnRemoveActionPressed;
        GetNode<Button>("Panel/MoveActionUpButton").Pressed += OnMoveActionUpPressed;
        GetNode<Button>("Panel/MoveActionDownButton").Pressed += OnMoveActionDownPressed;
        GetNode<Button>("Panel/ApplyRepeatButton").Pressed += OnApplyRepeatPressed;
    }

    public void Open(GameApplication application, Party party, string macroId)
    {
        _application = application;
        _party = party;
        _macroId = macroId;
        Title = $"Macro: {party.Automation.GetMacro(macroId).Name}";
        Refresh();
        PopupCenteredRatio(0.75f);
    }

    private void Refresh()
    {
        if (_party is null || _macroId is null || _actions is null)
        {
            return;
        }

        _actions.Clear();
        _actionIds.Clear();
        foreach (var action in _party.Automation.GetMacro(_macroId).Actions)
        {
            _actionIds.Add(action.Id);
            _actions.AddItem(FormatAction(action));
        }
    }

    private void OnRemoveActionPressed()
    {
        var selected = GetSelectedActionIndex();
        if (_application is null || _party is null || _macroId is null || selected is null)
        {
            return;
        }

        _application.RemoveMacroAction(_party.Id, _macroId, _actionIds[selected.Value]);
        Refresh();
    }

    private void OnMoveActionUpPressed()
    {
        MoveSelectedAction(-1);
    }

    private void OnMoveActionDownPressed()
    {
        MoveSelectedAction(1);
    }

    private void OnApplyRepeatPressed()
    {
        var selected = GetSelectedActionIndex();
        if (_application is null || _party is null || _macroId is null || selected is null)
        {
            return;
        }

        var action = _party.Automation.GetMacro(_macroId).Actions[selected.Value];
        if (action is not GatherMacroAction)
        {
            return;
        }

        _application.SetGatherActionRepeat(_party.Id, _macroId, action.Id, BuildRepeatPolicy());
        Refresh();
    }

    private void MoveSelectedAction(int offset)
    {
        var selected = GetSelectedActionIndex();
        if (_application is null || _party is null || _macroId is null || selected is null)
        {
            return;
        }

        var target = selected.Value + offset;
        if (target < 0 || target >= _actionIds.Count)
        {
            return;
        }

        _application.MoveMacroAction(_party.Id, _macroId, _actionIds[selected.Value], target);
        Refresh();
    }

    private int? GetSelectedActionIndex()
    {
        var selected = _actions?.GetSelectedItems() ?? Array.Empty<int>();
        return selected.Length == 0 ? null : selected[0];
    }

    private RepeatPolicy BuildRepeatPolicy()
    {
        var mode = (RepeatMode)(_repeatMode?.GetSelectedId() ?? (int)RepeatMode.Once);
        return mode switch
        {
            RepeatMode.Once => RepeatPolicy.Once,
            RepeatMode.Forever => RepeatPolicy.Forever,
            RepeatMode.Count => RepeatPolicy.Count(int.Parse(_repeatValue?.Text ?? "1")),
            RepeatMode.Duration => RepeatPolicy.Duration(float.Parse(_repeatValue?.Text ?? "1")),
            _ => RepeatPolicy.Once
        };
    }

    private static string FormatAction(MacroAction action)
    {
        return action switch
        {
            MoveToMacroAction move => $"MoveTo ({move.Destination.X}, {move.Destination.Y})",
            GatherMacroAction gather => $"{gather.Kind} - {gather.Repeat.Mode}",
            _ => action.Kind.ToString()
        };
    }
}
```

Create `src/Godot/Presentation/ProgramEditorPresenter.cs`:

```csharp
using Godot;
using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.GodotAdapter.Presentation;

public partial class ProgramEditorPresenter : Window
{
    private GameApplication? _application;
    private Party? _party;
    private ItemList? _steps;
    private OptionButton? _repeatMode;
    private LineEdit? _repeatValue;
    private readonly List<string> _stepIds = new();

    public override void _Ready()
    {
        _steps = GetNode<ItemList>("Panel/Steps");
        _repeatMode = GetNode<OptionButton>("Panel/RepeatMode");
        _repeatValue = GetNode<LineEdit>("Panel/RepeatValue");
        _repeatMode.AddItem("Once", (int)RepeatMode.Once);
        _repeatMode.AddItem("Forever", (int)RepeatMode.Forever);
        _repeatMode.AddItem("Count", (int)RepeatMode.Count);
        _repeatMode.AddItem("Duration", (int)RepeatMode.Duration);
        GetNode<Button>("Panel/ApplyStepRepeatButton").Pressed += OnApplyStepRepeatPressed;
        GetNode<Button>("Panel/ApplyProgramRepeatButton").Pressed += OnApplyProgramRepeatPressed;
    }

    public void Open(GameApplication application, Party party)
    {
        _application = application;
        _party = party;
        Title = "Program";
        Refresh();
        PopupCenteredRatio(0.75f);
    }

    private void Refresh()
    {
        if (_party is null || _steps is null)
        {
            return;
        }

        _steps.Clear();
        _stepIds.Clear();
        foreach (var step in _party.Automation.Program.Steps)
        {
            var macro = _party.Automation.GetMacro(step.MacroId);
            _stepIds.Add(step.Id);
            _steps.AddItem($"{macro.Name} - {step.Repeat.Mode}");
        }
    }

    private void OnApplyStepRepeatPressed()
    {
        var selected = _steps?.GetSelectedItems() ?? Array.Empty<int>();
        if (_application is null || _party is null || selected.Length == 0)
        {
            return;
        }

        _application.SetProgramStepRepeat(_party.Id, _stepIds[selected[0]], BuildRepeatPolicy());
        Refresh();
    }

    private void OnApplyProgramRepeatPressed()
    {
        if (_application is null || _party is null)
        {
            return;
        }

        _application.SetProgramRepeat(_party.Id, BuildRepeatPolicy());
        Refresh();
    }

    private RepeatPolicy BuildRepeatPolicy()
    {
        var mode = (RepeatMode)(_repeatMode?.GetSelectedId() ?? (int)RepeatMode.Once);
        return mode switch
        {
            RepeatMode.Once => RepeatPolicy.Once,
            RepeatMode.Forever => RepeatPolicy.Forever,
            RepeatMode.Count => RepeatPolicy.Count(int.Parse(_repeatValue?.Text ?? "1")),
            RepeatMode.Duration => RepeatPolicy.Duration(float.Parse(_repeatValue?.Text ?? "1")),
            _ => RepeatPolicy.Once
        };
    }
}
```

- [ ] **Step 3: Add scene nodes**

Modify `scenes/bootstrap.tscn` by adding ext resources for the three scripts:

```text
[ext_resource type="Script" path="res://src/Godot/Presentation/MacrosPresenter.cs" id="10_macros_presenter"]
[ext_resource type="Script" path="res://src/Godot/Presentation/MacroEditorPresenter.cs" id="11_macro_editor"]
[ext_resource type="Script" path="res://src/Godot/Presentation/ProgramEditorPresenter.cs" id="12_program_editor"]
```

Replace the existing `UiLayer/Hud/Margin/Label` node with this structure:

```text
[node name="Tabs" type="TabContainer" parent="UiLayer/Hud/Margin"]
layout_mode = 2

[node name="Status" type="VBoxContainer" parent="UiLayer/Hud/Margin/Tabs"]
layout_mode = 2

[node name="Label" type="Label" parent="UiLayer/Hud/Margin/Tabs/Status"]
layout_mode = 2
text = "HUD"

[node name="PlayButton" type="Button" parent="UiLayer/Hud/Margin/Tabs/Status"]
layout_mode = 2
text = "Play"

[node name="PauseButton" type="Button" parent="UiLayer/Hud/Margin/Tabs/Status"]
layout_mode = 2
text = "Pause"

[node name="StopButton" type="Button" parent="UiLayer/Hud/Margin/Tabs/Status"]
layout_mode = 2
text = "Stop"
```

Under `Macros`, add:

```text
[node name="Macros" type="VBoxContainer" parent="UiLayer/Hud/Margin/Tabs"]
layout_mode = 2
script = ExtResource("10_macros_presenter")

[node name="MacroName" type="LineEdit" parent="UiLayer/Hud/Margin/Tabs/Macros"]
layout_mode = 2
placeholder_text = "Macro name"

[node name="RecordButton" type="Button" parent="UiLayer/Hud/Margin/Tabs/Macros"]
layout_mode = 2
text = "Record Macro"

[node name="SaveButton" type="Button" parent="UiLayer/Hud/Margin/Tabs/Macros"]
layout_mode = 2
text = "Save Macro"

[node name="CancelButton" type="Button" parent="UiLayer/Hud/Margin/Tabs/Macros"]
layout_mode = 2
text = "Cancel"

[node name="MacroList" type="ItemList" parent="UiLayer/Hud/Margin/Tabs/Macros"]
layout_mode = 2

[node name="AddToProgramButton" type="Button" parent="UiLayer/Hud/Margin/Tabs/Macros"]
layout_mode = 2
text = "Add to Program"

[node name="EditMacroButton" type="Button" parent="UiLayer/Hud/Margin/Tabs/Macros"]
layout_mode = 2
text = "Edit Macro"

[node name="EditProgramButton" type="Button" parent="UiLayer/Hud/Margin/Tabs/Macros"]
layout_mode = 2
text = "Edit Program"

[node name="ProgramList" type="ItemList" parent="UiLayer/Hud/Margin/Tabs/Macros"]
layout_mode = 2
```

Add editor windows under `UiLayer`:

```text
[node name="MacroEditor" type="Window" parent="UiLayer"]
visible = false
script = ExtResource("11_macro_editor")

[node name="Panel" type="VBoxContainer" parent="UiLayer/MacroEditor"]

[node name="Actions" type="ItemList" parent="UiLayer/MacroEditor/Panel"]

[node name="MoveActionUpButton" type="Button" parent="UiLayer/MacroEditor/Panel"]
text = "Move Up"

[node name="MoveActionDownButton" type="Button" parent="UiLayer/MacroEditor/Panel"]
text = "Move Down"

[node name="RemoveActionButton" type="Button" parent="UiLayer/MacroEditor/Panel"]
text = "Remove Action"

[node name="RepeatMode" type="OptionButton" parent="UiLayer/MacroEditor/Panel"]

[node name="RepeatValue" type="LineEdit" parent="UiLayer/MacroEditor/Panel"]
placeholder_text = "Count or seconds"

[node name="ApplyRepeatButton" type="Button" parent="UiLayer/MacroEditor/Panel"]
text = "Apply Action Repeat"

[node name="ProgramEditor" type="Window" parent="UiLayer"]
visible = false
script = ExtResource("12_program_editor")

[node name="Panel" type="VBoxContainer" parent="UiLayer/ProgramEditor"]

[node name="Steps" type="ItemList" parent="UiLayer/ProgramEditor/Panel"]

[node name="RepeatMode" type="OptionButton" parent="UiLayer/ProgramEditor/Panel"]

[node name="RepeatValue" type="LineEdit" parent="UiLayer/ProgramEditor/Panel"]
placeholder_text = "Count or seconds"

[node name="ApplyStepRepeatButton" type="Button" parent="UiLayer/ProgramEditor/Panel"]
text = "Apply Step Repeat"

[node name="ApplyProgramRepeatButton" type="Button" parent="UiLayer/ProgramEditor/Panel"]
text = "Apply Program Repeat"
```

Set `UiLayer.visible = true` so the panel is visible during development.

- [ ] **Step 4: Bind presenter in Bootstrap**

Add field:

```csharp
private MacrosPresenter? _macrosPresenter;
```

In `_Ready`:

```csharp
_macrosPresenter = GetNode<MacrosPresenter>("UiLayer/Hud/Margin/Tabs/Macros");
```

Modify `src/Godot/Presentation/HudPresenter.cs` so `_Ready` loads the moved status label:

```csharp
_label = GetNode<Label>("Margin/Tabs/Status/Label");
GetNode<Button>("Margin/Tabs/Status/PlayButton").Pressed += OnPlayPressed;
GetNode<Button>("Margin/Tabs/Status/PauseButton").Pressed += OnPausePressed;
GetNode<Button>("Margin/Tabs/Status/StopButton").Pressed += OnStopPressed;
```

Add fields to `HudPresenter`:

```csharp
private GameApplication? _application;
private Party? _party;
```

At the start of `Sync`, store the current application and party:

```csharp
_application = application;
_party = party;
```

Add button handlers:

```csharp
private void OnPlayPressed()
{
    if (_application is not null && _party is not null)
    {
        _application.PlayProgram(_party.Id);
    }
}

private void OnPausePressed()
{
    if (_application is not null && _party is not null)
    {
        _application.PauseProgram(_party.Id);
    }
}

private void OnStopPressed()
{
    if (_application is not null && _party is not null)
    {
        _application.StopProgram(_party.Id);
    }
}
```

In `_Process`, after `_hudPresenter.Sync`:

```csharp
_macrosPresenter?.Bind(_application, party);
```

Do not add keyboard shortcuts for Record/Save/Play in this task. The visible controls in the `Macros` tab are the supported UI path.

- [ ] **Step 5: Build and run smoke**

Run:

```powershell
dotnet build RymoraLandOfHeroes.sln --no-restore
```

Expected: build succeeds.

Run the Godot smoke command used by this project:

```powershell
& "C:\Users\rmttyszka\AppData\Local\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe" --headless --path "." --quit-after 1
```

Expected output includes `Rymora Godot bootstrap ready`.

- [ ] **Step 6: Commit**

```powershell
git add src/Godot/Presentation/MacrosPresenter.cs src/Godot/Presentation/MacroEditorPresenter.cs src/Godot/Presentation/ProgramEditorPresenter.cs src/Godot/Bootstrap/Bootstrap.cs src/Godot/Presentation/HudPresenter.cs scenes/bootstrap.tscn
git commit -m "feat: add macro automation panel"
```

### Task 9: Documentation And Full Verification

**Files:**
- Modify: `docs/arquitetura/party.md`
- Modify: `docs/arquitetura/ui.md`
- Modify: `docs/proximos-passos.md`

- [ ] **Step 1: Update architecture docs**

In `docs/arquitetura/party.md`, add a section after `## 3. Fila de acoes`:

```markdown
### 3.x Macros e Program

Macros sao blocos salvos por party. Uma Macro possui nome e acoes ordenadas. Na v1, as acoes gravaveis sao `MoveTo`, `Mine` e `CutWood`.

Cada party possui um Program ativo. O Program e uma sequencia linear de usos de Macro. Cada uso referencia a Macro salva por id e define sua propria repeticao. Editar uma Macro afeta o proximo inicio dessa Macro durante a execucao do Program.

O Runner do Program controla Play, Pause, Stop e Error. Pause e Stop sao cooperativos: a acao atual termina antes do estado mudar.
```

In `docs/arquitetura/ui.md`, replace the provisional click notes with:

```markdown
Na aba `Macros`, o jogador pode iniciar `Record Macro`, salvar a Macro com nome, ver Macros da party e montar o Program ativo. Durante Record Macro, o clique no mapa usa o menu contextual, mas grava acoes em vez de executar.
```

In `docs/proximos-passos.md`, keep save/load, TransferItem and Dungeon out of immediate scope.

- [ ] **Step 2: Run full tests**

Run:

```powershell
dotnet test RymoraLandOfHeroes.sln --no-restore
```

Expected: all tests pass.

- [ ] **Step 3: Run full build**

Run:

```powershell
dotnet build RymoraLandOfHeroes.sln --no-restore
```

Expected: build succeeds with 0 errors.

- [ ] **Step 4: Run diff check**

Run:

```powershell
git diff --check
```

Expected: no output.

- [ ] **Step 5: Commit docs and final polish**

```powershell
git add docs/arquitetura/party.md docs/arquitetura/ui.md docs/proximos-passos.md
git commit -m "docs: update macro automation architecture"
```

---

## Final Verification Checklist

Run these commands after all tasks:

```powershell
dotnet test RymoraLandOfHeroes.sln --no-restore
dotnet build RymoraLandOfHeroes.sln --no-restore
git diff --check
git status --short
```

Expected results:

- Tests report all tests passed.
- Build reports 0 errors.
- `git diff --check` prints no output.
- `git status --short` shows only intended uncommitted files, or no output if everything was committed.

## Scope Guardrails

Do not implement in this plan:

- save/load persistence for Macros or Program;
- `TransferItem` recording;
- advanced item-count conditions;
- Program groups/sub-blocks;
- `Dungeon` or `EnterDungeon`.

These are intentionally outside the current implementation slice.
