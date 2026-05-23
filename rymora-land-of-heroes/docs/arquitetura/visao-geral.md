# Visao geral da arquitetura - Rymora Land of Heroes

Data de criacao: 2026-05-23
Stack: Godot 4.6 + C# (.NET), Jolt Physics (3D desabilitado)
Projeto: `rymora-land-of-heroes/`

Regras de negocio: `docs/regras/biblia-rpg.md`
Referencia de comportamento legado: projeto Unity em `../Rymora Idle/`

---

## 1. Proposito

Este documento define a arquitetura do jogo Rymora Land of Heroes, construido do zero em Godot com C#. O codigo legado em Unity serve apenas como referencia de regras e comportamento -- nenhum codigo sera reaproveitado.

Para cada regra de negocio, consulte a biblia. Para detalhes tecnicos por dominio, consulte os arquivos especificos em `docs/arquitetura/`.

---

## 2. Stack

| Camada | Tecnologia |
|--------|-----------|
| Engine | Godot 4.6 |
| Linguagem | C# (.NET) |
| Fisica | Godot Physics 2D (padrao) |
| Renderizacao | Forward Plus (padrao Godot 4) |

O projeto e 2D topdown. Duas telas: mapa (exploracao) e combate. Nao havera uso de fisica 3D, cameras 3D ou mesh 3D.

---

## 3. Regra de dependencia

A arquitetura segue uma regra unidirecional:

```
Core (C# puro) -> nada sabe sobre Godot
Adaptadores Godot -> podem chamar Core
UI -> apresenta estado, captura intencao, nao possui regras
```

- **Core**: regras de jogo, estado, formulas. Zero referencias a Godot.
- **Adaptadores Godot**: traduzem entrada, cena, assets, fisica em chamadas para o Core.
- **UI**: apresenta estado do Core e envia intencoes do jogador. Nao possui logica de jogo.

Se uma mudanca colocar regra de jogo dentro de um adaptador, presenter, spawner, ou painel de UI, a fronteira deve ser revista.

---

## 4. Dominios

O sistema e dividido nestes dominios:

| Dominio | Responsabilidade | Arquivo |
|---------|-----------------|---------|
| **World** | Mapa, terrenos, regioes, navegacao, encontros no mapa | `docs/arquitetura/mundo.md` |
| **Party** | Grupo, fila de acoes, inventario, viagem, coleta | `docs/arquitetura/party.md` |
| **Combat** | Instancia de combate, turnos, cooldowns, alvo, dano, efeitos | `docs/arquitetura/combate.md` |
| **Hero** | Atributos, pericias, propriedades, equipamento, progressao | `docs/arquitetura/heroi.md` |
| **Content** | Catalogos de itens, armas, armaduras, monstros, encontros, terrenos | `docs/arquitetura/conteudo.md` |
| **UI** | Telas, paineis, input, menu contextual, HUD | `docs/arquitetura/ui.md` |
| **Data** | Save/load, configuracao, progressao persistente | `docs/arquitetura/dados.md` |
| **Application** | Bootstrap, casos de uso, coordenacao entre dominios | `docs/arquitetura/aplicacao.md` |

Nenhum dominio pode importar ou depender de outro dominio no nivel de codigo Core. A comunicacao entre dominios acontece atraves do Application ou por eventos.

---

## 5. Padroes arquiteturais

### 5.1 Core puro vs Godot

- Todo codigo em `src/Core/` e C# puro (`.cs` files sem referencias Godot).
- Todo codigo em `src/Godot/` sao adaptadores, presenters, spawners que referenciam Godot.
- O Core nunca referencia Godot. Adaptadores Godot referenciam Core.

### 5.2 Eventos

Mudancas de estado no Core sao propagadas para adaptadores Godot via eventos. Adaptadores nunca chamam Core para "descobrir" se algo mudou -- eles reagem a eventos e consultam o estado atual.

### 5.3 Servicos e injecao

Preferir servicos estaticos ou singleton por cena para adaptadores Godot. Core usa classes puras instanciaveis. A composicao dos servicos Godot + Core acontece no bootstrap.

### 5.4 Assets e conversao

Assets Godot (cenas, texturas, recursos) sao carregados por adaptadores e convertidos em dados do Core. O Core nunca acessa Resources, preloads ou caminhos de arquivo.

---

## 6. Estrutura de pastas

```
rymora-land-of-heroes/
|-- docs/
|   |-- arquitetura/       # Documentacao tecnica por dominio
|   |   |-- visao-geral.md (este arquivo)
|   |   |-- mundo.md
|   |   |-- party.md
|   |   |-- combate.md
|   |   |-- heroi.md
|   |   |-- conteudo.md
|   |   |-- ui.md
|   |   |-- dados.md
|   |   `-- aplicacao.md
|   `-- regras/
|       `-- biblia-rpg.md
|-- src/
|   |-- Core/              # C# puro, sem ref Godot
|   |   |-- World/
|   |   |-- Party/
|   |   |-- Combat/
|   |   |-- Hero/
|   |   |-- Content/
|   |   `-- Application/
|   |-- Godot/             # Adaptadores, presenters, Godot-specific
|   |   |-- Adapters/
|   |   |-- UI/
|   |   |-- Spawners/
|   |   `-- Bootstrap/
|   `-- Tests/             # Testes (Godot ou NUnit)
|-- assets/                # Arte, audio, fontes
|-- scenes/                # Cenas Godot (.tscn)
`-- project.godot
```

---

## 7. Fluxo de bootstrap

1. Godot carrega a cena principal.
2. Bootstrap (adaptador Godot) cria e configura os servicos do Core.
3. Content loaders carregam catalogos de itens, armas, armaduras, monstros, encontros e terrenos.
4. World adapter leia o tilemap e registra cidades/regioes.
5. Party registry e inicializado com as parties da cena (ou do save).
6. Estado de tela inicial e definido como `Map`.
7. Presenters (camera, HUD, mini-map) sincronizam com o estado do Core.

Detalhes do fluxo de inicializacao serao definidos em `docs/arquitetura/aplicacao.md`.

---

## 8. Docs relacionados

- `docs/regras/biblia-rpg.md` - regras de negocio (fonte da verdade para design)
- `docs/arquitetura/mundo.md` - mapa, terrenos, navegacao
- `docs/arquitetura/party.md` - grupo, acoes, inventario
- `docs/arquitetura/combate.md` - combate, turnos, dano
- `docs/arquitetura/heroi.md` - atributos, pericias, progressao
- `docs/arquitetura/conteudo.md` - itens, armas, monstros
- `docs/arquitetura/ui.md` - telas, input, HUD
- `docs/arquitetura/dados.md` - save, config
- `docs/arquitetura/aplicacao.md` - bootstrap, casos de uso
- Projeto Unity legado em `../Rymora Idle/` - referencia de comportamento
