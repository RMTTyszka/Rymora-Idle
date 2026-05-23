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

Coordenadas do Core usam tipo proprio, sem dependencia Godot:

```csharp
public readonly record struct TilePosition(int X, int Y);
```

O adaptador Godot converte `Vector2I` do tilemap para `TilePosition` na entrada e faz o inverso apenas para renderizacao.

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

### 2.3 Adapter Godot atual

Implementacao inicial:

- `src/Godot/World/WorldTileMapAdapter.cs` le um `TileMapLayer` e cria `WorldState`.
- `WorldTileMapAdapter.ToTilePosition(Vector2 worldPosition)` converte posicao do mouse em coordenada logica do Core.
- `assets/data/world/terrain_tiles.json` mapeia `atlasCoords` para `TerrainData`, regiao, safe spot, cor provisoria e material coletavel.
- `src/Godot/World/TerrainTileCodes.cs` guarda apenas constantes do TileSet provisorio (`SourceId`, `TileSize`).
- `src/Godot/World/DemoTileMapBuilder.cs` cria um TileSet simples em memoria a partir do catalogo e preenche um mapa pequeno quando o TileMapLayer esta vazio.
- O demo usa tiles grandes (`96x96`) e poucas paredes internas para deixar movimento visivel durante validacao.

Mapeamento inicial de atlas coords fica em JSON:

| Atlas coords | TerrainType |
|--------------|-------------|
| `(0, 0)` | Plain |
| `(1, 0)` | Forest |
| `(2, 0)` | Mountain |
| `(3, 0)` | Mine |
| `(4, 0)` | City |
| `(5, 0)` | Wall |

Esse mapeamento continua provisorio ate existir tileset final com metadata ou pipeline de mapa definitivo, mas nao fica hardcoded no Core nem no world adapter.

---

## 3. Navegacao

### 3.1 Pathfinding

O movimento usa pathfinding em grade hexagonal:

- entrada: `origin (TilePosition)` e `destination (TilePosition)`
- saida: `IReadOnlyList<TilePosition>` (waypoints ate o destino, sem incluir a origem)
- respeita `IsWalkable` dos terrenos
- bloqueios: `Wall` tiles

Implementacao: A* sobre grade hexagonal. Otimizacoes (distancia de Manhattan hexagonal, cache de caminho) adiadas.

### 3.2 Movimento da Party

A Party segue o caminho tile a tile:

1. Core define o caminho e a velocidade no tile atual.
2. Adapter Godot sincroniza a posicao visual; interpolacao entre tiles fica no TODO abaixo.
3. Ao chegar em cada tile, Core verifica encontro e acoes pendentes.
4. Se o terreno nao for caminhavel, a fila de viagem e limpa.

TODO:
- Manter movimento logico do Core por tile.
- Adicionar interpolacao visual no Godot entre tile anterior e proximo tile.
- Usar progresso da acao (`CurrentTime / TimeToExecute`) para mover sprite aos poucos dentro do mesmo tile.
- Encontros e regras continuam disparando somente quando a party chega ao proximo tile logico.

### 3.3 Safe spots

Regioes marcadas como `IsSafeSpot` sao usadas como destino quando a Party morre. O Core encontra a regiao safe spot mais proxima da posicao da Party.

---

## 4. Encontros no mapa

Durante viagem, a cada tile percorrido:

1. Verifica se ja esta em combate -> pula.
2. Verifica se esta morto -> pula.
3. Verifica se terreno atual e City -> pula.
4. Calcula chance usando `EncounterProbability` e `terrain.EncounterRateModifier` conforme `EncounterPolicy`.
5. Se passar, seleciona um encontro aleatorio da regiao atual.
6. Inicia combate via Application.

O sinal de `EncounterRateModifier` e configurado por `EncounterModifierMode`. O valor default final ainda depende de decisao de design.

---

## 5. Acoes do mapa

O menu contextual mostra acoes disponiveis baseadas no tile alvo:

| Acao | Requisito de terreno |
|------|---------------------|
| Move | Qualquer tile caminhavel |
| Mine | `AllowsMining == true` |
| CutWood | `AllowsWoodcutting == true` (Forest) |
| EnterDungeon | `IsPlace == true` (Place ou City) |

O adapter Godot usa `TerrainData` e metadados de `terrain_tiles.json` para montar o menu contextual e enfileirar `PartyActionRequest`. O Core valida a execucao por flags do terreno.

Dungeons sao tratadas como encontros fixos: ao entrar, o Application inicia uma sequencia de combates sem deslocamento no mapa.

---

## 6. Dados do Core

### 6.1 WorldState

Estado central do World no Core:

```csharp
public sealed class WorldState
{
    // Mapa: acesso por coordenada
    public TerrainData GetTerrain(TilePosition position);
    public RegionData GetRegion(TilePosition position);

    // Navegacao
    public IReadOnlyList<TilePosition> FindPath(TilePosition from, TilePosition to);
    public bool IsWalkable(TilePosition position);
    public RegionData FindNearestSafeSpot(TilePosition position);
    public TilePosition FindNearestSafeSpotPosition(TilePosition position);

    // Encontros
    public EncounterTemplate SelectRandomEncounter(TilePosition position);
    public bool ShouldTriggerEncounter(TilePosition position, float baseProbability, EncounterPolicy policy);
}
```

---

## 7. Fluxos criticos

### 7.1 Selecionar tile e agir

1. Input adapter Godot detecta clique direito no tilemap.
2. Traduz coordenada de pixel para `TilePosition`.
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
