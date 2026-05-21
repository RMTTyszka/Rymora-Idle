# Documentacao do jogo - Rymora Idle

Este documento descreve como o projeto funciona hoje, com base no codigo, nos assets e na cena principal presentes no repositorio. O foco aqui e a implementacao atual, nao uma intencao futura de design.

## 1. Visao geral

Rymora Idle e um prototipo em Unity 6 (`6000.4.3f1`) centrado em:

- exploracao de mapa em tilemap hexagonal;
- selecao de um grupo de herois;
- fila de acoes simples no mapa;
- coleta de recursos por mineracao e corte de madeira;
- encontros aleatorios;
- combate automatico por cooldown;
- interface 2D em UGUI.

Toda a experiencia jogavel esta concentrada na cena [`Assets/Scenes/MainScreen.unity`](Assets/Scenes/MainScreen.unity).

## 2. Estrutura geral do projeto

Os blocos mais importantes sao:

- `Assets/Scripts/Global`: coordenacao geral do jogo, selecao de grupo e HUD principal.
- `Assets/Scripts/Creatures`: modelo de criatura, atributos, skills, propriedades, inventario, equipamentos e grupo de herois.
- `Assets/Scripts/Combats` e `Assets/GameObjects/Combat`: regras de combate e renderizacao dos participantes na tela de combate.
- `Assets/Scripts/Map` e `Assets/GameObjects/Maps`: tipos de terreno, regioes, clique no mapa, highlight e movimentacao.
- `Assets/Scripts/Items`: itens equipaveis e materias-primas.
- `Assets/GameObjects/UI`: menus, botoes e listas da interface.
- `Assets/Resources/Itens`: templates de armas e armaduras carregados em runtime.
- `Assets/Map`: tiles de terreno e tiles de regiao.
- `Assets/GameObjects/Creatures`: encontros e monstros configurados como assets.

Tambem existem pacotes/recursos de apoio importados:

- `TextMesh Pro`
- `ProgressBar`
- `DragCamera2D`

## 3. Cena principal e objetos de runtime

Na `MainScreen` existem alguns objetos centrais:

- `GameManager`: coordena inicializacao, cameras e escolha da tela atual.
- `GameData`: expoe configuracoes globais de encontro e carrega listas de itens.
- `PartyManager`: guarda a lista de grupos (`heroes`) e o grupo atualmente selecionado.
- `Map`: contem `MapManager` e `Pathfinder`, alem dos tilemaps de terreno, regiao, paredes e highlight.
- `CombatManager`: instancia corpos visuais para herois e monstros no modo de combate.
- `UIManager`: alterna paineis de status e combate.
- `CurrentHeroService` e `ControlManager`: fazem a troca do grupo selecionado via botao ou teclas `F1`, `F2`, `F3`.

### Estado atual da cena

- `Party1` esta ativo na cena.
- `Party2` e `Party3` existem e estao cadastrados no `PartyManager`, mas com `m_IsActive: 0` por padrao.
- A tela de combate possui 3 `HeroSpawner` e 4 `MonsterPawner`.
- O objeto `GameData` da cena esta configurado com:
  - `encounterProbability: 100`
  - `encounterInterval: 3`

Na pratica, isso significa que qualquer grupo vivo e viajando fora de cidade dispara um check de encontro a cada 3 segundos, com encontro garantido na configuracao atual da cena.

## 4. Fluxo de inicializacao

O bootstrap principal acontece assim:

1. `GameManager.Awake()` encontra `PartyManager` e `MapManager`.
2. `GameManager` define a tela inicial como `Map`.
3. A camera do mapa fica ativa e a camera de combate fica desativada.
4. O grupo atual passa a ser `PartyManager.heroes[0]`.
5. `GameManager` espera o `MapManager` terminar de varrer o tilemap de terreno.
6. `MapManager.Start()` percorre todas as celulas do tilemap:
   - registra todas as cidades encontradas;
   - guarda suas posicoes em `CityPositions`;
   - gera um `Guid` para cada `MapTerrain`.
7. Ao final dessa varredura, `MapManager` publica `OnCitiesPoulated`.
8. `GameManager.InitiateHero()`:
   - carrega armas e armaduras via `Resources.LoadAll`;
   - posiciona os 3 grupos nas 3 primeiras cidades encontradas;
   - equipa cada grupo com uma arma aleatoria e uma armadura aleatoria;
   - restaura vida dos membros.

## 5. Modelo de dados principal

### 5.1 Party

A classe `Heroes.Party` representa um grupo no mapa. Ela concentra:

- `Members`: lista de criaturas do grupo;
- `Hero`: atalho para o primeiro membro;
- `NextActions`: fila de acoes;
- `CurrentAction`: acao em execucao;
- `CombatInstance`: combate atual, quando existir;
- `LootBag`: inventario auxiliar;
- `WayPoints`: destinos enfileirados;
- `CurrentTerrain` e `CurrentRegion`: contexto do tile atual;
- temporizadores de encontro e de acao.

Cada `Party` e um `MonoBehaviour` na cena e executa seu loop em `Update()`.

Na pratica, o projeto atual parece operar com 1 membro por `Party`:

- o construtor de `Party` adiciona apenas uma `Creature` em `Members`;
- nao ha populacao adicional clara desses membros na cena principal;
- o atalho `Hero` sempre usa `Members.FirstOrDefault()`.

### 5.2 Creature

`Creature` e o modelo base de heroi ou monstro. Ela possui:

- `Name`
- `Level`
- `Life` / `MaxLife`
- `Inventory`
- `Equipment`
- `Skills`
- `Attributes`
- `Properties`
- `Combatant`
- `Sprite`

`MaxLife` hoje e calculado como:

- base fixa de `100`
- mais `Vitality * 10`

### 5.3 Atributos, skills e propriedades

O sistema numerico usa a abstracao `Roller`:

- `AttributeInstance` comeca em `5` pontos, divisor `5`;
- `SkillInstance` comeca em `5` pontos, divisor `5`;
- `PropertyInstance` comeca em `0` pontos, divisor `1`.

Cada roller pode receber bonus por tipo:

- `Innate`
- `Magic`
- `Equipment`
- `Food`
- `Furniture`

O valor efetivo e derivado de pontos base + bonus. O metodo `GetValue(level)` tambem pode disparar ganho de ponto automaticamente, dependendo do nivel do desafio recebido.

## 6. Loop principal de gameplay

### 6.1 Selecionar grupo

O grupo ativo pode ser trocado de duas formas:

- pelos botoes de selecao de heroi na UI;
- pelas teclas `F1`, `F2` e `F3`.

Quando um grupo e selecionado:

- `PartyManager.CurrentParty` muda;
- `CurrentHeroPanel` atualiza nome, nivel e fila de acoes;
- a camera do mapa e centralizada no grupo selecionado;
- se esse grupo estiver em combate, `CombatManager` instancia a representacao visual do combate.

### 6.2 Interagir com o mapa

Quando a tela atual e `Map`, `MapManager.Update()`:

- calcula o tile sob o mouse;
- destaca o tile no tilemap de highlight;
- no clique direito, abre o `ActionPanel` no ponto do mouse.

O menu contextual sempre mostra `Move` e habilita condicionalmente:

- `Mine` se o tile for `Mountain`;
- `CutWood` se o tile for `Forest`;
- `EnterDungeon` se o tile for `Place`.

Observacao importante: `City` herda de `Place`, entao cidades tambem entram na regra de `CanEnterDungeon()`.

### 6.3 Fila de acoes

As acoes sao representadas por `HeroAction`.

Campos principais:

- `Action`: etapa de preparacao/inicio;
- `ExecutionAction`: etapa executada quando o tempo fecha;
- `ActionEndType`: regra de parada;
- `TimeToExecute`
- `PassedTime`
- `ExecutedCount`
- `ActionType`

`Party.Update()` chama `TryAction()`, que:

1. ignora tudo se o grupo estiver em combate;
2. pega a proxima acao da fila, se `CurrentAction` estiver vazia;
3. inicia a acao;
4. acumula tempo;
5. ao bater `EndActionTime`, executa `ExecutionAction`.

### 6.4 Viagem

`Move` enfileira uma acao de `Travel`.

O deslocamento real acontece em `Party.TryMove()`:

- o grupo anda em linha reta com `Vector3.MoveTowards`;
- a velocidade depende de `MoveSpeed` do tile atual;
- se o proximo ponto sair do terreno valido, a fila e limpa.

Importante: o projeto tem `MapMover` e `Pathfinder` com busca em grade hexagonal, mas o loop normal de viagem nao usa esse pathfinding. O campo `MapMover.target` nunca e preenchido pelo fluxo principal. Hoje o caminho padrao e movimento direto ate o destino clicado.

### 6.5 Mineracao

Ao clicar em `Mine`:

1. se o grupo ainda nao estiver no tile clicado, a viagem e enfileirada primeiro;
2. e criada uma `HeroAction` com `LimitCount = 5`;
3. `InitiateMining()` define:
   - `Action = TryMine`
   - `ExecutionAction = Mine`
   - `TimeToExecute = Skills.MineTime` (`3`)

Em `TryMine()`:

- confirma se o terreno atual pode minerar;
- sorteia um metal com base na qualidade do tile;
- guarda esse material em `CurrentMaterial`;
- roda o skill roll de `Mining`;
- calcula `ActionPerformance`;
- reinicia os timers da acao.

Em `Mine()`:

- testa sucesso contra dificuldade `material.Level * 10 + 50`;
- se passar, adiciona o item ao inventario;
- incrementa tempo e contagem executada.

### 6.6 Corte de madeira

O fluxo de `CutWood` e equivalente ao de `Mine`, trocando:

- skill principal para `Lumberjacking`;
- tempo base para `Skills.CutWoodTime` (`2`);
- selecao de material a partir de `Forest.GetMaterial()`.

## 7. Recursos e inventario

### 7.1 Materiais coletaveis

Materiais definidos no codigo:

- madeira:
  - `Oak`
- metais:
  - `Iron`
  - `Bronze`
  - `Copper`
  - `Silver`
  - `Gold`
  - `Mythril`

Hoje, a floresta sempre retorna variantes de `Oak`, porque o array `Forest.Woods` so contem `Oak`.

### 7.2 Inventario

Cada criatura possui `Inventory`.

Comportamento atual:

- `AddItem()` adiciona a instancia do item;
- `GroupedItems` agrupa por `Name` e `Level`;
- `RemoveItem()` remove por nome e nivel;
- a UI mostra nome, nivel, quantidade e peso total.

O `ItemListComponent` atualiza a lista quando:

- o grupo selecionado muda;
- o inventario do grupo selecionado muda.

## 8. Terreno, mundo e dados de conteudo

### 8.1 Tipos de terreno

Classes de tile implementadas:

- `Plain`
- `Road`
- `Forest`
- `Mountain`
- `Mine`
- `Place`
- `City`
- `Wall`
- `Region`

Cada `MapTerrain` define:

- `isWalkable`
- `moveSpeed`
- `quality`
- `encounterRateModifier`

O metodo `Level()` devolve `quality + 1`.

### 8.2 Assets de terreno presentes

Os assets mais relevantes encontrados no projeto sao:

- `Road`: 1 asset
- `Plain`: 1 asset
- `Mountain`: quality 0 e 4
- `Forest`: quality 0 ate 9
- `City`: 1 asset (`10 - Capitol`)
- `Dungeon`: assets de 1 ate 10

### 8.3 Regioes

Regioes configuradas:

- `SafeSpot`
- `NefirPlains`
  - `regionName: Bloddy Plains`
- `NefirMontain`
  - serializado como `name: Montain of the Gray Troll`

As duas regioes jogaveis referenciam o mesmo encounter asset atual: `MontainEncounter`.

### 8.4 Encontros e monstros

Conteudo encontrado:

- encounters:
  - `MontainEncounter`
  - `PlaainEncounter`
- monstro:
  - `Goblin Warrior`

Nos assets atuais:

- `MontainEncounter` contem 1 `Goblin Warrior`;
- `PlaainEncounter` tambem contem 1 `Goblin Warrior`;
- porem as regioes `NefirPlains` e `NefirMontain` apontam para `MontainEncounter`, entao o encounter de planicie existe no projeto, mas nao esta ligado ao mapa no estado atual.

### 8.5 Itens equipaveis carregados por Resources

Armas em `Assets/Resources/Itens/Weapons`:

- `Spear`
- `None - Light`
- `None - Medium`
- `None - Heavy`

Armaduras em `Assets/Resources/Itens/Armors`:

- `Leather`

Uso atual:

- herois recebem arma e armadura aleatorias no spawn;
- monstros recebem arma "desarmada" aleatoria e armadura aleatoria ao serem criados a partir de `CreatureTemplate`.

## 9. Combate

### 9.1 Como o combate comeca

Durante `Travel`, cada grupo faz check de encontro quando:

- nao esta em combate;
- esta vivo;
- a acao atual e `Travel`;
- o terreno atual nao e `City`.

Quando o check passa:

1. o grupo entra em `InCombat`;
2. a regiao atual escolhe um `Encounter` aleatorio;
3. `CombatManager.StartCombat()` cria um `CombatInstance`;
4. herois e monstros recebem cooldown inicial com base nas armas equipadas;
5. se o grupo em combate for o grupo selecionado, o combate e instanciado visualmente.

### 9.2 Como o turno roda

`CombatManager.Update()` percorre todos os `Party` da cena, inclusive inativos, e executa `RunCombatTurn()` para cada grupo com combate ativo.

Dentro de `CombatInstance.RunCombatTurn()`:

- primeiro atacam os herois que podem atacar;
- depois atacam os monstros que podem atacar;
- em seguida o sistema verifica vitoria ou derrota.

### 9.3 Escolha de alvo

O alvo automatico e o inimigo com maior aggro:

- menos vida atual aumenta o aggro;
- a propriedade `Threat` tambem aumenta o aggro.

### 9.4 Regras principais de ataque

O fluxo base de ataque e:

1. calcular `hitRoll` com skill, atributo, propriedade `Hit` e modificador da arma;
2. comparar com `evadeRoll` do alvo;
3. se acertar:
   - rolar critico;
   - calcular dano;
   - aplicar dano no alvo;
   - publicar texto flutuante na UI;
4. se errar e nao for contra-ataque:
   - o alvo pode responder com contra-ataque.

O dano combina:

- dano base da arma por tamanho;
- multiplicadores do tipo de arma;
- atributo ofensivo;
- skill ofensiva;
- `Armslore`;
- protecao da armadura do alvo;
- penetracao de armadura.

Tipos de arma e mapeamento atual:

- `Cutting` -> `Swordmanship`, `Strength`
- `Piercing` -> `Fencing`, `Agility`
- `Ranged` -> `Archery`, `Agility`
- `Thrown` -> `Archery`, `Agility`/`Strength`
- `Catalyst` -> `Magery`, `Intuition`/`Wisdom`
- `None` -> `Wrestling`, `Vitality`
- `Smashing` -> `Heavyweaponship`, `Vitality`/`Strength`

### 9.5 Velocidade e cooldown

O combate e automatico por cooldown:

- cada arma tem `AttackSpeed`;
- o cooldown reduz por `Time.deltaTime`;
- a propriedade `AttackSpeed` acelera essa recarga.

### 9.6 Fim do combate

O combate termina quando:

- todos os monstros morrem -> vitoria dos herois;
- todos os membros do grupo morrem -> derrota.

Consequencias:

- vitoria:
  - limpa a instancia de combate;
  - nao existe tela de loot implementada ainda;
- derrota:
  - o grupo limpa a fila de acoes;
  - a acao atual e cancelada;
  - `Die()` pede um deslocamento para o `SafeSpot` mais proximo.

## 10. Interface atual

A UI principal oferece:

- selecao de grupo;
- painel do grupo atual com nome, nivel, fila de acoes e barra de progresso;
- inventario agrupado;
- peso total;
- botao de status;
- botao para alternar entre mapa e combate;
- botao de estrategia;
- menu contextual do mapa.

### Alternancia de telas

`UIManager.ToggleCombat()`:

- alterna entre camera do mapa e camera de combate;
- muda `GameManager.CurrentScreen`;
- mostra/oculta o container de detalhes do heroi.

### Painel de estrategia

Existe infraestrutura visual para estrategia:

- `Strategy`
- `StrategyContainer`
- `CombatActionMenu`
- `CombatActionController`

Porem, no estado atual:

- `UIManager.strategyContainer` esta `null` na cena;
- o botao de estrategia chama `ToggleStrategy()`;
- isso indica que a tela de estrategia existe na hierarquia, mas nao esta ligada corretamente ao `UIManager`;
- alem disso, as `Rows` de `CombatActionController` ainda nao sao usadas pelo combate automatico.

## 11. Sistemas presentes, mas ainda incompletos ou placeholders

Os seguintes pontos existem no codigo, mas ainda nao fecham o loop jogavel:

- `EnterDungeon()` apenas escreve `Debug.Log("Entering Dungeon")`.
- `DungeonManager` so cria um nivel a partir do `Place`, sem fluxo jogavel.
- `CombatActionController` e `CombatActionMenu` ainda nao influenciam a IA.
- `Creature.RunPowerAttack()` esta vazio.
- `CombatInstance.ProcessLootAndFinish()` encerra o combate, mas nao entrega loot.
- `CombatEvent` esta vazio.
- `Effect` e `EffectManager` existem, mas o combate atual quase nao usa efeitos.
- `RealmManagerService` e `ContentManager` publicam evento, mas esse fluxo nao esta integrado ao loop principal.
- `MapMover` + `Pathfinder` existem, mas o deslocamento normal do grupo continua sendo direto, sem navegacao por caminho.

## 12. Resumo do estado atual

Hoje o jogo funciona como um prototipo de idle/exploracao com:

- 1 cena principal;
- 1 grupo efetivamente ativo por padrao;
- 3 grupos cadastrados;
- 1 monstro configurado;
- 1 encounter efetivamente ligado ao mapa;
- 1 armadura e 4 armas base;
- coleta de madeira e metal;
- combate automatico com formulas basicas;
- HUD e inventario jogaveis.

O coracao do jogo ja existe: selecionar grupo, andar pelo mapa, acumular acoes, coletar recurso e entrar em combate automatico. O que ainda falta para fechar uma versao mais completa e principalmente:

- ativacao real de todos os grupos;
- uso correto do sistema de estrategia;
- dungeons funcionais;
- loot e progressao de combate;
- conteudo maior de mapas, monstros, itens e encontros;
- ligacao do pathfinding ao movimento padrao.

## 13. Como abrir e testar manualmente

### 13.1 Abrir o projeto

O projeto foi aberto/gerado com Unity `6000.4.3f1`. Para testar o jogo:

1. abrir a pasta raiz do projeto no Unity;
2. abrir manualmente a cena `Assets/Scenes/MainScreen.unity`;
3. entrar em Play Mode.

Observacao: `ProjectSettings/EditorBuildSettings.asset` nao mostra uma cena ativa configurada para build. Para testes no editor, use a `MainScreen` diretamente.

### 13.2 Controles conhecidos

- `F1`: seleciona `Party1`.
- `F2`: seleciona `Party2`.
- `F3`: seleciona `Party3`.
- Clique direito no mapa: abre o menu contextual de acoes do tile.
- Mouse sobre o mapa: atualiza o highlight do tile.
- Botao `Status`: alterna o painel de status.
- Botao `Combat`: alterna entre camera do mapa e camera de combate.
- Botao `Strategy`: chama `UIManager.ToggleStrategy()`, mas tende a falhar enquanto `strategyContainer` estiver sem referencia na cena.

### 13.3 Roteiro de teste rapido

Um roteiro minimo para validar o estado atual:

1. entrar em Play Mode na `MainScreen`;
2. confirmar que `Party1` aparece no mapa e que o HUD mostra o heroi atual;
3. clicar com botao direito em um tile de mapa;
4. usar `Move` em um tile caminhavel;
5. testar `Mine` em uma montanha;
6. testar `CutWood` em uma floresta;
7. observar o inventario atualizando quando a coleta passa no teste de skill;
8. durante viagem fora de cidade, aguardar o encontro automatico;
9. usar o botao `Combat` para ver a tela de combate;
10. aguardar o combate automatico finalizar.

Com a configuracao atual da cena, o encontro deve acontecer com facilidade porque `encounterProbability` esta em `100`.

## 14. Como adicionar conteudo

### 14.1 Adicionar arma

O jogo carrega armas por `Resources.LoadAll<WeaponTemplate>("Itens/Weapons")`.

Para adicionar uma arma:

1. criar um asset pelo menu `Itens/Weapon`;
2. salvar em `Assets/Resources/Itens/Weapons`;
3. definir `size`;
4. definir `damageCategory`.

Campos importantes:

- `WeaponSize`: `Light`, `Medium`, `Heavy`.
- `WeaponsDamageCategory`: `Smashing`, `Piercing`, `Cutting`, `Catalyst`, `None`, `Ranged`, `Thrown`.

### 14.2 Adicionar armadura

O jogo carrega armaduras por `Resources.LoadAll<ArmorTemplate>("Itens/Armors")`.

Para adicionar uma armadura:

1. criar um asset pelo menu `Itens/Armor`;
2. salvar em `Assets/Resources/Itens/Armors`;
3. definir `category`.

Categorias atuais:

- `Light`
- `Medium`
- `Heavy`

Os valores numericos de protecao e evasao ficam em `ArmorData.DataByCategory`.

### 14.3 Adicionar monstro

Monstros usam `CreatureTemplate`.

Para adicionar um monstro:

1. criar asset pelo menu `Monster`;
2. definir `creatureName`;
3. definir `monsterClass`;
4. atribuir um `sprite`;
5. adicionar esse template em algum asset de `Encounter`.

Quando o encontro inicia, `Creature.FromCreature()` transforma o template em uma `Creature` de runtime, equipa uma arma desarmada aleatoria e uma armadura aleatoria.

### 14.4 Adicionar encontro

Encontros usam o asset `Encounter`.

Para adicionar um encontro:

1. criar asset pelo menu `Encounter`;
2. preencher a lista `monsters`;
3. adicionar o encounter na lista `encounters` de um asset de `Region`.

Se a regiao nao referenciar o encounter, ele existe no projeto mas nunca e sorteado pelo fluxo atual.

### 14.5 Adicionar terreno ou regiao

Terrenos e regioes sao `Tile`/`TileBase` customizados.

Menus atuais:

- `Tiles/Terrain/Plain`
- `Tiles/Terrain/Road`
- `Tiles/Terrain/Forest`
- `Tiles/Terrain/Mountain`
- `Tiles/Terrain/Mine`
- `Tiles/Terrain/City`
- `Tiles/Terrain/Place`
- `Tiles/Regions/Region`

Para um tile participar corretamente do gameplay:

- o tile de terreno deve estar no tilemap `Common Terrain`;
- a regiao correspondente deve estar no tilemap `Regions Map`;
- tiles que bloqueiam passagem devem estar no tilemap `Walls`;
- para encontros, a regiao precisa ter ao menos um `Encounter`.

## 15. Relacoes entre sistemas

### 15.1 Eventos principais

`PartyManager` e o principal barramento de UI:

- `OnHeroSelected`: avisa que o grupo atual mudou.
- `OnActionUpdated`: avisa que a fila de acoes mudou.
- `OnInventoryUpdate`: avisa que o inventario do grupo atual mudou.

`MapManager` expoe:

- `OnCitiesPoulated`: avisa quando terminou de coletar cidades do mapa.

`RealmManagerService` expoe:

- `OnRealmSelected`: existe, mas nao esta integrado ao loop principal.

### 15.2 Dependencias de inicializacao

O fluxo atual depende de varias buscas por cena:

- `GameManager` procura `PartyManager` e `MapManager`;
- `MapManager` procura `PartyManager`;
- `Party` procura `MapManager`, `PartyManager`, `GameData` e `CombatManager`;
- `CombatManager` procura `PartyManager` e todos os `Party`;
- controles e paineis de UI recebem parte das referencias por serializacao.

Isso funciona no prototipo, mas deixa a cena sensivel a nomes, tags, objetos ausentes e ordem de inicializacao.

## 16. Formulas e parametros importantes

### 16.1 Vida

`Creature.MaxLife`:

```text
100 + Vitality.GetValue(0) * 10
```

### 16.2 Velocidade de movimento

`Party.Speed(moveSpeed)`:

```text
1 * (moveSpeed / 100)
```

Na pratica:

- `Road` e `Plain`: `100`, velocidade normal;
- `Forest`: `70`, mais lento;
- `Mountain`: `30`, bem mais lento.

### 16.3 Dificuldade de coleta

Mineracao e corte de madeira usam:

```text
material.Level * 10 + 50
```

O sucesso final usa `RollForChallenge(skill, difficult, material.Level)`:

```text
Random.Range(1, 101) + skill.GetValue(challengeLevel) > difficult
```

### 16.4 Chance de encontro

Durante viagem:

```text
Random.Range(1, 101) + CurrentTerrain.encounterRateModifier <= GameData.encounterProbability
```

Na cena atual, `encounterProbability` esta em `100`, entao o encontro e praticamente garantido quando o check acontece.

### 16.5 Ataque basico

Acerto:

```text
hitRoll >= evadeRoll
```

`hitRoll` soma:

- `Random.Range(1, 101)`;
- skill de acerto da arma;
- atributo de acerto da arma;
- propriedade `Hit`;
- modificador de acerto do tamanho da arma.

`evadeRoll` soma:

- `Random.Range(1, 101)`;
- `Agility`;
- `Tactics`;
- propriedade `Evasion`;
- evasao da armadura.

## 17. Riscos e problemas conhecidos

Estes pontos merecem atencao antes de expandir conteudo ou sistemas:

- `GameManager.InitiateHero()` assume que existem pelo menos 3 herois e 3 cidades. Se a cena ou o mapa nao tiverem isso, o fluxo pode quebrar.
- `Party2` e `Party3` estao inativos na cena, entao seus `Update()` nao rodam enquanto nao forem ativados.
- `UIManager.strategyContainer` esta sem referencia; clicar em `Strategy` pode causar `NullReferenceException`.
- `NefirPlains` e `NefirMontain` apontam para o mesmo `MontainEncounter`.
- `PlaainEncounter` existe, mas nao parece estar ligado a uma regiao usada.
- O movimento ignora o pathfinding e anda direto ate o ponto clicado.
- `MapMover.GoToClosetSafeSpot()` compara duas distancias calculadas com a mesma posicao, entao a escolha do safe spot mais proximo nao esta correta.
- `Mountain.GetMetalByLevel()` usa `Math.Min(metalIndex, Metals.Length)`, o que pode gerar indice fora do array quando `metalIndex == Metals.Length`.
- `Forest.GetMaterial()` e `Mountain.GetMaterial()` tem dois blocos `if (value < 30)`, tornando o segundo inalcancavel.
- `CbtTriggerType` define `CriticalDamage = 2` e `SpellName = 2`, ou seja, dois triggers compartilham o mesmo valor numerico.
- `Inventory.AddItem()` impede adicionar a mesma instancia duas vezes, mas permite instancias diferentes com mesmo nome/nivel; a UI agrupa depois por `Name` e `Level`.
- `RemoveItem()` pode chamar `RemoveAt(-1)` se for pedido para remover mais itens do que existem com aquele nome/nivel.
- Vitoria em combate nao entrega loot.
- Morte do grupo chama movimento para safe spot, mas esse fluxo depende de `MapMover` e pode nao mover como esperado no fluxo atual.

## 18. Melhorias recomendadas em ordem pratica

Uma sequencia pragmatica para evoluir o projeto:

1. Corrigir erros que podem gerar exception imediata: `strategyContainer`, indices de metal e remocao de inventario.
2. Decidir se `Party2` e `Party3` devem ser ativos e jogaveis ou apenas placeholders.
3. Ligar `Pathfinder` ao movimento normal ou remover `MapMover` do caminho de morte ate isso estar pronto.
4. Configurar `NefirPlains` com encounter proprio e revisar a probabilidade de encontro.
5. Implementar loot de combate e/ou recompensa de encounter.
6. Implementar entrada em dungeon como fluxo separado ou remover o botao ate existir gameplay.
7. Transformar estrategia de combate em comportamento real ou ocultar a UI temporariamente.
8. Criar testes pequenos para formulas de combate, inventario e selecao de material.
