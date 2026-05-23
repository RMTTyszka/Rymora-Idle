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

### 3.2 Combat screen

- Corpos de herois e monstros.
- Barras de vida de cada participante.
- Barras de progresso de ataque (cooldown).
- Texto flutuante de dano/cura/critico.
- Botao de estrategia (placeholder na v1).

### 3.3 Menu contextual (Map)

Ao clicar com botao direito no mapa, mostra acoes disponiveis:
- Move (sempre disponivel em tile caminhavel).
- Mine (se terreno permite mineracao).
- CutWood (se terreno permite corte).
- EnterDungeon (se for Place).

---

## 4. Input

| Acao | Entrada |
|------|---------|
| Selecionar Party 1 | F1 |
| Selecionar Party 2 | F2 |
| Selecionar Party 3 | F3 |
| Abrir menu contextual | Clique direito no mapa |
| Alternar para combate | Botao Combat |
| Alternar status | Botao Status |

Toda entrada e capturada por adaptadores Godot e convertida em intencoes para o Application.

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
