# Design Spec: Documentation Foundation And Core Refactor

Date: 2026-04-23
Project: Rymora Idle
Status: Approved in chat, pending written-spec review

## 1. Context

The current project already has a playable prototype loop in Unity:

- world exploration on a tilemap;
- party selection;
- queued map actions;
- gathering;
- random encounters;
- automatic cooldown-based combat;
- runtime content loaded from scene objects and `Resources`.

The current documentation in `DOCUMENTACAO_JOGO.md` is a strong operational snapshot of how the game behaves today, but it mixes two different concerns:

- stable architectural knowledge that should remain useful across future chats;
- volatile scene/content details that can drift as assets and scene wiring change.

At the same time, the runtime architecture currently concentrates too much responsibility in a few Unity managers and scene-bound `MonoBehaviour` classes, especially:

- `GameManager`
- `PartyManager`
- `CombatManager`
- `MapManager`
- `UIManager`
- `Heroes.Party`

This creates three problems:

1. gameplay rules are tightly coupled to Unity scene objects and visual state;
2. system ownership is unclear, so behavior is spread across unrelated managers;
3. future refactors risk mixing behavior changes with architectural cleanup.

This spec defines:

- the new documentation foundation for future chats;
- the target architecture for a behavior-preserving refactor;
- the migration strategy and boundaries for future implementation plans.

## 2. Goals

### 2.1 Primary goals

- Create a documentation structure that is reliable as long-term context for future implementation chats.
- Preserve current gameplay behavior while reorganizing architecture.
- Move core game rules toward pure C# domain code with Unity used as an adapter layer.
- Reduce responsibility overlap between scene managers.
- Make future feature work easier without implementing new features during the refactor itself.

### 2.2 Non-goals

- Do not add new gameplay features during the refactor.
- Do not redesign balance, combat formulas, encounter rules, or map rules unless needed to preserve current behavior.
- Do not replace Unity scene/UI workflows all at once.
- Do not attempt a full-system big-bang rewrite in a single implementation pass.

## 3. Key decisions

The user-approved design decisions are:

- Refactor must preserve current external behavior.
- Documentation foundation will be split into two documents.
- First refactor wave may be architecturally deep.
- The architecture target is `Core puro + adaptadores Unity`.
- Migration should be incremental, not big-bang.

## 4. Documentation foundation

### 4.1 New documentation model

The project should stop using a single long file as both architecture guide and runtime snapshot.

Instead, the documentation foundation will be split into:

1. `docs/arquitetura-estavel.md`
2. `docs/estado-atual.md`

`DOCUMENTACAO_JOGO.md` should become a short index that explains the split and points readers to the two canonical files.

### 4.2 Document purposes

### `docs/arquitetura-estavel.md`

Purpose:

- capture concepts that should remain valid across scene/content changes;
- define system boundaries, ownership, contracts, and allowed dependencies;
- explain the gameplay flow at the architectural level instead of asset-instance level.

It should contain:

- game overview;
- module boundaries;
- ownership by subsystem;
- main gameplay flows;
- state ownership rules;
- event and dependency rules;
- terminology and naming rules;
- refactor guardrails.

### `docs/estado-atual.md`

Purpose:

- capture the validated snapshot of the current playable project state;
- describe what is actually wired in the scene and content as of a specific date/commit;
- make temporary limitations explicit without pretending they are architectural truths.

It should contain:

- validation date;
- validated commit hash;
- main scene and central objects;
- connected assets and content actually in use;
- behavior confirmed in code/scene;
- known placeholders, gaps, and inconsistencies.

### 4.3 Documentation rules

To keep future chats reliable, both documents must follow these rules:

- Every volatile statement must be tied to a validation date and preferably a commit.
- Scene-specific details belong in `estado-atual`, not `arquitetura-estavel`.
- Architectural responsibilities must be described in terms of ownership, not just behavior.
- Known quirks that affect refactor safety must be called out explicitly.
- Future chats should treat `arquitetura-estavel` as the preferred source for design intent and `estado-atual` as the preferred source for current reality.

## 5. Target architecture

### 5.1 High-level shape

The target architecture is:

- `Core`: pure C# rules and state, with no Unity dependency;
- `Unity adapters`: `MonoBehaviour` classes that translate scene/input/UI/assets into the core model and reflect core state back into the scene.

The core owns game rules.
Unity owns presentation, scene lifecycle, input plumbing, and asset wiring.

### 5.2 Core domains

The core should be organized around these bounded contexts:

### `Application`

Responsibilities:

- bootstrap orchestration at the use-case level;
- game screen state;
- cross-domain coordination rules;
- top-level use cases such as select party, enqueue action, start combat, finish combat.

Must not own:

- scene object references;
- cameras;
- tilemaps;
- direct UI components.

### `World`

Responsibilities:

- abstract view of map terrain/region state;
- movement rules;
- encounter checks;
- world navigation contracts;
- terrain interaction eligibility.

Must not own:

- Unity tilemaps directly;
- context menus;
- camera movement.

### `Party`

Responsibilities:

- party state;
- party selection state or selection-facing read models;
- action queue;
- out-of-combat lifecycle;
- inventory-facing gameplay rules;
- waypoint/travel progress at the domain level.

Must not own:

- camera centering;
- combat scene spawning;
- direct UI refresh behavior.

### `Combat`

Responsibilities:

- combat instance state;
- turn execution;
- cooldown progression;
- target selection;
- attack resolution;
- combat end conditions.

Must not own:

- combat scene visuals;
- floating text UI;
- GameObject spawning.

### `Content`

Responsibilities:

- content catalogs loaded from project assets;
- conversion from Unity assets/templates to core-readable data;
- lookup services for weapons, armor, encounters, monsters, terrain descriptors.

Must not own:

- gameplay flow;
- visual scene state.

### `UI`

Responsibilities:

- rendering current state to the player;
- collecting user intent;
- presenting map context actions, combat view, status panels, and inventories.

Must not own:

- gameplay rules;
- authoritative state transitions.

### 5.3 Unity adapter layer

The Unity side should become thin and explicit. Current manager classes should evolve toward adapters/presenters instead of owning rules.

Target direction:

- `GameManager` -> `AppBootstrap` or `ScreenFlowAdapter`
- `PartyManager` -> split across `PartyRegistryAdapter`, `PartySelectionAdapter`, and camera-focused presentation glue
- `CombatManager` -> split into `CombatRuntimeAdapter` and `CombatScenePresenter`
- `MapManager` -> split into `WorldMapReader`, `MapInputAdapter`, `MapHighlightPresenter`, and `ContextActionPresenter`
- `UIManager` -> reduced to panel/screen presentation concerns only

`Heroes.Party` is the most critical refactor hotspot. It currently mixes:

- entity-like party state;
- action queue state;
- movement execution;
- encounter polling;
- gathering flow;
- inventory mutation publishing;
- death handling;
- Unity lifecycle hooks.

The target architecture must split those responsibilities into domain state plus adapters/orchestrators.

### 5.4 Dependency rule

Allowed dependency direction:

- `Core` knows nothing about Unity.
- Unity adapters may call into `Core`.
- Presenters may observe or translate core state, but must not become alternate owners of gameplay logic.
- Content loaders may transform Unity assets into core data structures.

This rule is mandatory. If a refactor decision violates it, the design should be reconsidered before implementation continues.

## 6. Target flow model

### 6.1 Bootstrap

Current behavior must be preserved, but ownership changes:

- Unity scene initializes adapters and asset bindings.
- bootstrap code loads content catalogs;
- world adapters scan or expose map data;
- party registry is initialized;
- selected party state is established;
- current screen defaults to map;
- adapters sync cameras and UI from the core-facing state.

### 6.2 Party selection flow

Target flow:

- input or button triggers a select-party use case;
- application/party state changes in the core-facing layer;
- camera presenter centers the selected party;
- UI presenters update hero panel, actions, and combat view;
- combat scene presentation reacts only if the selected party is in combat.

The selection flow must stop depending on a manager that also owns camera logic and event broadcasting as one mixed concern.

### 6.3 Map action flow

Target flow:

- map input adapter resolves clicked tile/cell;
- world reader exposes terrain and region context;
- application layer determines valid actions;
- chosen action is translated into a domain action request;
- party action queue stores action intent, not scene-bound lambda behavior.

This is a major design correction. The current system stores executable delegates inside `HeroAction`, which couples action intent to scene/runtime code paths.

### 6.4 Travel flow

Target flow:

- core tracks travel intent, progress, and logical state;
- world/navigation services decide movement legality and route progress;
- Unity adapter applies the resulting world position to transforms;
- encounter checks happen from core-facing travel state, not as incidental side effects of a scene-heavy party object.

Current direct point-to-point movement may be preserved initially even if pathfinding is not yet integrated.

### 6.5 Gathering flow

Target flow:

- world layer determines whether the terrain supports the requested gathering action;
- party/action systems manage execution timing and progress;
- gathering rules resolve material, difficulty, performance, and success;
- inventory updates happen through party/inventory domain services;
- UI updates observe state changes instead of being called from multiple gameplay points.

### 6.6 Combat flow

Target flow:

- encounter start becomes an application/combat use case;
- combat runtime owns participants, cooldown progression, targeting, and outcome resolution;
- combat presentation mirrors combatants in the scene;
- damage text and visual effects subscribe to combat results instead of being embedded in combat rule ownership.

## 7. Migration strategy

### 7.1 General strategy

Use incremental extraction under a fixed architectural target.

This means:

- define the target model first;
- migrate one responsibility cluster at a time;
- preserve visible behavior after each slice;
- avoid mixing feature work with refactor work.

This is not a big-bang rewrite.

### 7.2 Required sequencing

Recommended order:

1. Characterize current behavior with docs and lightweight verification checks.
2. Introduce shared core contracts and translation boundaries.
3. Extract `Party` and action queue behavior from `Heroes.Party`.
4. Extract combat runtime from `CombatManager` and related scene-heavy ownership.
5. Extract world/navigation/interaction boundaries from `MapManager`.
6. Simplify top-level application/bootstrap flow.
7. Reduce UI and presentation classes to observers/presenters.

### 7.3 Why `Party` goes first

`Heroes.Party` is the highest-leverage seam because it currently sits at the intersection of:

- movement;
- gathering;
- encounter checks;
- combat start;
- queue execution;
- inventory updates;
- death handling.

Refactoring it first creates better boundaries for both combat and world systems.

### 7.4 Decomposition into sub-projects

The overall architecture refactor is too large for a single implementation burst.

It should be decomposed into implementation sub-projects:

1. documentation foundation migration;
2. party/action-queue extraction;
3. combat runtime extraction;
4. world/map interaction extraction;
5. bootstrap and UI cleanup.

Each sub-project should get its own implementation plan and completion criteria.

The first implementation plan should target sub-project 1 plus the preparatory scaffolding needed for sub-project 2.

## 8. Refactor guardrails

These rules are mandatory during implementation:

- No new features during refactor branches.
- No behavior changes unless explicitly documented and approved.
- Move ownership before optimizing formulas or balancing values.
- Do not let adapters become new god-objects.
- Prefer narrow services/use cases over replacing one large manager with another large manager.
- Keep naming tied to responsibility, not historical scene placement.
- Preserve testability by isolating pure logic from Unity APIs.

## 9. Refactor agent profile

The user asked for an agent that is suitable for this refactor. The correct profile is not "cleanup agent" or "rename agent". The task requires an architecture-driven refactor agent with these constraints:

- preserve current behavior first;
- optimize for responsibility boundaries;
- extract pure-core logic incrementally;
- avoid feature work during refactor;
- use domain-by-domain slices;
- keep Unity adapters thin;
- produce explicit contracts and acceptance criteria per slice.

Suggested mandate for future refactor sessions:

- Mission: migrate gameplay rules toward pure core code while preserving current behavior.
- Forbidden work: new features, balancing changes, speculative rewrites.
- Priority domains: party/action queue first, then combat runtime, then world/map.
- Definition of success: current manager overlap is reduced, domain ownership is explicit, and Unity scene code is no longer the primary home of gameplay rules.

## 10. Acceptance criteria for this design

This design is successful if future implementation work produces:

- two canonical documentation layers with clear purpose separation;
- a documented dependency rule of `Core` -> no Unity;
- explicit ownership boundaries for `Application`, `World`, `Party`, `Combat`, `Content`, and `UI`;
- a phased migration plan rather than a full rewrite;
- a first implementation slice focused on documentation migration and party/action extraction scaffolding.

## 11. Risks and unresolved items

Known risks:

- current gameplay behavior is partly encoded in scene wiring and `MonoBehaviour` timing order;
- some current data structures mix asset data, runtime state, and presentation assumptions;
- `HeroAction` currently stores executable delegates, which will require careful translation to preserve behavior;
- some documentation statements about scene/content can drift quickly without explicit validation metadata.

Open items to settle in implementation planning:

- exact folder/module layout for the new core and adapter layers;
- how core state changes are surfaced back to existing UI classes during migration;
- what verification coverage is realistic before introducing more tests;
- whether content-loading abstractions should be introduced before or during party extraction.

## 12. Immediate next step

After user review of this written spec, the next skill/process step is to write an implementation plan for:

- the documentation split;
- the initial core/adapter scaffolding;
- the first extraction seam around party/action ownership.
