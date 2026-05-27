# Macro Full-Screen UI Design

Data: 2026-05-27
Status: aprovado em conversa; aguardando revisao do arquivo antes do plano de implementacao.

## 1. Objetivo

Corrigir a UI de Macros para que ela nao fique espremida no HUD lateral, nao corte controles e possa crescer sem quebrar a tela.

## 2. Problema Atual

A aba `Macros` esta dentro de `UiLayer/Hud/Margin/Tabs`. O HUD tem largura e altura fixas. A lista de controles cresceu, nao ha scroll geral e parte da interface fica fora da area visivel.

Tambem existem janelas separadas para editor de Macro e editor de Program. Isso deixa a experiencia fragmentada e nao corresponde ao fluxo desejado de abrir uma tela grande de Macros.

## 3. Novo Fluxo

Na esquerda da tela existe uma barra estreita de menu com icones. Na v1 desta mudanca, ela tera somente o item `Macros`.

Fluxo:

1. Jogo abre mostrando o HUD/status compacto.
2. Jogador clica no icone `Macros` no canto esquerdo.
3. O HUD/status compacto fica escondido.
4. Abre um modal full-screen de Macros.
5. Jogador edita Macros e Program dentro desse modal.
6. Jogador fecha o modal.
7. O HUD/status compacto volta a aparecer.

## 4. Estrutura Visual

### 4.1 Barra Esquerda

Barra fixa no canto esquerdo da tela.

Regras:

- mostra apenas icones ou botoes compactos;
- nesta etapa tera somente `Macros`;
- nao deve conter a UI de edicao;
- clicar `Macros` abre o modal full-screen.

### 4.2 HUD/Status Compacto

O HUD/status compacto fica separado do menu de Macros.

Regras:

- mostra estado da party, acao atual, proxima acao, erro e botoes `Play`, `Pause`, `Stop`;
- fica visivel no jogo normal;
- fica escondido quando o modal de Macros esta aberto;
- volta ao fechar o modal.

### 4.3 Modal Full-Screen De Macros

O modal ocupa praticamente a tela inteira e fica acima do mapa.

Regras:

- possui botao `Close`;
- bloqueia visualmente a tela de status enquanto aberto;
- possui `ScrollContainer` para nao cortar conteudo;
- contem lista de Macros, controles de gravacao, Program ativo e editores internos;
- nao depende de janelas pequenas separadas para editar Macro ou Program.

## 5. Conteudo Do Modal

O modal de Macros deve conter:

- controles de `Record Macro`, `Save Recording` e `Cancel Recording`;
- campo de nome da Macro;
- lista de Macros salvas da party;
- painel/editor da Macro selecionada com rename, lista de acoes, mover/remover acao, editar `MoveTo`, editar repeticao de `Mine`/`CutWood`;
- lista do Program ativo;
- painel/editor do Program com adicionar Macro selecionada, mover/remover steps, editar repeticao do step e repeticao do Program inteiro;
- todos os controles de repeticao: `Once`, `Forever`, `Count`, `Duration`.

## 6. Arquitetura

Godot continua sendo adaptador de UI.

Regras:

- Core nao referencia Godot;
- UI nao decide regra de validade de jogo;
- UI chama `GameApplication` para gravar, editar e executar;
- validacoes de texto da UI podem existir apenas para evitar crash de parse;
- estado do modal e visibilidade do HUD ficam no adapter Godot.

## 7. Documentacao

Atualizar `docs/arquitetura/ui.md` apos implementar:

- documentar barra esquerda de menus;
- documentar que `Macros` abre modal full-screen;
- documentar que o HUD/status some enquanto o modal esta aberto;
- documentar que editores de Macro e Program vivem dentro do modal;
- remover ou ajustar notas antigas sobre aba `Macros` dentro do HUD.

## 8. Testes E Verificacao

Verificar:

- build .NET sem erros;
- testes Core continuam passando;
- cena Godot compila;
- quando possivel, smoke Godot headless;
- se o executavel Godot documentado continuar ausente, registrar a limitacao.

## 9. Fora Do Escopo

Nao implementar agora:

- novos menus alem de `Macros`;
- persistencia save/load;
- `TransferItem` em Macro;
- condicoes avancadas;
- grupos/sub-blocos;
- Dungeon/EnterDungeon.
