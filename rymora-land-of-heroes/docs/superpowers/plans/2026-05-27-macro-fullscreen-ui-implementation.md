# Macro Full-Screen UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move Macro editing out of the cramped HUD tab into a full-screen scrollable modal opened from a left icon menu.

**Architecture:** Keep Core unchanged. Godot owns modal visibility, the left menu button, and HUD visibility. Existing presenter commands still call `GameApplication`; presenter inheritance changes only adapt UI layout.

**Tech Stack:** Godot 4.6, C#/.NET 8, existing Godot C# presenters, xUnit Core tests.

---

## File Map

- Modify `scenes/bootstrap.tscn`: remove `Macros` from HUD tabs, add left menu rail, add full-screen `MacrosModal` with scrollable content, move Macro/Program editor panels inside modal.
- Modify `src/Godot/Presentation/HudPresenter.cs`: read compact status nodes from the new HUD-only layout.
- Modify `src/Godot/Bootstrap/Bootstrap.cs`: wire left `MacrosButton`; hide HUD while modal is open; show HUD again when modal closes; bind the modal presenter.
- Modify `src/Godot/Presentation/MacrosPresenter.cs`: become the modal presenter, handle `CloseButton`, use nested node paths, bind embedded editors.
- Modify `src/Godot/Presentation/MacroEditorPresenter.cs`: convert from `Window` to embedded `PanelContainer`; `Open` shows the panel instead of popping a window.
- Modify `src/Godot/Presentation/ProgramEditorPresenter.cs`: convert from `Window` to embedded `PanelContainer`; `Open` shows the panel instead of popping a window.
- Modify `docs/arquitetura/ui.md`: document left icon menu, full-screen Macro modal, HUD hide/show behavior, and embedded editors.

---

## Task 1: Scene Shell And HUD Paths

**Files:**
- Modify: `scenes/bootstrap.tscn`
- Modify: `src/Godot/Presentation/HudPresenter.cs`
- Modify: `src/Godot/Bootstrap/Bootstrap.cs`

- [ ] **Step 1: Replace HUD tabs with compact status rows**

In `scenes/bootstrap.tscn`, replace the `Tabs`, `Status`, and `Macros` children under `UiLayer/Hud/Margin` with:

```text
[node name="Rows" type="VBoxContainer" parent="UiLayer/Hud/Margin"]
layout_mode = 2

[node name="Label" type="Label" parent="UiLayer/Hud/Margin/Rows"]
layout_mode = 2
text = "HUD"

[node name="Controls" type="HBoxContainer" parent="UiLayer/Hud/Margin/Rows"]
layout_mode = 2

[node name="PlayButton" type="Button" parent="UiLayer/Hud/Margin/Rows/Controls"]
layout_mode = 2
text = "Play"

[node name="PauseButton" type="Button" parent="UiLayer/Hud/Margin/Rows/Controls"]
layout_mode = 2
text = "Pause"

[node name="StopButton" type="Button" parent="UiLayer/Hud/Margin/Rows/Controls"]
layout_mode = 2
text = "Stop"
```

- [ ] **Step 2: Add left menu rail**

In `scenes/bootstrap.tscn`, under `UiLayer`, add:

```text
[node name="MenuRail" type="PanelContainer" parent="UiLayer"]
offset_left = 8.0
offset_top = 12.0
offset_right = 56.0
offset_bottom = 80.0

[node name="Rows" type="VBoxContainer" parent="UiLayer/MenuRail"]
layout_mode = 2

[node name="MacrosButton" type="Button" parent="UiLayer/MenuRail/Rows"]
layout_mode = 2
text = "M"
tooltip_text = "Macros"
```

Move `UiLayer/Hud` to the right of the rail:

```text
offset_left = 68.0
offset_top = 12.0
offset_right = 388.0
offset_bottom = 180.0
```

- [ ] **Step 3: Add full-screen Macros modal shell**

In `scenes/bootstrap.tscn`, add `MacrosModal` under `UiLayer` with `visible = false`, script `ExtResource("10_macros_presenter")`, anchors full rect, and this structure:

```text
[node name="MacrosModal" type="PanelContainer" parent="UiLayer"]
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
offset_left = 72.0
offset_top = 24.0
offset_right = -24.0
offset_bottom = -24.0
visible = false
script = ExtResource("10_macros_presenter")

[node name="Margin" type="MarginContainer" parent="UiLayer/MacrosModal"]
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
theme_override_constants/margin_left = 12
theme_override_constants/margin_top = 12
theme_override_constants/margin_right = 12
theme_override_constants/margin_bottom = 12

[node name="Rows" type="VBoxContainer" parent="UiLayer/MacrosModal/Margin"]
layout_mode = 2

[node name="Header" type="HBoxContainer" parent="UiLayer/MacrosModal/Margin/Rows"]
layout_mode = 2

[node name="Title" type="Label" parent="UiLayer/MacrosModal/Margin/Rows/Header"]
layout_mode = 2
text = "Macros"

[node name="CloseButton" type="Button" parent="UiLayer/MacrosModal/Margin/Rows/Header"]
layout_mode = 2
text = "Close"

[node name="Scroll" type="ScrollContainer" parent="UiLayer/MacrosModal/Margin/Rows"]
layout_mode = 2
size_flags_vertical = 3

[node name="Content" type="VBoxContainer" parent="UiLayer/MacrosModal/Margin/Rows/Scroll"]
layout_mode = 2
```

Move all Macro controls from the old HUD tab under `UiLayer/MacrosModal/Margin/Rows/Scroll/Content`.

- [ ] **Step 4: Update `HudPresenter` node paths**

Change `_Ready` in `src/Godot/Presentation/HudPresenter.cs` to:

```csharp
_label = GetNodeOrNull<Label>("Margin/Rows/Label");
_playButton = GetNodeOrNull<Button>("Margin/Rows/Controls/PlayButton");
_pauseButton = GetNodeOrNull<Button>("Margin/Rows/Controls/PauseButton");
_stopButton = GetNodeOrNull<Button>("Margin/Rows/Controls/StopButton");
```

- [ ] **Step 5: Update `Bootstrap` bindings**

In `src/Godot/Bootstrap/Bootstrap.cs`, change `_macrosPresenter` lookup to:

```csharp
_macrosPresenter = GetNodeOrNull<MacrosPresenter>("UiLayer/MacrosModal");
```

Add a field:

```csharp
private Button? _macrosButton;
```

In `_Ready`, get and wire the button:

```csharp
_macrosButton = GetNodeOrNull<Button>("UiLayer/MenuRail/Rows/MacrosButton");
if (_macrosButton is not null)
{
    _macrosButton.Pressed += OpenMacrosModal;
}
if (_macrosPresenter is not null)
{
    _macrosPresenter.Closed += OnMacrosModalClosed;
}
```

Add methods:

```csharp
private void OpenMacrosModal()
{
    _hudPresenter?.Hide();
    _macrosPresenter?.Show();
}

private void OnMacrosModalClosed()
{
    _hudPresenter?.Show();
}
```

- [ ] **Step 6: Build**

Run:

```powershell
dotnet build RymoraLandOfHeroes.sln --no-restore
```

Expected: build succeeds with 0 errors.

- [ ] **Step 7: Commit**

```powershell
git add scenes/bootstrap.tscn src/Godot/Presentation/HudPresenter.cs src/Godot/Bootstrap/Bootstrap.cs
git commit -m "feat: add fullscreen macro modal shell"
```

---

## Task 2: Embedded Macro And Program Presenters

**Files:**
- Modify: `src/Godot/Presentation/MacrosPresenter.cs`
- Modify: `src/Godot/Presentation/MacroEditorPresenter.cs`
- Modify: `src/Godot/Presentation/ProgramEditorPresenter.cs`
- Modify: `scenes/bootstrap.tscn`

- [ ] **Step 1: Update `MacrosPresenter` paths and close event**

In `src/Godot/Presentation/MacrosPresenter.cs`, add:

```csharp
public event Action? Closed;
```

Add field:

```csharp
private Button? _closeButton;
```

Update `_Ready` to read modal paths:

```csharp
_closeButton = GetNodeOrNull<Button>("Margin/Rows/Header/CloseButton");
_macroName = GetNodeOrNull<LineEdit>("Margin/Rows/Scroll/Content/MacroName");
_recordButton = GetNodeOrNull<Button>("Margin/Rows/Scroll/Content/RecordButton");
_saveButton = GetNodeOrNull<Button>("Margin/Rows/Scroll/Content/SaveButton");
_cancelButton = GetNodeOrNull<Button>("Margin/Rows/Scroll/Content/CancelButton");
_macroList = GetNodeOrNull<ItemList>("Margin/Rows/Scroll/Content/MacroList");
_addToProgramButton = GetNodeOrNull<Button>("Margin/Rows/Scroll/Content/AddToProgramButton");
_editMacroButton = GetNodeOrNull<Button>("Margin/Rows/Scroll/Content/EditMacroButton");
_editProgramButton = GetNodeOrNull<Button>("Margin/Rows/Scroll/Content/EditProgramButton");
_programList = GetNodeOrNull<ItemList>("Margin/Rows/Scroll/Content/ProgramList");
_moveStepUpButton = GetNodeOrNull<Button>("Margin/Rows/Scroll/Content/MoveStepUpButton");
_moveStepDownButton = GetNodeOrNull<Button>("Margin/Rows/Scroll/Content/MoveStepDownButton");
_removeStepButton = GetNodeOrNull<Button>("Margin/Rows/Scroll/Content/RemoveStepButton");
_stepRepeatMode = GetNodeOrNull<OptionButton>("Margin/Rows/Scroll/Content/StepRepeatMode");
_stepRepeatValue = GetNodeOrNull<LineEdit>("Margin/Rows/Scroll/Content/StepRepeat");
_setStepRepeatButton = GetNodeOrNull<Button>("Margin/Rows/Scroll/Content/SetStepRepeatButton");
_activeProgramLabel = GetNodeOrNull<Label>("Margin/Rows/Scroll/Content/ActiveProgramLabel");
_macroEditor = GetNodeOrNull<MacroEditorPresenter>("Margin/Rows/Scroll/Content/Editors/MacroEditor");
_programEditor = GetNodeOrNull<ProgramEditorPresenter>("Margin/Rows/Scroll/Content/Editors/ProgramEditor");
```

Wire close:

```csharp
if (_closeButton is not null) _closeButton.Pressed += OnClosePressed;
```

Add method:

```csharp
private void OnClosePressed()
{
    Hide();
    Closed?.Invoke();
}
```

- [ ] **Step 2: Convert `MacroEditorPresenter` to embedded panel**

Change class declaration in `src/Godot/Presentation/MacroEditorPresenter.cs`:

```csharp
public partial class MacroEditorPresenter : PanelContainer
```

Remove `CloseRequested += Hide;` from `_Ready`.

Change `Open` to:

```csharp
public void Open(string macroId)
{
    _macroId = macroId;
    Show();
    Refresh();
}
```

- [ ] **Step 3: Convert `ProgramEditorPresenter` to embedded panel**

Change class declaration in `src/Godot/Presentation/ProgramEditorPresenter.cs`:

```csharp
public partial class ProgramEditorPresenter : PanelContainer
```

Remove `CloseRequested += Hide;` from `_Ready`.

Change `Open` to:

```csharp
public void Open()
{
    Show();
    Refresh();
}
```

- [ ] **Step 4: Move editor nodes inside modal scene**

In `scenes/bootstrap.tscn`, remove root `UiLayer/MacroEditor` and `UiLayer/ProgramEditor` window nodes.

Under `UiLayer/MacrosModal/Margin/Rows/Scroll/Content`, add:

```text
[node name="Editors" type="HBoxContainer" parent="UiLayer/MacrosModal/Margin/Rows/Scroll/Content"]
layout_mode = 2

[node name="MacroEditor" type="PanelContainer" parent="UiLayer/MacrosModal/Margin/Rows/Scroll/Content/Editors"]
visible = false
script = ExtResource("11_macro_editor")

[node name="ProgramEditor" type="PanelContainer" parent="UiLayer/MacrosModal/Margin/Rows/Scroll/Content/Editors"]
visible = false
script = ExtResource("12_program_editor")
```

Move each editor's existing `Margin/Rows/...` children under the matching embedded panel.

- [ ] **Step 5: Build and tests**

Run:

```powershell
dotnet build RymoraLandOfHeroes.sln --no-restore
dotnet test RymoraLandOfHeroes.sln --no-restore
```

Expected: build succeeds, tests pass.

- [ ] **Step 6: Commit**

```powershell
git add src/Godot/Presentation/MacrosPresenter.cs src/Godot/Presentation/MacroEditorPresenter.cs src/Godot/Presentation/ProgramEditorPresenter.cs scenes/bootstrap.tscn
git commit -m "feat: embed macro editors in fullscreen modal"
```

---

## Task 3: Documentation And Final Verification

**Files:**
- Modify: `docs/arquitetura/ui.md`

- [ ] **Step 1: Update UI architecture docs**

In `docs/arquitetura/ui.md`, update `## 3. Painels` implementation notes to say:

```markdown
Implementacao atual:
- `HudPresenter` mostra status compacto da party selecionada, tela atual, posicao, acao atual, proxima acao de Macro, erro, fila pendente, inventario e botoes `Play`, `Pause`, `Stop`.
- `MenuRail` fica no canto esquerdo com botoes compactos de menu. Nesta etapa, existe apenas `Macros`.
- Clicar `Macros` abre o modal full-screen `MacrosModal` e esconde o HUD/status compacto.
- Fechar `MacrosModal` mostra o HUD/status compacto novamente.
- `MacrosModal` possui scroll e concentra gravacao, lista de Macros, Program ativo e editores internos de Macro e Program.
- `Bootstrap` sincroniza HUD e modal a cada `_Process` usando estado do Core.
```

Update `Fluxo de Macros` to remove old references to a HUD tab and say:

```markdown
- O menu esquerdo `Macros` abre o modal full-screen de Macros.
- O modal lista Macros salvos, adiciona Macros ao Program ativo e mostra editores internos para ordenar/remover acoes e ajustar repeticoes.
```

- [ ] **Step 2: Run full verification**

Run:

```powershell
dotnet test RymoraLandOfHeroes.sln --no-restore
dotnet build RymoraLandOfHeroes.sln --no-restore
git diff --check
git status --short
```

Expected:

- tests pass;
- build succeeds with 0 errors;
- `git diff --check` prints no output;
- status shows only intended doc changes before commit.

- [ ] **Step 3: Check Godot executable availability**

Run:

```powershell
Test-Path -LiteralPath "C:\Users\rmttyszka\AppData\Local\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe"
```

If `True`, run:

```powershell
& "C:\Users\rmttyszka\AppData\Local\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe" --headless --path "." --quit-after 1
```

Expected output includes `Rymora Godot bootstrap ready`. If `False`, report smoke unavailable.

- [ ] **Step 4: Commit**

```powershell
git add docs/arquitetura/ui.md
git commit -m "docs: update fullscreen macro ui"
```

---

## Final Verification Checklist

Run after all tasks:

```powershell
dotnet test RymoraLandOfHeroes.sln --no-restore
dotnet build RymoraLandOfHeroes.sln --no-restore
git diff --check
git status --short
```

Expected:

- tests pass;
- build succeeds;
- diff check has no output;
- worktree is clean after commits.

## Scope Guardrails

Do not implement:

- new left menu items besides `Macros`;
- save/load persistence;
- `TransferItem` in Macros;
- advanced conditions;
- Program groups/sub-blocks;
- Dungeon/EnterDungeon.
