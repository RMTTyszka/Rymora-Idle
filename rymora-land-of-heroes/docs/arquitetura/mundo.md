# Mundo - Rymora Land of Heroes

Data de criacao: 2026-05-23
Regras de negocio: `docs/regras/biblia-rpg.md` (secao Terrenos, Regioes, Viagem, Encontros)
Arquitetura geral: `docs/arquitetura/visao-geral.md`

---

## 1. Proposito

O dominio **World** cobre tudo relacionado ao mapa, terrenos, regioes, navegacao e encontros no mapa. Sua responsabilidade e fornecer uma abstracao do mundo para o resto do sistema, sem depender de tilemaps Godot.

O World nao possui:
- renderizacao de tilemap
- camera
- input de mouse
- UI de menu contextual

---

## 2. Mapa

O mapa e uma grade hexagonal 2D topdown. Cada celula contem:

| Componente | Descricao |
|-----------|-----------|
| Terrain | Tipo de terreno (Plain, Forest, Mountain, Road, City, Place, Mine, Wall) |
| Region | Regiao geografica (define encontros, safe spots) |


O mapa e carregado a partir de um tilemap Godot. O World adapter le o tilemap e converte para dados do Core na inicializacao.

### 2.1 Terrenos

Ver `docs/regras/biblia-rpg.md` para regras de negocio completas.

Estrutura no Core:

```csharp
public sealed record TerrainData(
    TerrainType Type,
    bool IsWalkable,
    float MoveSpeed,
    int Quality,
    int EncounterRateModifier,
    bool AllowsMining,
    bool AllowsWoodcutting,
    bool IsCity,
    bool IsPlace
)
```

### 2.2 Regioes

Regioes agrupam tiles e definem:
- lista de encontros possiveis
- se e safe spot (sem encontros)

Estrutura no Core:

```csharp
public sealed record RegionData(
    string Name,
    bool IsSafeSpot,
    IReadOnlyList<EncounterTemplate> Encounters
)
```

---

## 3. Navegacao

### 3.1 Pathfinding

O movimento usa pathfinding em grade hexagonal:

- entrada: `origin (Vector2I)` e `destination (Vector2I)`
- saida: `IReadOnlyList<Vector2I>` (caminho)
- respeita `IsWalkable` dos terrenos
- bloqueios: `Wall` tiles

Implementacao: A* sobre grade hexagonal. Otimizacoes (distancia de Manhattan hexagonal, cache de caminho) adiadas.

### 3.2 Movimento da Party

A Party segue o caminho tile a tile:

1. Core define o caminho e a velocidade no tile atual.
2. Adapter Godot interpola a posicao visual entre tiles.
3. Ao chegar em cada tile, Core verifica encontro e acoes pendentes.
4. Se o terreno nao for caminhavel, a fila de viagem e limpa.

### 3.3 Safe spots

Regioes marcadas como `IsSafeSpot` sao usadas como destino quando a Party morre. O Core encontra a regiao safe spot mais proxima da posicao da Party.

---

## 4. Encontros no mapa

Durante viagem, a cada tile percorrido:

1. Verifica se ja esta em combate -> pula.
2. Verifica se esta morto -> pula.
3. Verifica se terreno atual e City -> pula.
4. Calcula chance: `Random.Range(1, 101) + terrain.EncounterRateModifier <= GameData.EncounterProbability`.
5. Se passar, seleciona um encontro aleatorio da regiao atual.
6. Inicia combate via Application.

---

## 5. Acoes do mapa

O menu contextual mostra acoes disponiveis baseadas no tile alvo:

| Acao | Requisito de terreno |
|------|---------------------|
| Move | Qualquer tile caminhavel |
| Mine | `AllowsMining == true` (Mountain) |
| CutWood | `AllowsWoodcutting == true` (Forest) |
| EnterDungeon | `IsPlace == true` (Place ou City) |

O World fornece ao Application a lista de acoes validas para um tile. O Application decide qual acao enfileirar.

Dungeons sao tratadas como encontros fixos: ao entrar, o Application inicia uma sequencia de combates sem deslocamento no mapa.

---

## 6. Dados do Core

### 6.1 WorldState

Estado central do World no Core:

```csharp
public sealed class WorldState
{
    // Mapa: acesso por coordenada
    public TerrainData GetTerrain(Vector2I position);
    public RegionData GetRegion(Vector2I position);

    // Navegacao
    public IReadOnlyList<Vector2I> FindPath(Vector2I from, Vector2I to);
    public bool IsWalkable(Vector2I position);
    public RegionData FindNearestSafeSpot(Vector2I position);

    // Encontros
    public EncounterTemplate SelectRandomEncounter(Vector2I position);
    public bool ShouldTriggerEncounter(Vector2I position, int probability);
}
```

---

## 7. Fluxos criticos

### 7.1 Selecionar tile e agir

1. Input adapter Godot detecta clique direito no tilemap.
2. Traduz coordenada de pixel para `Vector2I` do tile.
3. Chama `WorldState.GetTerrain(position)` e `WorldState.GetRegion(position)`.
4. Application determina acoes validas.
5. UI mostra menu contextual com acoes disponiveis.
6. Jogador seleciona acao.
7. Application enfileira acao na Party.

### 7.2 Viajar

1. Party tem fila de waypoints.
2. Core avanca Party para proximo tile no caminho.
3. A cada tile: verifica encontro.
4. Se encontrar: Application inicia combate.
5. Se chegar ao destino: proxima acao da fila.

### 7.3 Morrer e voltar

1. Party morre em combate.
2. Core limpa fila de acoes e acao atual.
3. Core encontra safe spot mais proximo (`FindNearestSafeSpot`).
4. Party enfileira viagem ate o safe spot.
5. (Heroi vira fantasma - ver regras em biblia e heroi.md)
