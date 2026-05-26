# Map Macros Design

Data: 2026-05-26
Status: aprovado para especificacao; aguardando revisao do arquivo pelo criador antes do plano de implementacao.

## 1. Objetivo

Criar o fluxo final de programacao de acoes no mapa para uma party, usando Macros reutilizaveis e um Program ativo por party.

O jogador deve poder:

- gravar uma Macro sem mover a party;
- salvar a Macro com nome na lista da party;
- editar Macros depois;
- montar o Program ativo arrastando Macros salvas para uma sequencia linear;
- configurar repeticao no uso de cada Macro dentro do Program;
- controlar execucao com Play, Pause e Stop;
- ver status, acao atual, proxima acao e erros no painel da party.

## 2. Escopo v1

Incluido:

- Macros especificas da party selecionada.
- Um Program ativo por party.
- Macros e Program em memoria durante a sessao.
- Record Macro pelo menu/clique normal do mapa.
- Acoes de Macro: `MoveTo`, `Mine`, `CutWood`.
- Repeticoes v1: uma vez, infinito, X vezes, por tempo.
- Program linear, sem grupos ou sub-blocos.
- Drag-and-drop para ordenar Macros no Program.
- Edicao de Macro em tela maior.
- Falha de requisito para o Program e mostra erro visivel.

Fora da v1, mas documentado como TODO:

- Persistir Macros e Program no save/load.
- `TransferItem` dentro de Macros.
- Condicoes avancadas, como executar ate ter X itens.
- Grupos/sub-blocos dentro do Program.
- `Dungeon`/`EnterDungeon`, que permanece TODO tardio.

## 3. Conceitos

### 3.1 Macro

Uma Macro e um bloco salvo da party. Ela possui nome e lista ordenada de acoes.

A Macro nao define repeticao propria. Repeticao existe em dois lugares:

- na acao produtiva dentro da Macro;
- no uso da Macro dentro do Program.

### 3.2 Program

O Program e o "pai de todos" da party. Ele define a sequencia linear de Macros que a party executa.

Cada party tem somente um Program ativo na v1.

O Program possui configuracao propria de repeticao:

- uma vez;
- infinito;
- X vezes;
- por tempo.

### 3.3 Program Step

Um Program Step e um uso de uma Macro dentro do Program.

Ele referencia a Macro salva por id, nao por snapshot. Isso permite que editar a Macro atualize seus usos no Program. Se uma Macro for editada enquanto esta em execucao, a mudanca vale somente na proxima vez que essa Macro comecar.

Cada Program Step possui sua propria repeticao:

- uma vez;
- infinito;
- X vezes;
- por tempo.

## 4. Modelo De Dominio

O modelo deve ficar no Core, sem referencias a Godot.

Tipos previstos:

```csharp
public sealed class PartyMacro
{
    public string Id { get; }
    public string Name { get; }
    public IReadOnlyList<MacroAction> Actions { get; }
}

public sealed class PartyProgram
{
    public IReadOnlyList<ProgramStep> Steps { get; }
    public RepeatPolicy Repeat { get; }
}

public sealed class ProgramStep
{
    public string MacroId { get; }
    public RepeatPolicy Repeat { get; }
}

public abstract class MacroAction
{
    public string Id { get; }
}
```

A estrutura exata pode mudar durante a implementacao, mas as fronteiras devem permanecer:

- Core guarda Macros, Program e estado de execucao.
- Godot apresenta e converte input.
- UI nao decide regra de validade nem execucao.

## 5. Acoes Da Macro

### 5.1 MoveTo

`MoveTo` recebe uma `TilePosition` absoluta.

Regras:

- sempre executa uma vez;
- nao possui repeticao ou condicao propria;
- aparece como acao separada no editor;
- pode ser editado, reordenado ou removido;
- se o destino ja for o tile atual, termina praticamente instantaneo;
- usa o mesmo pathfinding e fluxo de viagem ja existente.

### 5.2 Mine

`Mine` executa mineracao no tile atual quando a acao roda.

Regras:

- possui repeticao propria;
- usa regras existentes de terreno, skill e coleta;
- falha por requisito para o Program na v1.

### 5.3 CutWood

`CutWood` executa corte de madeira no tile atual quando a acao roda.

Regras:

- possui repeticao propria;
- usa regras existentes de terreno, skill e coleta;
- falha por requisito para o Program na v1.

## 6. Record Macro

O jogador inicia gravacao pelo botao `Record Macro` na area `Macros`.

Fluxo:

1. Jogador seleciona uma party.
2. Jogador abre a aba lateral `Macros`.
3. Jogador clica `Record Macro`.
4. O jogo entra em modo record para aquela party.
5. A party nao se move enquanto o modo record estiver ativo.
6. O jogador clica no mapa e escolhe acoes pelo menu normal.
7. Em vez de executar, o sistema adiciona acoes na Macro em gravacao.
8. Para `Mine` e `CutWood`, o sistema sempre adiciona `MoveTo X,Y` antes da acao produtiva.
9. Jogador salva com nome ou cancela.
10. `Save Macro` exige nome e salva a Macro na lista da party.
11. `Cancel` descarta a gravacao.

Exemplo de gravacao:

1. Jogador esta gravando.
2. Clica em uma montanha e escolhe `Mine`.
3. Macro recebe `MoveTo (10, 4)`.
4. Macro recebe `Mine`.
5. Clica em uma floresta e escolhe `CutWood`.
6. Macro recebe `MoveTo (13, 6)`.
7. Macro recebe `CutWood`.

O sistema sempre grava o `MoveTo`, mesmo se o destino for igual a posicao atual, para evitar comportamento implicito fragil.

## 7. Interface

### 7.1 Painel Principal Da Party

O painel principal deve ficar compacto e sempre mostrar:

- status da party;
- acao atual;
- proxima acao;
- controles `Play`, `Pause`, `Stop`.

### 7.2 Abas Laterais

O painel lateral possui abas:

- `Status`;
- `Macros`.

A aba `Macros` concentra a lista de Macros da party e o Program ativo.

### 7.3 Aba Macros

A aba `Macros` mostra:

- lista de Macros salvas da party;
- botao `Record Macro`;
- Program ativo da party;
- sequencia linear de Program Steps;
- repeticao configurada em cada Program Step.

O jogador pode:

- arrastar uma Macro salva para o Program;
- reordenar Program Steps por drag-and-drop;
- remover Program Steps;
- abrir editor grande para configuracoes complexas.

### 7.4 Macro Editor

O editor grande de Macro permite:

- renomear a Macro;
- ver a lista de acoes;
- reordenar acoes;
- remover uma acao inteira;
- editar destino de `MoveTo`;
- editar repeticao de `Mine` e `CutWood`.

`MoveTo` aparece como acao separada, mas nao possui repeticao.

### 7.5 Program Editor

O editor grande de Program permite:

- ver a sequencia completa;
- arrastar Macros para o Program;
- reordenar Program Steps;
- remover Program Steps;
- configurar repeticao de cada Program Step;
- configurar repeticao do Program inteiro.

O Program referencia as Macros salvas. Ele nao copia a lista de acoes da Macro.

## 8. Execucao

O `ProgramRunner` controla o Program ativo da party.

Estados:

- `Idle`: nada rodando.
- `Running`: executando Program.
- `PauseRequested`: pause solicitado; termina a acao atual antes de pausar.
- `Paused`: mantem posicao no Program para `Resume`.
- `StopRequested`: stop solicitado; termina a acao atual antes de parar.
- `Error`: execucao parada por falha de requisito, com mensagem visivel.

Controles:

- `Play`: inicia do comeco quando esta `Idle` ou `Error`.
- `Play` em `Paused`: funciona como `Resume`.
- `Pause`: solicita pausa cooperativa; nao corta acao no meio.
- `Stop`: solicita parada cooperativa; termina a acao atual, encerra o Program e volta o ponteiro para o comeco.

Execucao normal:

1. Program inicia no primeiro Program Step.
2. Program Step resolve a Macro pelo id.
3. Runner captura a versao da Macro no momento em que ela comeca.
4. Acoes da Macro executam em ordem.
5. Repeticao da acao produtiva e aplicada dentro da Macro.
6. Repeticao do Program Step e aplicada ao uso da Macro.
7. Ao terminar o Program Step, avanca para o proximo.
8. Ao terminar o ultimo Program Step, aplica repeticao do Program inteiro.
9. Se nao houver repeticao restante, volta para `Idle`.

Regra de edicao durante execucao:

- O Program Step referencia a Macro salva.
- A Macro em execucao usa a versao capturada quando ela comecou.
- Edicoes feitas durante a execucao valem na proxima vez que essa Macro comecar.

## 9. Falhas

Na v1, qualquer falha de requisito interrompe a execucao do Program.

Exemplos:

- caminho invalido;
- destino inexistente;
- terreno nao mineravel para `Mine`;
- terreno sem madeira para `CutWood`;
- Macro referenciada nao existe;
- Program vazio ao apertar `Play`.

Resultado:

- Runner entra em `Error`;
- party fica idle;
- erro fica visivel na UI;
- fila atual nao continua automaticamente.

Esse comportamento e intencional para ajudar a debugar erros de codigo e configuracao durante o prototipo.

## 10. Data Flow

1. UI captura intencao do jogador.
2. Godot converte coordenadas do mapa para `TilePosition`.
3. Application recebe comandos de automacao.
4. Core altera Macros, Program ou Runner.
5. Runner executa usando sistemas existentes de viagem e coleta.
6. HUD le estado atual e apresenta status, acao atual, proxima acao e erro.

Comandos esperados no Application:

- iniciar/cancelar/salvar Record Macro;
- adicionar acao de mapa na Macro em gravacao;
- renomear Macro;
- remover/reordenar/editar acao de Macro;
- adicionar/remover/reordenar Program Step;
- configurar repeticao de Program Step;
- configurar repeticao do Program;
- Play/Pause/Stop.

## 11. Testes Do Core

Testes principais:

- gravar `Mine` adiciona `MoveTo` antes de `Mine`;
- gravar `CutWood` adiciona `MoveTo` antes de `CutWood`;
- `MoveTo` nao aceita repeticao;
- Macros pertencem a party especifica;
- Program possui somente uma sequencia linear;
- Program Step referencia Macro por id, nao por snapshot permanente;
- edicao de Macro vale na proxima entrada da Macro, nao no meio da execucao atual;
- Program executa Program Steps em ordem;
- Program Step respeita repeticao uma vez, infinito, X vezes e por tempo;
- Program inteiro respeita repeticao uma vez, infinito, X vezes e por tempo;
- fim do Program fica `Idle` por padrao;
- `Pause` espera acao atual terminar e preserva posicao para `Resume`;
- `Stop` espera acao atual terminar e volta ponteiro para o comeco;
- falha de requisito coloca Runner em `Error` e expoe mensagem.

## 12. TODOs Registrados

- Persistir Macros, Program e estado relevante no save/load.
- Adicionar `TransferItem` ao Record Macro e ao editor.
- Adicionar condicoes avancadas de parada, como ate ter X itens.
- Avaliar grupos/sub-blocos dentro do Program quando a sequencia linear nao for suficiente.
- Reabrir design de `Dungeon`/`EnterDungeon` somente depois que automacao de mapa, viagem, coleta, encontros e HUD estiverem mais proximos do resultado final.
