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
    public Hero Leader { get; } // primeiro membro
    public PartyActionQueue ActionQueue { get; }
    public Inventory Inventory { get; }
    public Vector2I Position { get; set; } // tile atual
    public bool IsAlive { get; }
    public bool IsInCombat { get; set; }
}
```

- Cada Party tem 1 heroi na v1 (expansivel para grupo).
- `Leader` e atalho para o primeiro membro.
- Position e a coordenada do tile onde a party esta.

### 2.2 Ciclo de vida

- Party ativa: executa acoes, viaja, coleta, encontra monstros.
- Party em combate: pausa acoes de mapa, executa combate.
- Party morta: limpa fila, retorna ao safe spot, heroi vira fantasma.

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
    public string ItemName { get; } // para ByItemQuantity
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
    public PartyActionState Current { get; }
    public int PendingCount { get; }

    public void Enqueue(PartyActionRequest request);
    public PartyActionState StartNextIfIdle();
    public void CompleteCurrentIfFinished();
    public void Clear();
}
```

### 3.4 Ciclo de execucao

1. `StartNextIfIdle()` - se nao ha acao atual, pega da fila.
2. `MarkStarted()` - inicia contagem.
3. `AddProgress(deltaTime, performance)` - acumula tempo.
4. `IsReadyToExecute` - quando `CurrentTime >= TimeToExecute`.
5. `MarkExecuted()` - executa efeito, incrementa contagem, reseta CurrentTime.
6. `IsComplete` - verifica se `ExecutedCount >= LimitCount` (ByCount), `PassedTime >= EndTime` (ByTime), ou item quantity atingiu (ByItemQuantity).
7. `CompleteCurrentIfFinished()` - se completo, avanca pra proxima.

### 3.5 Loop da Party

A Party e atualizada pelo Application em intervalos regulares de tempo de jogo:

```
se IsInCombat -> nao faz nada (combate gerencia)
se morta -> retorna ao safe spot
senao:
    se ActionQueue.Current == null -> pega proxima
    se ActionQueue.Current.Started == false -> comeca
    se ActionQueue.Current.IsReadyToExecute -> executa
    se ActionQueue.Current.IsComplete -> limpa e pega proxima
```

---

## 4. Inventario

### 4.1 Estrutura

```csharp
public sealed class Inventory
{
    public IReadOnlyList<Item> Items { get; }
    public float TotalWeight { get; }

    public void AddItem(Item item);
    public bool RemoveItem(string name, int level, int quantity);
}
```

### 4.2 Regras

- Itens agrupaveis por `Name` + `Level`.
- `AddItem` adiciona a instancia.
- `RemoveItem` remove por nome, nivel e quantidade.
- Limite de peso/espacos: adiado.

### 4.3 Transferencia entre parties

- Exige que ambas parties estejam no mesmo tile.
- Pode ser programada como acao (`TransferItem`).
- Partes envolvidas tem seus inventories modificados atomicamente.

---

## 5. Coleta

### 5.1 Mineracao

1. Acao `Mine` enfileirada com `TimeToExecute` e `LimitCount`.
2. `TryMine()`:
   - valida terreno atual == Mountain.
   - seleciona material baseado na qualidade do tile.
   - faz roll de `Mining` vs dificuldade (`material.Level * 10 + 50`).
3. `Mine()`:
   - se sucesso: adiciona material ao inventario.
   - incrementa contagem executada.

### 5.2 Corte de madeira

Mesmo fluxo da mineracao, trocando:
- skill: `Lumberjacking`
- `TimeToExecute`: menor que mineracao
- material: `Forest.GetMaterial()` (Oak atualmente)

---

## 6. Viagem

### 6.1 Waypoints

- Party pode ter multiplos destinos enfileirados.
- `WayPoints: Queue<Vector2I>`.
- Ao chegar em um waypoint, remove da fila.

### 6.2 Movimento

1. Core define caminho via World (pathfinding).
2. Party avanca tile a tile a cada tick.
3. Velocidade depende do terreno atual: `1 * (moveSpeed / 100)`.
4. Ao chegar em cada tile:
   - verifica encontro (se nao for City).
   - se houver encontro, Application inicia combate.
5. Se terreno nao for caminhavel, limpa fila.

---

## 7. Morte e retorno

1. Party morre em combate.
2. Core limpa `ActionQueue` (Current + pending).
3. Core encontra safe spot mais proximo via `World.FindNearestSafeSpot()`.
4. Party enfileira `Travel` ate o safe spot.
5. Heroi vira fantasma (detalhes em `heroi.md`).
