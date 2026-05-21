# Documentation And Core Scaffolding Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Split the current documentation into canonical architecture/current-state layers and add the first pure C# core scaffolding for party action ownership.

**Architecture:** This phase preserves current gameplay behavior. It adds a Unity-free `Rymora.Core` assembly and EditMode tests, but does not wire the new core into existing managers yet. The current `Heroes.Party`, `HeroAction`, `ActionMenu`, `MapManager`, and `CombatManager` continue to run the game.

**Tech Stack:** Unity 6000.4.3f1, C# 9, Unity Test Framework 1.6.0, NUnit EditMode tests, Markdown documentation.

---

## Scope

This plan implements the first slice from `docs/superpowers/specs/2026-04-23-documentation-and-core-refactor-design.md`:

- documentation foundation migration;
- initial core/adapter assembly boundary;
- first tested pure-core model for party action queue ownership.

This plan does not replace existing runtime behavior. The new `Rymora.Core.Party` types are scaffolding for the next refactor plan.

## File Structure

Create:

- `docs/arquitetura-estavel.md`: canonical stable architecture document.
- `docs/estado-atual.md`: validated snapshot of current project state.
- `docs/superpowers/plans/2026-04-24-documentation-and-core-scaffolding.md`: this plan.
- `Assets/Scripts/Core/Rymora.Core.asmdef`: Unity-free core assembly boundary.
- `Assets/Scripts/Core/Party/PartyActionType.cs`: core action type enum.
- `Assets/Scripts/Core/Party/PartyActionEndType.cs`: core action completion enum.
- `Assets/Scripts/Core/Party/PartyActionRequest.cs`: immutable action intent.
- `Assets/Scripts/Core/Party/PartyActionState.cs`: runtime progress for one action.
- `Assets/Scripts/Core/Party/PartyActionQueue.cs`: pure queue for party actions.
- `Assets/Tests/EditMode/Rymora.Core.Tests.asmdef`: EditMode test assembly.
- `Assets/Tests/EditMode/PartyActionQueueTests.cs`: characterization tests for the new queue.

Modify:

- `DOCUMENTACAO_JOGO.md`: convert to a short index that points to the two canonical docs.

Do not modify in this plan:

- `Assets/Scripts/Creatures/Heroes/Party.cs`
- `Assets/Scripts/Creatures/Heroes/HeroAction.cs`
- `Assets/GameObjects/UI/ActionMenu.cs`
- `Assets/GameObjects/Maps/MapManager.cs`
- `Assets/GameObjects/Combat/CombatManager.cs`

## Task 1: Create Stable Architecture Documentation

**Files:**

- Create: `docs/arquitetura-estavel.md`

- [ ] **Step 1: Create the architecture document**

Create `docs/arquitetura-estavel.md` with this content:

```markdown
# Arquitetura estavel - Rymora Idle

Data de validacao inicial: 2026-04-24
Spec de origem: `docs/superpowers/specs/2026-04-23-documentation-and-core-refactor-design.md`

Este documento descreve a arquitetura pretendida e as regras de responsabilidade do projeto. Ele deve permanecer util mesmo quando cena, assets, balanceamento e conteudo mudarem.

Para detalhes volateis do estado atual do projeto, use `docs/estado-atual.md`.

## 1. Visao geral

Rymora Idle e um prototipo Unity de exploracao idle com mapa em tilemap, grupos de herois, fila de acoes, coleta, encontros aleatorios, combate automatico e UI 2D.

A direcao arquitetural aprovada e:

- regras de jogo em um core C# puro;
- Unity como camada de adaptacao para cena, input, visual, cameras, tilemaps, spawners e assets;
- migracao incremental, preservando comportamento externo atual.

## 2. Regra de dependencia

A regra principal e:

- `Core` nao referencia Unity;
- adaptadores Unity podem chamar o `Core`;
- UI apresenta estado e dispara intencoes;
- regras de jogo ficam no `Core`;
- assets Unity sao convertidos para dados consumiveis pelo `Core`.

Se uma mudanca colocar regra de jogo dentro de um presenter, spawner, camera adapter ou painel de UI, a fronteira deve ser revista.

## 3. Contextos principais

### Application

Dono de casos de uso e coordenacao entre dominios.

Responsabilidades:

- bootstrap em nivel de aplicacao;
- estado de tela;
- selecao de party;
- inicio e fim de combate;
- enfileiramento de intencoes de acao;
- coordenacao entre party, mundo, combate e conteudo.

Nao deve possuir:

- referencias diretas a camera;
- referencias diretas a tilemaps;
- componentes visuais;
- detalhes de GameObject.

### World

Dono das regras de mapa e contexto do mundo.

Responsabilidades:

- leitura abstrata de terreno e regiao;
- elegibilidade de acoes por terreno;
- movimento logico;
- checks de encontro;
- contratos de navegacao.

Nao deve possuir:

- UI de menu contextual;
- transform de party;
- cameras;
- spawners.

### Party

Dono do estado de grupo e ciclo fora de combate.

Responsabilidades:

- membros do grupo;
- fila de acoes;
- acao atual;
- progresso de viagem e coleta;
- inventario relacionado ao grupo;
- estado vivo/morto;
- resposta logica a derrota.

Nao deve possuir:

- centralizacao de camera;
- renderizacao de combate;
- atualizacao direta de HUD;
- leitura direta de mouse.

### Combat

Dono das regras de combate.

Responsabilidades:

- instancia de combate;
- participantes;
- cooldowns;
- escolha de alvo;
- resolucao de ataques;
- eventos de resultado;
- condicoes de vitoria e derrota.

Nao deve possuir:

- criacao de corpos visuais;
- texto flutuante;
- cameras;
- botoes ou paineis.

### Content

Dono de catalogos e conversao de conteudo.

Responsabilidades:

- armas;
- armaduras;
- encontros;
- monstros;
- terrenos;
- conversao de assets/templates Unity para dados do core.

Nao deve possuir:

- loop de gameplay;
- regras de UI;
- controle de cena.

### UI

Dono da apresentacao e captura de intencao do jogador.

Responsabilidades:

- mostrar estado atual;
- permitir selecao de party;
- mostrar fila de acoes;
- mostrar inventario;
- mostrar combate;
- exibir acoes disponiveis do mapa.

Nao deve possuir:

- regra de combate;
- regra de coleta;
- regra de encontro;
- estado autoritativo do jogo.

## 4. Managers atuais e destino arquitetural

### GameManager

Estado atual: coordena inicializacao, cameras e tela atual.

Destino: `AppBootstrap` ou `ScreenFlowAdapter`.

Responsabilidade futura:

- inicializar adaptadores;
- acionar bootstrap de aplicacao;
- sincronizar tela inicial.

### PartyManager

Estado atual: registra parties, controla selecao, publica eventos e centraliza camera.

Destino: divisao entre registro, selecao e presenter de camera.

Responsabilidade futura:

- expor parties existentes na cena ao core/adapters;
- publicar mudanca de selecao;
- deixar camera e UI reagirem como apresentacao.

### CombatManager

Estado atual: cria instancia de combate, instancia corpos visuais, roda turnos e publica feedback visual.

Destino: divisao entre runtime de combate e presenter de cena.

Responsabilidade futura:

- adaptar chamadas entre Unity e core;
- apresentar combatentes;
- exibir eventos de combate vindos do core.

### MapManager

Estado atual: le mouse, destaca tile, abre menu contextual, guarda tile atual e varre cidades.

Destino: divisao entre leitor de mapa, input adapter, highlight presenter e presenter do menu contextual.

Responsabilidade futura:

- traduzir tilemaps para contexto de mundo;
- traduzir input em intencao;
- manter apresentacao de hover e menu.

### UIManager

Estado atual: alterna paineis e cameras.

Destino: presenter de paineis/telas.

Responsabilidade futura:

- visibilidade de UI;
- estado visual de telas;
- nenhuma regra de gameplay.

### Heroes.Party

Estado atual: mistura estado, fila de acoes, movimento, coleta, encontro, combate, inventario, morte e lifecycle Unity.

Destino: separar estado de party, fila de acoes, servicos de execucao e adapter Unity.

Responsabilidade futura:

- representar a party na cena;
- delegar regras ao core;
- aplicar resultado visual/logico vindo da aplicacao.

## 5. Fluxos principais

### Bootstrap

Unity inicializa cena e adaptadores.
O bootstrap registra conteudo, parties, mapa e estado inicial.
A tela inicial continua sendo mapa.

### Selecao de party

Input ou botao dispara uma intencao de selecao.
O estado de selecao muda.
Camera, HUD e presenter de combate reagem ao estado.

### Acoes no mapa

Input de mapa resolve o tile clicado.
World informa acoes validas.
Application cria uma requisicao de acao.
Party enfileira a intencao.

### Viagem

Party/core acompanha destino, progresso e terreno atual.
Unity aplica posicao visual.
Encontros sao checados durante travel.

### Coleta

World valida terreno.
Party/action acompanha tempo e execucoes.
Regra de coleta resolve material, dificuldade, performance e sucesso.
Inventario recebe o item quando houver sucesso.

### Combate

Application inicia combate a partir de encounter.
Combat resolve turnos e resultados.
Unity apresenta corpos, texto e efeitos.
Fim de combate limpa estado e aplica vitoria ou derrota.

## 6. Regras para proximas implementacoes

- Preservar comportamento antes de melhorar design funcional.
- Nao adicionar feature nova durante refactor.
- Criar testes de caracterizacao para regras extraidas.
- Migrar por dominio, com verificacao apos cada etapa.
- Manter adapters pequenos.
- Evitar trocar um manager grande por outro manager grande.
- Nomear tipos por responsabilidade atual, nao por historico da cena.
```

- [ ] **Step 2: Verify the document exists**

Run:

```powershell
Test-Path 'docs\arquitetura-estavel.md'
```

Expected:

```text
True
```

- [ ] **Step 3: Verify the architecture rule is present**

Run:

```powershell
Select-String -Path 'docs\arquitetura-estavel.md' -Pattern 'Core` nao referencia Unity'
```

Expected: one match in section `2. Regra de dependencia`.

- [ ] **Step 4: Commit the architecture doc**

Run:

```powershell
git add docs/arquitetura-estavel.md
git commit -m "docs: add stable architecture guide"
```

Expected: commit succeeds with one new file.

## Task 2: Create Current-State Documentation And Index

**Files:**

- Create: `docs/estado-atual.md`
- Modify: `DOCUMENTACAO_JOGO.md`

- [ ] **Step 1: Create the current-state document**

Create `docs/estado-atual.md` by moving the current long-form content from `DOCUMENTACAO_JOGO.md` into this file.

Use this exact header before the existing body:

```markdown
# Estado atual do jogo - Rymora Idle

Data de validacao: 2026-04-24
Commit validado: `3f1b56bba2e76deea5cfd8836eb1234627d61a7e`
Cena principal validada: `Assets/Scenes/MainScreen.unity`
Fonte original: `DOCUMENTACAO_JOGO.md`

Este documento descreve o estado observado do projeto nesta validacao. Ele contem detalhes de cena, conteudo conectado e comportamento atual que podem mudar com facilidade.

Para regras arquiteturais estaveis, use `docs/arquitetura-estavel.md`.
```

After that header, paste the current sections from `DOCUMENTACAO_JOGO.md` starting at:

```markdown
## 1. Visao geral
```

through the end of the existing file.

- [ ] **Step 2: Replace the root documentation file with an index**

Replace the full content of `DOCUMENTACAO_JOGO.md` with:

```markdown
# Documentacao do jogo - Rymora Idle

Esta pagina e o indice de documentacao do projeto.

Use estes documentos como fonte principal:

- `docs/arquitetura-estavel.md`: responsabilidades, fronteiras arquiteturais, fluxos estaveis e regras para futuras implementacoes.
- `docs/estado-atual.md`: snapshot validado do estado atual da cena, conteudo conectado, comportamento observado e gaps conhecidos.

## Como usar em novos chats

Para planejar feature nova, comece por `docs/arquitetura-estavel.md`.

Para entender como o jogo funciona hoje no projeto Unity, consulte `docs/estado-atual.md`.

Para refactors, use os dois documentos:

- primeiro confira as fronteiras pretendidas em `docs/arquitetura-estavel.md`;
- depois valide o comportamento atual em `docs/estado-atual.md`.

## Spec de refactor

O design aprovado para a reorganizacao arquitetural esta em:

- `docs/superpowers/specs/2026-04-23-documentation-and-core-refactor-design.md`
```

- [ ] **Step 3: Verify the index points to both canonical docs**

Run:

```powershell
Select-String -Path 'DOCUMENTACAO_JOGO.md' -Pattern 'docs/arquitetura-estavel.md','docs/estado-atual.md'
```

Expected: at least two matches.

- [ ] **Step 4: Verify the state document has validation metadata**

Run:

```powershell
Select-String -Path 'docs\estado-atual.md' -Pattern 'Commit validado','Cena principal validada'
```

Expected: two matches.

- [ ] **Step 5: Commit the documentation split**

Run:

```powershell
git add DOCUMENTACAO_JOGO.md docs/estado-atual.md
git commit -m "docs: split game documentation by stability"
```

Expected: commit succeeds with one new doc and one modified root doc.

## Task 3: Add Pure Core And Test Assembly Boundaries

**Files:**

- Create: `Assets/Scripts/Core/Rymora.Core.asmdef`
- Create: `Assets/Tests/EditMode/Rymora.Core.Tests.asmdef`

- [ ] **Step 1: Create the core assembly definition**

Create `Assets/Scripts/Core/Rymora.Core.asmdef` with:

```json
{
    "name": "Rymora.Core",
    "rootNamespace": "Rymora.Core",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": true
}
```

- [ ] **Step 2: Create the EditMode test assembly definition**

Create `Assets/Tests/EditMode/Rymora.Core.Tests.asmdef` with:

```json
{
    "name": "Rymora.Core.Tests",
    "rootNamespace": "Rymora.Core.Tests",
    "references": [
        "Rymora.Core"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "optionalUnityReferences": [
        "TestAssemblies"
    ],
    "noEngineReferences": false
}
```

- [ ] **Step 3: Verify both assembly files exist**

Run:

```powershell
Test-Path 'Assets\Scripts\Core\Rymora.Core.asmdef'
Test-Path 'Assets\Tests\EditMode\Rymora.Core.Tests.asmdef'
```

Expected:

```text
True
True
```

- [ ] **Step 4: Commit assembly boundaries**

Run:

```powershell
git add Assets/Scripts/Core/Rymora.Core.asmdef Assets/Tests/EditMode/Rymora.Core.Tests.asmdef
git commit -m "chore: add core assembly boundaries"
```

Expected: commit succeeds with two new asmdef files.

## Task 4: Write Failing Party Action Queue Tests

**Files:**

- Create: `Assets/Tests/EditMode/PartyActionQueueTests.cs`

- [ ] **Step 1: Add tests for the pure action queue**

Create `Assets/Tests/EditMode/PartyActionQueueTests.cs` with:

```csharp
using NUnit.Framework;
using Rymora.Core.Party;

namespace Rymora.Core.Tests
{
    public class PartyActionQueueTests
    {
        [Test]
        public void StartNextIfIdle_DequeuesFirstRequestAndLeavesRemainingPending()
        {
            var queue = new PartyActionQueue();
            var travel = PartyActionRequest.Travel();
            var mine = PartyActionRequest.Mine(3m, 5);

            queue.Enqueue(travel);
            queue.Enqueue(mine);

            var current = queue.StartNextIfIdle();

            Assert.AreSame(travel, current.Request);
            Assert.AreEqual(1, queue.PendingCount);
            Assert.AreSame(current, queue.Current);
        }

        [Test]
        public void StartNextIfIdle_ReturnsCurrentActionWhenAlreadyRunning()
        {
            var queue = new PartyActionQueue();
            queue.Enqueue(PartyActionRequest.Travel());
            queue.Enqueue(PartyActionRequest.CutWood(2m, 5));

            var first = queue.StartNextIfIdle();
            var second = queue.StartNextIfIdle();

            Assert.AreSame(first, second);
            Assert.AreEqual(1, queue.PendingCount);
        }

        [Test]
        public void ByCountAction_CompletesAfterExecutedCountReachesLimit()
        {
            var state = new PartyActionState(PartyActionRequest.Mine(3m, 2));

            state.MarkStarted();
            state.MarkExecuted(0.5m);

            Assert.IsFalse(state.IsComplete);

            state.MarkStarted();
            state.MarkExecuted(0.5m);

            Assert.IsTrue(state.IsComplete);
        }

        [Test]
        public void ByTimeAction_CompletesWhenPassedTimeReachesEndTime()
        {
            var request = new PartyActionRequest(
                PartyActionType.Mine,
                PartyActionEndType.ByTime,
                1m,
                endTime: 2m);
            var state = new PartyActionState(request);

            state.MarkExecuted(1.5m);

            Assert.IsFalse(state.IsComplete);

            state.MarkExecuted(0.5m);

            Assert.IsTrue(state.IsComplete);
        }

        [Test]
        public void Clear_RemovesCurrentAndPendingActions()
        {
            var queue = new PartyActionQueue();
            queue.Enqueue(PartyActionRequest.Travel());
            queue.Enqueue(PartyActionRequest.Mine(3m, 5));
            queue.StartNextIfIdle();

            queue.Clear();

            Assert.IsNull(queue.Current);
            Assert.AreEqual(0, queue.PendingCount);
        }
    }
}
```

- [ ] **Step 2: Run EditMode tests to verify they fail before implementation**

Run:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.4.3f1\Editor\Unity.exe' -batchmode -quit -projectPath '.' -runTests -testPlatform EditMode -testResults 'Temp\editmode-test-results.xml'
```

Expected: FAIL because `Rymora.Core.Party.PartyActionQueue`, `PartyActionRequest`, `PartyActionState`, `PartyActionType`, and `PartyActionEndType` do not exist yet.

- [ ] **Step 3: Commit failing tests**

Run:

```powershell
git add Assets/Tests/EditMode/PartyActionQueueTests.cs
git commit -m "test: characterize party action queue"
```

Expected: commit succeeds with one new test file.

## Task 5: Implement Core Party Action Types

**Files:**

- Create: `Assets/Scripts/Core/Party/PartyActionType.cs`
- Create: `Assets/Scripts/Core/Party/PartyActionEndType.cs`
- Create: `Assets/Scripts/Core/Party/PartyActionRequest.cs`

- [ ] **Step 1: Add action type enum**

Create `Assets/Scripts/Core/Party/PartyActionType.cs` with:

```csharp
namespace Rymora.Core.Party
{
    public enum PartyActionType
    {
        Travel = 0,
        Mine = 1,
        CutWood = 2
    }
}
```

- [ ] **Step 2: Add action end type enum**

Create `Assets/Scripts/Core/Party/PartyActionEndType.cs` with:

```csharp
namespace Rymora.Core.Party
{
    public enum PartyActionEndType
    {
        ByCount = 0,
        ByItemQuantity = 1,
        ByTime = 2
    }
}
```

- [ ] **Step 3: Add immutable action request**

Create `Assets/Scripts/Core/Party/PartyActionRequest.cs` with:

```csharp
using System;

namespace Rymora.Core.Party
{
    public sealed class PartyActionRequest
    {
        public PartyActionType ActionType { get; }
        public PartyActionEndType EndType { get; }
        public decimal TimeToExecute { get; }
        public int? LimitCount { get; }
        public decimal? EndTime { get; }
        public string ItemName { get; }

        public PartyActionRequest(
            PartyActionType actionType,
            PartyActionEndType endType,
            decimal timeToExecute,
            int? limitCount = null,
            decimal? endTime = null,
            string itemName = null)
        {
            if (timeToExecute < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(timeToExecute), "Time to execute cannot be negative.");
            }

            if (limitCount.HasValue && limitCount.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limitCount), "Limit count cannot be negative.");
            }

            if (endTime.HasValue && endTime.Value < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(endTime), "End time cannot be negative.");
            }

            ActionType = actionType;
            EndType = endType;
            TimeToExecute = timeToExecute;
            LimitCount = limitCount;
            EndTime = endTime;
            ItemName = itemName;
        }

        public static PartyActionRequest Travel()
        {
            return new PartyActionRequest(
                PartyActionType.Travel,
                PartyActionEndType.ByCount,
                0m,
                limitCount: 1);
        }

        public static PartyActionRequest Mine(decimal timeToExecute, int limitCount)
        {
            return new PartyActionRequest(
                PartyActionType.Mine,
                PartyActionEndType.ByCount,
                timeToExecute,
                limitCount: limitCount);
        }

        public static PartyActionRequest CutWood(decimal timeToExecute, int limitCount)
        {
            return new PartyActionRequest(
                PartyActionType.CutWood,
                PartyActionEndType.ByCount,
                timeToExecute,
                limitCount: limitCount);
        }
    }
}
```

- [ ] **Step 4: Commit action request types**

Run:

```powershell
git add Assets/Scripts/Core/Party/PartyActionType.cs Assets/Scripts/Core/Party/PartyActionEndType.cs Assets/Scripts/Core/Party/PartyActionRequest.cs
git commit -m "feat: add core party action requests"
```

Expected: commit succeeds with three new core files.

## Task 6: Implement Core Party Action State And Queue

**Files:**

- Create: `Assets/Scripts/Core/Party/PartyActionState.cs`
- Create: `Assets/Scripts/Core/Party/PartyActionQueue.cs`

- [ ] **Step 1: Add action runtime state**

Create `Assets/Scripts/Core/Party/PartyActionState.cs` with:

```csharp
using System;

namespace Rymora.Core.Party
{
    public sealed class PartyActionState
    {
        public PartyActionRequest Request { get; }
        public decimal CurrentTime { get; private set; }
        public decimal PassedTime { get; private set; }
        public int ExecutedCount { get; private set; }
        public bool Started { get; private set; }

        public PartyActionState(PartyActionRequest request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public bool IsReadyToExecute
        {
            get { return CurrentTime >= Request.TimeToExecute; }
        }

        public bool IsComplete
        {
            get
            {
                switch (Request.EndType)
                {
                    case PartyActionEndType.ByCount:
                        return Request.LimitCount.HasValue && ExecutedCount >= Request.LimitCount.Value;
                    case PartyActionEndType.ByTime:
                        return Request.EndTime.HasValue && PassedTime >= Request.EndTime.Value;
                    case PartyActionEndType.ByItemQuantity:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void MarkStarted()
        {
            Started = true;
        }

        public void AddProgress(decimal deltaTime, decimal performance)
        {
            CurrentTime += deltaTime * performance;
        }

        public void MarkExecuted(decimal deltaTime)
        {
            Started = false;
            PassedTime += deltaTime;
            ExecutedCount++;
            CurrentTime = 0m;
        }
    }
}
```

- [ ] **Step 2: Add action queue**

Create `Assets/Scripts/Core/Party/PartyActionQueue.cs` with:

```csharp
using System;
using System.Collections.Generic;

namespace Rymora.Core.Party
{
    public sealed class PartyActionQueue
    {
        private readonly Queue<PartyActionRequest> pending = new Queue<PartyActionRequest>();

        public PartyActionState Current { get; private set; }

        public int PendingCount
        {
            get { return pending.Count; }
        }

        public void Enqueue(PartyActionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            pending.Enqueue(request);
        }

        public PartyActionState StartNextIfIdle()
        {
            if (Current == null && pending.Count > 0)
            {
                Current = new PartyActionState(pending.Dequeue());
            }

            return Current;
        }

        public void CompleteCurrentIfFinished()
        {
            if (Current != null && Current.IsComplete)
            {
                Current = null;
            }
        }

        public void Clear()
        {
            pending.Clear();
            Current = null;
        }
    }
}
```

- [ ] **Step 3: Run EditMode tests**

Run:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.4.3f1\Editor\Unity.exe' -batchmode -quit -projectPath '.' -runTests -testPlatform EditMode -testResults 'Temp\editmode-test-results.xml'
```

Expected: PASS for all tests in `PartyActionQueueTests`.

- [ ] **Step 4: Commit queue implementation**

Run:

```powershell
git add Assets/Scripts/Core/Party/PartyActionState.cs Assets/Scripts/Core/Party/PartyActionQueue.cs
git commit -m "feat: add core party action queue"
```

Expected: commit succeeds with two new core files.

## Task 7: Verify Core Isolation And Documentation Links

**Files:**

- Verify: `Assets/Scripts/Core/**/*.cs`
- Verify: `docs/arquitetura-estavel.md`
- Verify: `docs/estado-atual.md`
- Verify: `DOCUMENTACAO_JOGO.md`

- [ ] **Step 1: Verify core files do not reference Unity**

Run:

```powershell
Select-String -Path 'Assets\Scripts\Core\**\*.cs' -Pattern 'UnityEngine','MonoBehaviour','GameObject','Transform','Vector3','Tilemap'
```

Expected: no matches.

- [ ] **Step 2: Verify root documentation is an index**

Run:

```powershell
Get-Content -Path 'DOCUMENTACAO_JOGO.md' -TotalCount 40
```

Expected: output starts with `# Documentacao do jogo - Rymora Idle` and contains links to `docs/arquitetura-estavel.md` and `docs/estado-atual.md`.

- [ ] **Step 3: Verify current-state document still contains the gameplay snapshot**

Run:

```powershell
Select-String -Path 'docs\estado-atual.md' -Pattern '## 9. Combate','## 10. Interface atual','## 11. Sistemas presentes'
```

Expected: three matches.

- [ ] **Step 4: Verify EditMode test result file exists**

Run:

```powershell
Test-Path 'Temp\editmode-test-results.xml'
```

Expected:

```text
True
```

- [ ] **Step 5: Commit verification-only doc adjustment if needed**

If verification finds a broken link or missing validation metadata, fix that exact issue and run:

```powershell
git add DOCUMENTACAO_JOGO.md docs/arquitetura-estavel.md docs/estado-atual.md
git commit -m "docs: fix documentation foundation links"
```

Expected: commit succeeds only if a documentation fix was made. If no fix was made, skip this commit step.

## Task 8: Final Review

**Files:**

- Review: `docs/arquitetura-estavel.md`
- Review: `docs/estado-atual.md`
- Review: `DOCUMENTACAO_JOGO.md`
- Review: `Assets/Scripts/Core/Rymora.Core.asmdef`
- Review: `Assets/Scripts/Core/Party/*.cs`
- Review: `Assets/Tests/EditMode/*.cs`
- Review: `Assets/Tests/EditMode/Rymora.Core.Tests.asmdef`

- [ ] **Step 1: Check worktree**

Run:

```powershell
git status --short
```

Expected: no unrelated files are staged. Existing user changes may remain unstaged.

- [ ] **Step 2: Inspect recent commits**

Run:

```powershell
git log --oneline -n 8
```

Expected: recent commits include documentation split, core assembly boundaries, party action request types, and party action queue.

- [ ] **Step 3: Run final EditMode test pass**

Run:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.4.3f1\Editor\Unity.exe' -batchmode -quit -projectPath '.' -runTests -testPlatform EditMode -testResults 'Temp\editmode-test-results.xml'
```

Expected: PASS for all EditMode tests.

- [ ] **Step 4: Report completion**

Report these items:

```text
Implemented:
- split DOCUMENTACAO_JOGO.md into stable architecture and current-state docs
- added Unity-free Rymora.Core assembly
- added tested core party action queue scaffolding

Verification:
- EditMode tests passed
- core files contain no Unity references

Deferred:
- no existing runtime manager behavior was changed in this phase
- wiring Heroes.Party to the new core queue remains for the next implementation plan
```
