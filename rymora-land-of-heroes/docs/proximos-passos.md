# Proximos Passos - Rymora Land of Heroes

Data: 2026-05-26
Ultimo commit publicado: `edc2c50 feat: validate content and clean map tiles`

---

## Estado atual

- Projeto Godot C# prototipo criado e publicado em `origin/main`.
- Core puro em `src/Core`, sem referencias Godot.
- Adapter Godot em `src/Godot`, com Bootstrap, mapa, HUD, menu contextual e combate visual.
- Config e conteudo carregam de JSON em `assets/data`.
- Regioes carregam de `assets/data/world/regions.json`, com safe spot, modificador de chance e encounters por bioma/terreno.
- Zonas carregam de `assets/data/world/zones.json`, com nivel e modificador de chance.
- `TerrainLayer`, `RegionLayer` e `ZoneLayer` usam TileSets editaveis e possuem mapa inicial pintado na cena; `DemoTileMapBuilder` fica apenas como fallback quando nao ha tiles pintados.
- Atlas visual dos tiles hexagonais foi limpo: sem texto/labels dentro dos hexagonos e com celulas preenchidas para reduzir espacamento visual.
- Validacao inicial de conteudo JSON foi implementada no loader Godot e falha cedo para referencias invalidas e atlas coords duplicadas.
- Party ja possui fila de acoes com `Travel`, `Mine`, `CutWood` e `TransferItem`.
- Acoes ja podem terminar por contagem, quantidade de item ou tempo.
- UI atual ainda e provisoria: clique esquerdo move limpando fila; clique direito abre menu contextual com Move/Mine/Cut Wood.
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

### 1. Programacao de acoes no mapa

Objetivo: substituir o fluxo provisorio de clique/menu por um fluxo mais proximo do final para programar a fila de acoes da party selecionada.

Arquivos principais:

- `docs/regras/biblia-rpg.md`
- `docs/arquitetura/party.md`
- `docs/arquitetura/ui.md`
- `docs/arquitetura/mundo.md`
- `src/Core/Party/PartyAction.cs`
- `src/Core/Application/GameApplication.cs`
- `src/Godot/Bootstrap/Bootstrap.cs`
- `src/Godot/Presentation/HudPresenter.cs`
- `src/Godot/World/WorldTileMapAdapter.cs`

Escopo a detalhar antes de codigo:

- Fluxo ideal do jogador desde clicar no mapa ate a party comecar a executar.
- Como escolher acao e alvo: mover, minerar, cortar madeira e transferir itens.
- Como escolher modo de repeticao: executar uma vez, repetir para sempre, repetir por quantidade ou repetir por tempo.
- Como a nova acao interage com a fila atual: limpar, enfileirar, inserir, substituir ou editar.
- Como mostrar requisitos, caminho, duracao, repeticao, invalidos e falhas.
- Como a HUD apresenta fila, acao atual e progresso sem colocar regra de jogo na UI.
- Core continua sem tipos Godot; Godot so converte input e apresenta estado.

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

### 4. Dados e validacao de conteudo

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

### 5. TODO tardio: Dungeon/EnterDungeon

Objetivo: manter dungeon fora do foco atual. `Dungeon`/`EnterDungeon` so deve voltar depois que programacao de acoes no mapa, viagem, coleta, encontros e HUD estiverem mais proximos do resultado final.

Notas:

- Nao implementar dungeon agora.
- Nao expor `EnterDungeon` como acao do menu contextual atual.
- Quando voltar ao escopo, tratar como design proprio antes de codigo.

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
Continue o prototipo Godot C# de Rymora Land of Heroes. Leia primeiro docs/proximos-passos.md, docs/arquitetura/visao-geral.md e docs/regras/biblia-rpg.md. Mantenha Core puro sem Godot refs. Proximo foco: detalhar e implementar programacao de acoes no mapa para party selecionada. Nao implementar dungeon agora; `Dungeon`/`EnterDungeon` e TODO tardio. Antes de codigo, feche o fluxo do jogador desde clicar no mapa ate a party comecar a executar.
```
