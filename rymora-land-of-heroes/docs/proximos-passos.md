# Proximos Passos - Rymora Land of Heroes

Data: 2026-05-23
Ultimo commit publicado: `a109ac4 feat: add Godot C# prototype`

---

## Estado atual

- Projeto Godot C# prototipo criado e publicado em `origin/main`.
- Core puro em `src/Core`, sem referencias Godot.
- Adapter Godot em `src/Godot`, com Bootstrap, mapa, HUD, menu contextual e combate visual.
- Config e conteudo carregam de JSON em `assets/data`.
- Regioes carregam de `assets/data/world/regions.json`, com safe spot, modificador de chance e encounters por bioma/terreno.
- Zonas carregam de `assets/data/world/zones.json`, com nivel e modificador de chance.
- `TerrainLayer`, `RegionLayer` e `ZoneLayer` usam TileSets editaveis e possuem mapa inicial pintado na cena; `DemoTileMapBuilder` fica apenas como fallback quando nao ha tiles pintados.
- Testes xUnit em `src/Tests` com ObjectMother.
- Cena principal: `scenes/bootstrap.tscn`.
- `project.godot` aponta para `res://scenes/bootstrap.tscn`.

---

## Verificacoes que passaram

- `dotnet build RymoraLandOfHeroes.sln`
- `dotnet test RymoraLandOfHeroes.sln --no-restore`
- Godot headless smoke test
- `git diff --check`

Resultado esperado do smoke test Godot:

```text
Rymora Godot bootstrap ready. World from TileMapLayer. Mine queued: True.
Rymora Core loop OK. Iron=1. Elapsed=...
```

Comando Godot usado:

```powershell
& "C:\Users\rmttyszka\AppData\Local\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_Microsoft.Winget.Source_8wekyb3d8bbwe\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe" --headless --path "." --quit-after 1
```

---

## Proximas prioridades

### 1. Modelo final de mapa em 3 layers

Objetivo: mapa editavel em tres layers: terreno/bioma, regiao e zona. Regiao define encontros por bioma; zona define nivel.

Arquivos principais:

- `assets/data/world/terrain_tiles.json`
- `assets/art/world/terrain_hex_atlas.png`
- `assets/art/world/region_hex_atlas.png`
- `assets/art/world/zone_hex_atlas.png`
- `assets/world/terrain_hex_tileset.tres`
- `assets/world/region_hex_tileset.tres`
- `assets/world/zone_hex_tileset.tres`
- `assets/data/world/regions.json`
- `assets/data/world/zones.json`
- `assets/data/content/encounters.json`
- `src/Godot/World/DemoTileMapBuilder.cs`
- `src/Godot/World/WorldTileMapAdapter.cs`
- `src/Core/World/RegionData.cs`
- `src/Core/Content/Templates.cs`

Regras:

- Mapa continua usando tilemap hexagonal 2D topdown.
- Cada `TerrainType`/bioma continua tendo tile configuravel no catalogo.
- Regioes vem de `RegionLayer` + `regions.json`.
- Zonas vem de `ZoneLayer` + `zones.json`.
- Encontros sao selecionados por `region + terrain`.
- Nivel vem de `zone`.
- Para editar mapa: abrir `scenes/bootstrap.tscn`, pintar `TerrainLayer`, `RegionLayer` e `ZoneLayer`, salvar a cena.
- Core continua sem tipos Godot.
- Atlas visual de tile deve ficar sem texto, siglas ou labels dentro dos hexagonos; o hexagono deve preencher a celula `96x96` sem padding visual; metadados ficam nos JSONs e em ferramentas de debug.

### 2. Movimento visual suave

Objetivo: manter movimento logico do Core por tile, mas fazer o sprite da party interpolar entre tiles no Godot.

Arquivos principais:

- `src/Godot/Presentation/PartyPresenter.cs`
- `src/Godot/Bootstrap/Bootstrap.cs`
- `src/Core/Party/PartyAction.cs`
- `src/Core/Application/GameApplication.cs`

Regras:

- Core continua avancando tile a tile.
- Godot pode interpolar visualmente usando progresso da acao atual (`CurrentTime / TimeToExecute`).
- Encontros, coleta e regras disparam somente quando Core chega ao proximo tile logico.
- Nao colocar `Vector2I`, `Node` ou `Texture2D` no Core.

### 3. Encontros por viagem

Objetivo: transformar encontros de viagem em fluxo real de jogo, nao apenas menu `Start Combat`.

Arquivos principais:

- `assets/data/game_config.json`
- `assets/data/content/encounters.json`
- `src/Core/World/WorldState.cs`
- `src/Core/Application/GameApplication.cs`
- `src/Godot/Presentation/CombatPresenter.cs`

Notas:

- `WorldState.ShouldTriggerEncounter` ja existe.
- `GameApplication.ExecuteTravel` ja chama `StartCombat` quando o encontro dispara.
- `EncounterProbability` deve ficar configuravel no JSON e nao hardcoded.
- Decidir se `EncounterInterval` continua necessario ou se encontro por tile e suficiente para v1.
- Adicionar testes para chance de encontro configurada via `GameConfig` se comportamento mudar.

### 4. Tileset e mapa menos demo

Objetivo: substituir `DemoTileMapBuilder` por fluxo de mapa mais proximo do final.

Arquivos principais:

- `assets/data/world/terrain_tiles.json`
- `src/Godot/World/DemoTileMapBuilder.cs`
- `src/Godot/World/WorldTileMapAdapter.cs`
- `scenes/bootstrap.tscn`

Opcoes:

- Criar TileSet real no Godot e manter `terrain_tiles.json` como catalogo de metadata.
- Manter builder apenas como fallback de desenvolvimento.
- Adicionar mapa inicial editavel na cena, sem gerar tudo por codigo.

### 5. Dados e validacao de conteudo

Objetivo: falhar cedo quando JSON tiver erro.

Estado atual: validacao inicial implementada no loader de conteudo Godot.

Arquivos principais:

- `src/Godot/Content/JsonGameContentLoader.cs`
- `src/Godot/Content/GameContent.cs`
- `assets/data/**/*.json`

Validacoes implementadas:

- Weapon referenciada por creature existe.
- Creature referenciada por encounter existe.
- Encounter referenciado por region existe.
- Material referenciado por terrain existe.
- Terrain, region e zone atlas coords nao duplicam.

Validacoes futuras uteis:

- Toda regiao nao-safe usada no mapa tem encontros, se encontros forem obrigatorios.

---

## Regras importantes para continuar

- Biblia RPG continua fonte da verdade: `docs/regras/biblia-rpg.md`.
- Regras de negocio ficam no Core.
- Godot so adapta, apresenta e converte tipos.
- Core nao referencia Godot.
- Valores pendentes devem ir para config/conteudo, nao hardcode.
- Testes usam xUnit + ObjectMother.
- Cada teste testa uma coisa.
- Preferir mudancas pequenas, finalistas e verificaveis.

---

## Prompt sugerido para nova sessao

```text
Continue o prototipo Godot C# de Rymora Land of Heroes. Leia primeiro docs/proximos-passos.md, docs/arquitetura/visao-geral.md e docs/regras/biblia-rpg.md. Mantenha Core puro sem Godot refs. Proximo foco: mapa editavel em 3 layers (`TerrainLayer`, `RegionLayer`, `ZoneLayer`), encontros por `region + terrain` e nivel por zone. Depois rode build, testes e Godot headless.
```
