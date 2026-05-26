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

O mapa e uma grade hexagonal 2D topdown. Este e o formato final escolhido para o mapa. Heptagonos nao tessellam o plano sozinhos e octogonos exigem pecas auxiliares; portanto o mapa usa hexagonos.

Cada celula combina tres layers editaveis:

| Componente | Descricao |
|-----------|-----------|
| TerrainLayer | Tipo de terreno/bioma visual e regras do tile (Plain, Forest, Mountain, Volcano, Desert, Jungle, Swamp, Snow, Water, Ice, Hills, Road, City, Place, Mine, Wall) |
| RegionLayer | Regiao geografica do mundo (ex: Montanha do Troll Cinzento), define safe spot, chance local e encontros por bioma |
| ZoneLayer | Zona/sub-regiao dentro da regiao (ex: Borda, Encosta, Pico), define nivel e modificador local de chance |


O mapa e carregado a partir de tres `TileMapLayer` Godot configurados com TileSet hexagonal. O World adapter cruza as tres layers por celula e converte para dados do Core na inicializacao.

Coordenadas do Core usam tipo proprio, sem dependencia Godot:

```csharp
public readonly record struct TilePosition(int X, int Y);
```

O adaptador Godot converte `Vector2I` do tilemap para `TilePosition` na entrada e faz o inverso apenas para renderizacao.

### 2.1 Terrenos

Ver `docs/regras/biblia-rpg.md` para regras de negocio completas.

Terrenos tambem sao usados como biomas visuais do mapa. Cada tipo final de terreno/bioma deve possuir pelo menos um tile hexagonal configuravel no catalogo de tiles. Variacoes visuais podem existir por atlas coords diferentes, mas devem apontar para um mesmo `TerrainType` quando tiverem as mesmas regras.

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
- lista/tabela de encontros por `TerrainType`
- modificador local de chance de encontro
- se e safe spot (sem encontros)

Estrutura no Core:

```csharp
public sealed record RegionData(
    string Id,
    string Name,
    bool IsSafeSpot,
    float EncounterProbabilityModifier,
    IReadOnlyDictionary<TerrainType, IReadOnlyList<EncounterTemplate>> EncountersByTerrain
)
```

Config de regiao fica em JSON dedicado para ser facil balancear sem alterar mapa ou codigo:

```json
{
  "id": "gray-troll-mountain",
  "name": "Montanha do Troll Cinzento",
  "isSafeSpot": false,
  "encounterProbabilityModifier": 0,
  "encountersByTerrain": {
    "Hills": ["young-troll"],
    "Mountain": ["gray-troll-warrior"],
    "Volcano": ["ash-imp"]
  }
}
```

### 2.3 Zonas

Zonas representam profundidade/sub-regiao dentro da regiao. Elas definem nivel e modificador local, sem definir bioma nem lista de encontros.

Estrutura no Core:

```csharp
public sealed record ZoneData(
    string Id,
    string Name,
    int Level,
    float EncounterProbabilityModifier
)
```

Config de zona fica em JSON dedicado:

```json
{
  "id": "deep",
  "name": "Interior",
  "level": 3,
  "encounterProbabilityModifier": 10
}
```

### 2.4 Adapter Godot atual

Implementacao inicial:

- `src/Godot/World/WorldTileMapAdapter.cs` le `TerrainLayer`, `RegionLayer` e `ZoneLayer` e cria `WorldState`.
- `WorldTileMapAdapter.ToTilePosition(Vector2 worldPosition)` converte posicao do mouse em coordenada logica do Core.
- `assets/data/world/terrain_tiles.json` mapeia atlas coords do `TerrainLayer` para `TerrainData`, bioma visual, cor provisoria e material coletavel.
- `assets/data/world/regions.json` mapeia atlas coords do `RegionLayer` para regiao, safe spot, chance local e encontros por terreno.
- `assets/data/world/zones.json` mapeia atlas coords do `ZoneLayer` para zona, nivel e chance local.
- `src/Godot/World/TerrainTileCodes.cs` guarda apenas constantes do TileSet provisorio (`SourceId`, `TileSize`).
- `assets/art/world/terrain_hex_atlas.png` contem atlas visual provisorio dos tiles hexagonais, sem texto ou labels dentro dos hexagonos.
- `assets/world/terrain_hex_tileset.tres` e o TileSet editavel usado pelo `TerrainLayer`.
- `assets/world/region_hex_tileset.tres` e o TileSet editavel usado pelo `RegionLayer`.
- `assets/world/zone_hex_tileset.tres` e o TileSet editavel usado pelo `ZoneLayer`.
- `scenes/bootstrap.tscn` liga as tres layers aos TileSets editaveis e possui mapa inicial pintado na cena.
- `src/Godot/World/DemoTileMapBuilder.cs` preenche um mapa pequeno somente quando o TileMapLayer esta vazio. Se o mapa for pintado e salvo na cena, o fallback nao altera os tiles.
- O demo usa tiles grandes (`96x96`) e poucas paredes internas para deixar movimento visivel durante validacao.

Fluxo para editar mapa no Godot:
- Abrir `scenes/bootstrap.tscn`.
- Selecionar `TerrainLayer` para pintar biomas/terrenos.
- Selecionar `RegionLayer` para pintar regioes.
- Selecionar `ZoneLayer` para pintar zonas/levels.
- Salvar a cena.
- Ao rodar o jogo, `WorldTileMapAdapter` cruza as tres layers pintadas e `DemoTileMapBuilder` nao substitui o mapa.

Regra visual dos atlases:
- Tiles hexagonais do mapa nao devem conter texto, siglas ou labels desenhados no centro.
- Cada hexagono deve ocupar toda a celula `96x96` do TileSet, encostando nas bordas esperadas do tile para nao deixar espacamento transparente entre tiles vizinhos.
- A identificacao de terreno, regiao e zona deve vir de `terrain_tiles.json`, `regions.json`, `zones.json` e das ferramentas de edicao/debug, nao da arte do tile.
- Labels no proprio tile poluem a leitura do mapa jogavel e ficam proibidos mesmo em arte provisoria.

Mapeamento inicial de atlas coords fica em JSON. O proximo passo e cobrir todos os biomas/terrenos do enum, mesmo com arte provisoria:

| Atlas coords | TerrainType |
|--------------|-------------|
| `(0, 0)` | Plain |
| `(1, 0)` | Forest |
| `(2, 0)` | Mountain |
| `(3, 0)` | Mine |
| `(4, 0)` | City |
| `(5, 0)` | Wall |
| `(6, 0)` | Desert |
| `(7, 0)` | Jungle |
| `(8, 0)` | Swamp |
| `(9, 0)` | Snow |
| `(10, 0)` | Water |
| `(11, 0)` | Ice |
| `(12, 0)` | Hills |
| `(13, 0)` | Road |
| `(14, 0)` | Place |
| `(15, 0)` | Volcano |

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
2. Adapter Godot sincroniza a posicao visual e interpola entre tiles usando progresso da acao atual.
3. Ao chegar em cada tile, Core verifica encontro e acoes pendentes.
4. Se o terreno nao for caminhavel, a fila de viagem e limpa.

Regras de apresentacao:
- Core mantem movimento logico por tile.
- Godot interpola visualmente entre tile atual e proximo waypoint.
- Progresso da acao (`CurrentTime / TimeToExecute`) move sprite aos poucos dentro do mesmo passo logico.
- Encontros e regras continuam disparando somente quando a party chega ao proximo tile logico.

### 3.3 Safe spots

Regioes marcadas como `IsSafeSpot` sao usadas como destino quando a Party morre. O Core encontra a regiao safe spot mais proxima da posicao da Party.

---

## 4. Encontros no mapa

Durante viagem, a cada tile percorrido:

1. Verifica se ja esta em combate -> pula.
2. Verifica se esta morto -> pula.
3. Verifica se terreno atual e City -> pula.
4. Calcula chance usando `GameConfig.EncounterProbability`, `terrain.EncounterRateModifier` conforme `EncounterPolicy`, `region.EncounterProbabilityModifier` e `zone.EncounterProbabilityModifier`.
5. Se passar, seleciona um encontro da tabela da regiao filtrada pelo terreno/bioma atual.
6. Inicia combate via Application.

`GameConfig.EncounterProbability` e a chance base global de encontro durante viagem. `RegionData.EncounterProbabilityModifier` ajusta uma regiao especifica. `ZoneData.EncounterProbabilityModifier` ajusta a profundidade/sub-regiao. `TerrainData.EncounterRateModifier` ajusta tiles/biomas especificos. O sinal de `EncounterRateModifier` e configurado por `EncounterModifierMode`.

---

## 5. Acoes do mapa

O mapa fornece contexto para programar a fila de acoes da party selecionada. A UI pode mostrar acoes disponiveis baseadas no tile alvo, mas a decisao final de execucao pertence ao Application/Core.

| Acao | Requisito de terreno |
|------|---------------------|
| Move | Qualquer tile caminhavel |
| Mine | `AllowsMining == true` |
| CutWood | `AllowsWoodcutting == true` (Forest) |

O adapter Godot usa `TerrainData` e metadados de `terrain_tiles.json` para apresentar opcoes e converter o clique em `TilePosition`. A programacao deve produzir `PartyActionRequest` para a fila da party, respeitando os modos confirmados na biblia: uma vez, repetir para sempre, repetir por quantidade definida ou repetir por tempo definido.

Transferencia de itens entre parties tambem e acao programavel, mas depende de selecao de party alvo e inventario, nao apenas de terreno.

Fora do foco atual:
- `Dungeon`/`EnterDungeon` fica como TODO tardio.
- `Place` continua sendo terreno/local especial no mapa, mas nao gera acao de dungeon enquanto esse design nao for retomado.

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
    public ZoneData GetZone(TilePosition position);

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

### 7.1 Programar acao no mapa

1. Input adapter Godot detecta clique no tilemap conforme o fluxo de programacao aprovado.
2. Traduz coordenada de pixel para `TilePosition`.
3. Chama `WorldState.GetTerrain(position)`, `WorldState.GetRegion(position)` e `WorldState.GetZone(position)`.
4. Application/Core determinam quais acoes sao validas para a party selecionada, tile alvo e estado atual.
5. UI apresenta opcoes de acao, requisitos, caminho e modo de repeticao sem duplicar regra.
6. Jogador confirma a programacao.
7. Application enfileira, substitui ou altera a fila da Party conforme a regra aprovada para esse fluxo.
8. Party inicia a proxima acao quando o loop do Core avancar e a fila permitir.

### 7.2 Viajar

1. Party tem fila de waypoints.
2. Core avanca Party para proximo tile no caminho.
3. A cada tile: cruza terreno + regiao + zona.
4. Verifica encontro usando chance combinada e tabela da regiao por terreno.
5. Se encontrar: Application inicia combate.
6. Se chegar ao destino: proxima acao da fila.

### 7.3 Morrer e voltar

1. Party morre em combate.
2. Core limpa fila de acoes e acao atual.
3. Core encontra safe spot mais proximo (`FindNearestSafeSpot`).
4. Party enfileira viagem ate o safe spot.
5. (Heroi vira fantasma - ver regras em biblia e heroi.md)
