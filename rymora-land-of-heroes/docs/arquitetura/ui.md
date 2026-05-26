# UI - Rymora Land of Heroes

Data de criacao: 2026-05-23
Regras de negocio: `docs/regras/biblia-rpg.md` (secao Interface de Jogo Enquanto Regra de Produto)
Arquitetura geral: `docs/arquitetura/visao-geral.md`

---

## 1. Proposito

O dominio **UI** cobre apresentacao de estado do jogo e captura de intencao do jogador. UI nao possui regras de jogo - ela exibe o que o Core informa e envia comandos para o Application.

---

## 2. Telas

Duas telas na v1:

| Tela | Visivel quando |
|------|---------------|
| **Map** | Party selecionada nao esta em combate (ou nenhuma combate ativo) |
| **Combat** | Party selecionada esta em combate |

A alternancia e controlada pelo Application. UI apenas reage ao estado.

---

## 3. Painels

### 3.1 Map screen

- Mapa (tilemap Godot com camera).
- Hero panel: nome, nivel, barra de vida, fila de acoes, progresso da acao atual.
- Mini inventario: itens agrupados do grupo selecionado.
- Botao de status (alterna painel de status detalhado).
- Botao de combate (alterna para tela de combate).

Implementacao atual:
- `HudPresenter` mostra party selecionada, tela atual, posicao, acao atual, fila pendente e inventario.
- A aba `Macros` permite iniciar `Record Macro`, salvar com nome, listar Macros da party e montar o Program ativo.
- O painel de status mostra estado do Program, acao atual, proxima acao de Macro, erros e botoes `Play`, `Pause` e `Stop`.
- HUD fica em `UiLayer/Hud` na cena `scenes/bootstrap.tscn`.
- `Bootstrap` sincroniza HUD a cada `_Process` usando estado do Core.

### 3.2 Combat screen

- Corpos de herois e monstros.
- Barras de vida de cada participante.
- Barras de progresso de ataque (cooldown).
- Texto flutuante de dano/cura/critico.
- Botao de estrategia (placeholder na v1).

Implementacao atual provisoria:
- `CombatPresenter` desenha overlay com herois, monstros e barras de vida.
- `CombatPresenter` mostra historico com os ultimos eventos do combate.
- Encontros de viagem alternam automaticamente para tela de combate quando o Core inicia combate.
- Eventos de combate sao logados no console Godot.
- Ao terminar combate, overlay some e party volta ao mapa.

### 3.3 Menu contextual (Map)

Ao clicar com botao direito no mapa, mostra acoes disponiveis:
- Move (sempre disponivel em tile caminhavel).
- Mine (se terreno permite mineracao).
- CutWood (se terreno permite corte).

Implementacao atual:
- `PopupMenu` em `scenes/bootstrap.tscn` mostra acoes do tile clicado.
- `Move` aparece em tile caminhavel.
- `Mine` aparece em tile com `AllowsMining`.
- `Cut Wood` aparece em tile com `AllowsWoodcutting`.
- `Mine` e `Cut Wood` enfileiram travel ate o tile alvo antes da coleta quando a party esta em outro tile.
- Durante `Record Macro`, cliques no mapa e no menu contextual gravam acoes no Macro em edicao em vez de executa-las na fila da party.

Fluxo de Macros:
- `Record Macro` inicia uma sessao de gravacao para a party selecionada.
- Clique esquerdo no mapa grava `MoveTo` quando a party esta gravando; fora da gravacao, continua enviando movimento imediato.
- Clique direito abre o menu contextual; durante a gravacao, `Mine` e `Cut Wood` gravam acoes de Macro em vez de enfileirar coleta.
- Salvar exige nome e cria um Macro da party com as acoes gravadas.
- A aba `Macros` lista Macros salvos, adiciona Macros ao Program ativo e abre editores para ordenar/remover acoes e ajustar repeticoes.
- A UI continua sem regra de jogo: ela mostra opcoes e envia intencoes; Application/Core validam requisitos e execucao.

TODO tardio:
- `Dungeon`/`EnterDungeon` nao faz parte do foco atual e nao deve aparecer como acao imediata ou acao de Macro nesta etapa.

---

## 4. Input

| Acao | Entrada |
|------|---------|
| Selecionar Party 1 | F1 |
| Selecionar Party 2 | F2 |
| Selecionar Party 3 | F3 |
| Mover imediato / gravar `MoveTo` | Clique esquerdo no mapa |
| Abrir menu contextual | Clique direito no mapa |
| Alternar para combate | Botao Combat |
| Alternar status | Botao Status |

Toda entrada e capturada por adaptadores Godot e convertida em intencoes para o Application.

Implementacao atual:
- `Bootstrap._UnhandledInput` captura clique esquerdo.
- `Bootstrap._UnhandledInput` captura clique direito e abre menu contextual.
- `WorldTileMapAdapter.ToTilePosition(Vector2 worldPosition)` converte posicao do mouse para `TilePosition`.
- `GameApplication.EnqueueAction` recebe `PartyActionRequest` de `Travel` com `Destination`.
- Fora de `Record Macro`, clique esquerdo limpa a fila atual da party para o movimento responder imediatamente ao jogador.
- Quando a party esta em `Record Macro`, clique esquerdo grava `MoveTo` e o menu contextual grava `Mine`/`CutWood`.
- Destino invalido imprime log com posicao atual, destino, caminhabilidade e tamanho do path.
- `PartyPresenter` sincroniza a posicao visual da party apos cada `Update`.
- `PartyPresenter` interpola entre tile atual e proximo waypoint usando progresso da acao, sem mudar regra tile-based do Core.

---

## 5. Estrutura

```csharp
// Intencoes do jogador (enviadas para Application)
public sealed record SelectPartyIntent(string PartyId);
public sealed record ExecuteMapActionIntent(TilePosition Position, ActionType Action);
public sealed record ToggleCombatScreenIntent();
public sealed record ToggleStatusPanelIntent();

// Estado que UI consome do Core
public sealed record UIState(
    Screen CurrentScreen, // Map, Combat
    PartyInfo SelectedParty,
    IReadOnlyList<ActionType> AvailableActions,
    CombatInfo CurrentCombat
);
```

UI nunca modifica estado diretamente. UI envia intencoes -> Application processa -> Core atualiza estado -> UI reage.
