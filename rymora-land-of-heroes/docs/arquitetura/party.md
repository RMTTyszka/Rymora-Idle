# Party - Rymora Land of Heroes

Data de criacao: 2026-05-23
Regras de negocio: `docs/regras/biblia-rpg.md` (secao Herois, Acoes de Heroi, Coleta, Inventario, Viagem)
Arquitetura geral: `docs/arquitetura/visao-geral.md`

---

## 1. Proposito

O dominio **Party** cobre estado do grupo, fila de acoes, inventario, progresso de viagem e coleta, e ciclo fora de combate. Cada Party e uma entidade no Core que evolve independentemente.

Party nao possui:
- renderizacao de sprite no mapa
- camera
- input

---

## 2. Party

### 2.1 Estrutura

```csharp
public sealed class Party
{
    public string Id { get; }
    public IReadOnlyList<Hero> Members { get; }
    public Hero? Leader { get; } // null quando party esta vazia
    public PartyActionQueue ActionQueue { get; }
    public Inventory Inventory { get; }
    public TilePosition Position { get; set; } // tile atual no Core
    public bool IsAlive { get; }
    public bool IsInCombat { get; set; }
}
```

- Estado inicial da v1 pode usar 1 heroi por Party.
- O modelo precisa suportar `0..N` herois por Party, porque herois podem mudar de party e uma party pode ficar sem heroi.
- `Leader` e atalho para o primeiro membro quando existir.
- Position e a coordenada do tile onde a party esta.

### 2.2 Ciclo de vida

- Party ativa: executa acoes, viaja, coleta, encontra monstros.
- Party em combate: pausa acoes de mapa, executa combate.
- Party morta: limpa fila, retorna ao safe spot, herois mortos viram fantasmas.

---

## 3. Fila de acoes

### 3.1 Tipos de acao

```csharp
public enum PartyActionType
{
    Travel,
    Mine,
    CutWood,
    TransferItem
}
```

### 3.2 Modos de termino

A biblia confirma quatro formas de programar acoes da party: executar uma vez, repetir para sempre, repetir por uma quantidade definida ou repetir por tempo definido. A estrutura atual cobre contagem, quantidade de item e tempo; o fluxo final de UI/Application ainda precisa definir como representar repeticao infinita sem criar comportamento ambiguo.

```csharp
public enum PartyActionEndType
{
    ByCount,        // executar N vezes
    ByItemQuantity, // executar ate ter N do item
    ByTime          // executar por T segundos
}
```

### 3.3 Estruturas

**Request** (intencao, imutavel):

```csharp
public sealed class PartyActionRequest
{
    public PartyActionType ActionType { get; }
    public PartyActionEndType EndType { get; }
    public float TimeToExecute { get; } // tempo entre execucoes
    public int? LimitCount { get; }
    public float? EndTime { get; }
    public string? ItemName { get; } // coleta, transferencia, ByItemQuantity
    public int? ItemLevel { get; }
    public float? ItemWeight { get; }
    public int? Quantity { get; } // transferencia
    public string? TargetPartyId { get; } // transferencia
    public TilePosition? Destination { get; } // travel
    public IReadOnlyList<TilePosition>? Path { get; } // preenchido pelo Application
}
```

**State** (runtime, mutavel):

```csharp
public sealed class PartyActionState
{
    public PartyActionRequest Request { get; }
    public float CurrentTime { get; private set; } // progresso ate TimeToExecute
    public float PassedTime { get; private set; }  // total decorrido
    public int ExecutedCount { get; private set; }
    public bool Started { get; private set; }
    public bool IsComplete { get; }
    public bool IsReadyToExecute { get; }
}
```

**Queue**:

```csharp
public sealed class PartyActionQueue
{
    public PartyActionState? Current { get; }
    public int PendingCount { get; }

    public void Enqueue(PartyActionRequest request);
    public PartyActionState? StartNextIfIdle();
    public void CompleteCurrentIfFinished();
    public void Clear();
}
```

### 3.4 Ciclo de execucao

O ciclo abaixo descreve a execucao depois que a acao ja foi programada. A UI pode criar acoes imediatas pelo mapa ou gravar Macros que depois emitem `PartyActionRequest` durante a execucao do Program.

1. `StartNextIfIdle()` - se nao ha acao atual, pega da fila.
2. `MarkStarted()` - inicia contagem.
3. `Application` valida se a acao pode rodar (heroi vivo para travel/coleta; transferencia pode rodar sem heroi se parties estao no mesmo tile).
4. `AddProgress(deltaTime, performance)` - acumula tempo.
5. `IsReadyToExecute` - quando `CurrentTime >= TimeToExecute`.
6. `MarkExecuted()` - executa efeito, incrementa contagem, reseta CurrentTime.
7. `IsComplete` - verifica se `ExecutedCount >= LimitCount` (ByCount), `PassedTime >= EndTime` (ByTime), ou item quantity atingiu (ByItemQuantity).
8. `CompleteCurrentIfFinished()` - se completo, avanca pra proxima.

Persistencia atual:

- `ActionQueue.Current` e acoes pendentes sao salvas.
- Progresso parcial (`CurrentTime`, `PassedTime`, `ExecutedCount`, `Started`) e salvo.
- `PartyActionRequest` salva todos os campos, incluindo path calculado e `AutomationActionId`.

### 3.5 Loop da Party

A Party e atualizada pelo Application em intervalos regulares de tempo de jogo:

```
se IsInCombat -> nao faz nada (combate gerencia)
se morta -> retorna ao safe spot
senao:
    se ActionQueue.Current == null -> pega proxima
    se ActionQueue.Current.Started == false -> comeca
    se ActionQueue.Current.IsReadyToExecute -> executa travel/coleta/transferencia
    se ActionQueue.Current.IsComplete -> limpa e pega proxima
```

---

## 4. Macros e Program

Macros sao salvos por Party em `PartyAutomation`.

- Um `Macro` tem nome e uma lista ordenada de acoes.
- As acoes gravaveis na v1 sao `MoveTo`, `Mine` e `CutWood`.
- Cada Party tem um `Program` ativo.
- O `Program` e uma sequencia linear de usos de Macros.
- Cada uso referencia um Macro salvo por `id` e define sua propria repeticao.
- Editar um Macro afeta a proxima vez que esse Macro iniciar durante a execucao do Program.
- O `ProgramRunner` controla estados `Play`, `Pause`, `Stop` e `Error`.
- `Pause` e `Stop` sao cooperativos: a acao atual termina antes da mudanca de estado.

Persistencia atual:

- Automation salva recording ativo, Macros, Program e Runner.
- Recording salva id, acoes gravadas e contador da proxima acao.
- Program salva repeticao global, steps e contador do proximo step.
- Runner salva estado, erro, indices, iteracoes, tempos acumulados, acao atual e execution id.

---

## 5. Inventario

### 5.1 Estrutura

```csharp
public sealed class Inventory
{
    public IReadOnlyList<Item> Items { get; }
    public float TotalWeight { get; }

    public void AddItem(Item item);
    public bool RemoveItem(string name, int level, int quantity);
}
```

### 5.2 Regras

- Itens agrupaveis por `Name` + `Level`.
- `AddItem` adiciona a instancia.
- `RemoveItem` remove por nome, nivel e quantidade.
- Inventario da Party e salvo por itens agrupados com nome, nivel, peso e quantidade.
- Limite de peso/espacos: adiado.

### 5.3 Transferencia entre parties

- Exige que ambas parties estejam no mesmo tile.
- Pode ser programada como acao (`TransferItem`).
- Parties envolvidas tem seus inventarios modificados atomicamente.

---

## 6. Coleta

### 6.1 Mineracao

1. Acao `Mine` enfileirada com `TimeToExecute` e `LimitCount`.
2. `TryMine()`:
   - valida terreno atual == Mountain.
   - seleciona material baseado na qualidade do tile.
   - faz roll de `Mining` vs dificuldade definida em `CollectionConfig` para o nivel do material.
3. `Mine()`:
   - se sucesso: adiciona material ao inventario da Party.
   - incrementa contagem executada.

### 6.2 Corte de madeira

Mesmo fluxo da mineracao, trocando:
- skill: `Lumberjacking`
- `TimeToExecute`: menor que mineracao
- material: `Forest.GetMaterial()` (Oak atualmente)

---

## 7. Viagem

### 7.1 Waypoints

- Party pode ter multiplos destinos enfileirados.
- `WayPoints: Queue<TilePosition>`.
- Ao chegar em um waypoint, remove da fila.

### 7.2 Movimento

1. Core define caminho via World (pathfinding).
2. Party avanca tile a tile a cada tick.
3. Velocidade depende do terreno atual: `1 * (moveSpeed / 100)`.
4. Ao chegar em cada tile:
   - verifica encontro (se nao for City).
   - se houver encontro, Application inicia combate.
5. Se terreno nao for caminhavel, limpa fila.

---

## 8. Morte e retorno

1. Party morre em combate.
2. Core limpa `ActionQueue` (Current + pending).
3. Core encontra safe spot mais proximo via `World.FindNearestSafeSpot()`.
4. Party enfileira `Travel` ate o safe spot.
5. Herois mortos viram fantasmas (detalhes em `heroi.md`).
