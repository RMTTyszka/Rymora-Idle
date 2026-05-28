# Full Game Save/Load Design

Data: 2026-05-28
Status: aprovado em conversa; aguardando revisao do arquivo antes do plano de implementacao.

## 1. Objetivo

Implementar save/load completo do estado jogavel atual de Rymora Land of Heroes.

O jogador deve poder fechar o jogo e voltar com o mesmo estado logico do Core: parties, herois, inventario, fila de acoes, automacao, combate ativo e selecao atual.

## 2. Escopo

Salvar tudo que define o runtime jogavel atual:

- versao do save;
- data/hora do save;
- tempo jogado; se o runtime ainda nao tiver contador, adicionar `PlayTimeSeconds` em `GameApplication`;
- party selecionada;
- tela atual (`Map` ou `Combat`);
- todas as parties conhecidas;
- posicao de cada party;
- estado vivo/morto/em combate de cada party;
- membros/herois de cada party;
- vida atual e vida maxima de cada criatura;
- atributos, pericias e propriedades;
- equipamento por slot usando ids/dados de conteudo;
- inventario da party;
- acao atual da fila, incluindo progresso parcial;
- acoes pendentes da fila;
- automacao da party: recording, macros, program ativo e runner;
- combates ativos, incluindo herois, monstros, estado e cooldowns atuais.

Nao salvar estado tecnico que nao e regra de jogo:

- nodes, scenes, `Vector2I`, `Texture2D`, `Resource` ou outros tipos Godot;
- delegates/event subscribers;
- posicao interpolada do sprite entre tiles, porque ela e derivada da acao atual;
- historico visual/log de combate, salvo se virar regra persistente depois;
- estado interno de RNG, exceto se o projeto adicionar um random source serializavel.

## 3. Principios

- Core continua puro, sem referencia Godot.
- Serializacao JSON fica no adapter Godot.
- Save usa DTOs simples e versionados.
- Load valida antes de aplicar estado.
- Erro de save/load deve ser claro e falhar cedo.
- TDD obrigatorio: cada comportamento novo nasce com teste falhando primeiro.
- Documentacao deve ser atualizada antes e depois da implementacao.

## 4. Arquitetura

### 4.1 Core

Adicionar dominio `Data` em `src/Core/Data`.

Componentes:

- `SaveData`: raiz versionada do save.
- DTOs de party, hero, stats, inventory, queue, automation e combat.
- `SaveSnapshotBuilder`: cria `SaveData` a partir de `GameApplication`.
- `SaveRestorer`: recria/aplica estado de Core a partir de `SaveData`.
- `SaveValidation`: valida versao, ids e referencias obrigatorias antes do restore.

O Core nao le arquivo. O Core nao conhece JSON. O Core so transforma estado em DTO e DTO em estado.

### 4.2 Godot Adapter

Adicionar adapter de armazenamento em `src/Godot/Data`.

Componentes:

- `JsonSaveStore`: serializa/deserializa `SaveData` com `System.Text.Json`.
- Caminho default: `user://saves/save-1.json`.
- `Bootstrap`: tenta carregar save existente no start; salva no quit e em intervalo configuravel por `GameConfig`.

UI manual de save/load fica fora desta fatia.

## 5. Modelo De Dados

### 5.1 SaveData

Campos minimos:

- `SaveVersion` (`"1"` na v1);
- `SavedAtUtc`;
- `PlayTimeSeconds`;
- `SelectedPartyId`;
- `CurrentScreen`;
- `Parties`;
- `ActiveCombats`.

### 5.2 PartySaveData

Salvar:

- `PartyId`;
- `Position`;
- `IsInCombat`;
- `Members`;
- `Inventory`;
- `ActionQueue`;
- `Automation`.

`IsAlive` e `IsDefeated` nao sao salvos como fonte de verdade. O restore deriva ambos da vida dos membros.

### 5.3 CreatureSaveData

Salvar:

- `Name`;
- `Life`;
- `MaxLife`;
- `Sprite`;
- `Attributes`;
- `Skills`;
- `Properties`;
- `Equipment`.

Stats salvam pontos e divisor para restaurar exatamente o valor atual.

Equipamento salva referencia por conteudo quando possivel. Se o equipamento atual ainda nao tiver ids estaveis suficientes, salvar os campos completos do template para nao perder estado.

### 5.4 ActionQueueSaveData

Salvar:

- acao atual, se existir;
- `CurrentTime`;
- `PassedTime`;
- `ExecutedCount`;
- `Started`;
- acoes pendentes em ordem.

`PartyActionRequest` deve salvar todos seus campos, incluindo path calculado e `AutomationActionId`, para que load continue a acao do mesmo ponto.

### 5.5 AutomationSaveData

Salvar:

- recording ativo, incluindo id e acoes gravadas;
- macros salvos, nomes e acoes em ordem;
- program ativo, repeat do program e steps em ordem;
- runner state completo: estado, erro, indices, iteracoes, tempos acumulados, acao atual e execution id.

Se campos internos do runner nao tiverem acesso publico, adicionar API de snapshot/restore no Core. Nao usar reflection.

### 5.6 CombatSaveData

Salvar:

- `PartyId` dono do combate;
- estado do combate;
- herois participantes por referencia aos membros da party;
- monstros participantes completos;
- cooldown atual e total por arma;
- estado atual de vida de cada combatente.

Eventos ja disparados e subscribers nao entram no save. Depois do load, presenters Godot reassinam eventos como parte do bootstrap normal.

## 6. Fluxo Save

1. `Bootstrap` pede snapshot para `GameApplication`.
2. `GameApplication` chama `SaveSnapshotBuilder`.
3. Builder percorre parties, queues, automation e combates ativos.
4. Builder retorna `SaveData` validado.
5. `JsonSaveStore` escreve JSON em arquivo temporario.
6. Store substitui `save-1.json` de forma atomica quando possivel.

## 7. Fluxo Load

1. `Bootstrap` procura `user://saves/save-1.json`.
2. Se nao existe, cria jogo novo pelo fluxo atual.
3. Se existe, `JsonSaveStore` le JSON.
4. `SaveValidation` valida versao e estrutura.
5. `SaveRestorer` cria/restaura `GameApplication`.
6. Presenters sincronizam do estado restaurado como fazem no bootstrap normal.

Load invalido nao deve deixar jogo meio restaurado. Se falhar, bootstrap aborta com erro claro e nao sobrescreve o arquivo salvo.

## 8. Erros E Validacao

Validar:

- versao suportada;
- ids de party unicos;
- `SelectedPartyId` existe;
- `ActiveCombat.PartyId` existe;
- paths e tile positions sao validos no mundo atual;
- macros/steps referenciam macros existentes;
- repeat policies possuem dados exigidos;
- action requests possuem campos obrigatorios para seu tipo;
- inventario nao possui quantidade invalida;
- criatura possui stats completos para todos enums atuais;
- equipamento referencia conteudo existente ou carrega template completo salvo.

Erro deve citar campo e id afetado quando possivel.

## 9. Testes

TDD por comportamento.

Testes Core esperados:

- snapshot salva posicao, inventario e membros da party;
- restore recria party na posicao salva;
- snapshot salva fila atual com progresso parcial;
- restore continua acao atual com `CurrentTime`, `PassedTime` e `ExecutedCount`;
- snapshot/restore preserva macros e program;
- snapshot/restore preserva runner em `Running`, `Paused`, `Error` e com erro;
- snapshot/restore preserva recording ativo;
- snapshot/restore preserva combate ativo e cooldowns;
- validation rejeita versao desconhecida;
- validation rejeita selected party inexistente;
- validation rejeita action request incompleto.

Testes Godot adapter esperados quando viavel sem engine runtime:

- `JsonSaveStore` escreve e le `SaveData` sem perda de campos;
- load de arquivo inexistente retorna ausencia de save, nao erro.

## 10. Documentacao Pos-Implementacao

Atualizar:

- `docs/arquitetura/dados.md`: formato real de save, campos e fluxo Godot;
- `docs/arquitetura/aplicacao.md`: bootstrap com load opcional e save no quit/intervalo;
- `docs/arquitetura/party.md`: fila e automacao passam a ser persistidas;
- `docs/arquitetura/combate.md`: combate ativo passa a ser persistido;
- `docs/proximos-passos.md`: marcar save/load como implementado e registrar limitacoes restantes.

## 11. Verificacao

Rodar:

```powershell
dotnet build RymoraLandOfHeroes.sln
dotnet test RymoraLandOfHeroes.sln --no-restore
git diff --check
```

Smoke Godot headless deve rodar se executavel estiver disponivel. Se continuar ausente nesta maquina, registrar limitacao.

## 12. Fora Do Escopo

Nao implementar nesta fatia:

- editor visual/manual de saves;
- multiplos slots;
- cloud save;
- migracao entre varias versoes antigas;
- criptografia/compressao;
- save de configuracoes do jogador;
- Dungeon/EnterDungeon;
- crafting/lojas.
