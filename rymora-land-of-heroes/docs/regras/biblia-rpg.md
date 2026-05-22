# Biblia de Regras do RPG

## Proposito

Este documento organiza as regras de negocio do RPG de Rymora Idle.

Ele registra somente informacoes confirmadas no prototipo antigo ou decisoes pendentes do criador. Nenhuma regra nova deve ser considerada aprovada por estar ausente, implicita ou parecida com outro RPG.

Este documento nao descreve organizacao interna do projeto, ferramentas, telas ou tecnologia. Esses assuntos devem ficar em outro documento.

## Estado das Informacoes

Cada regra deve ser lida com um destes estados:

- Confirmado: existe no prototipo antigo e deve continuar existindo no RPG novo.
- Decisao pendente: precisa de decisao do criador antes de virar regra final.
- Inconsistencia do prototipo: existe no prototipo antigo, mas precisa de correcao, renomeacao ou validacao.

## Fantasia Central

Confirmado:

- A fantasia principal do jogo e fantasia medieval.
- O jogador representa os proprios aventureiros.
- Os herois sao aventureiros independentes.
- O tom do mundo e medieval classico.
- O jogo possui herois.
- O jogo possui monstros.
- O jogo possui encontros de combate.
- O jogo possui viagem pelo mapa.
- O jogo possui mineracao.
- O jogo possui corte de madeira.
- O jogo possui inventario, equipamentos e materiais.
- O jogo possui terrenos e regioes.

## Estrutura Geral de Jogo

Confirmado:

- O jogador controla grupos chamados parties.
- O jogador seleciona uma party por vez.
- Ao selecionar uma party, o jogador pode controla-la e programar suas acoes.
- O jogador pode trocar a party selecionada e prosseguir controlando outra party.
- Cada party possui um heroi principal.
- O jogo tera somente tres herois ate o final.
- Um heroi pode entrar na party de outro heroi.
- Se uma party fica sem heroi, ela fica parada.
- Uma party sem heroi so pode ser usada para consultar seu inventario e ajustar sua programacao de acoes.
- Um heroi so pode sair de uma party quando estiver no mesmo local de outra party para a qual ele ira.
- Quando um heroi entra na party de outro heroi, ele leva consigo somente seu equipamento.
- Itens de inventario nao acompanham automaticamente um heroi que entra em outra party.
- Itens de inventario precisam ser transferidos manualmente de uma party para outra.
- A transferencia de itens entre parties exige que as duas parties estejam no mesmo local.
- Uma party sem heroi pode transferir itens se outra party estiver no mesmo local.
- As acoes no mapa sao programadas por party.
- As acoes de combate sao programadas por heroi.
- A transferencia manual de itens entre parties tambem pode ser programada como acao.
- O inventario pertence a party.
- A party pode viajar.
- A party pode executar acoes em fila.
- As acoes da party podem ser programadas para executar uma vez, repetir para sempre, repetir por uma quantidade definida de vezes ou repetir por tempo definido.
- Por enquanto, uma acao em loop para automaticamente quando nao consegue continuar por falha ou falta de requisito.
- A parte idle do jogo consiste em programar acoes e deixar o jogo executa-las.
- A party pode entrar em combate durante viagem.
- A party pode coletar recursos do terreno.
- Uma party morta perde suas acoes atuais e deve voltar para um ponto seguro.

## Herois

Confirmado:

- Um heroi possui nome.
- Um heroi possui nivel.
- Um heroi possui vida atual e vida maxima.
- Um heroi possui atributos.
- Um heroi possui pericias.
- Um heroi possui propriedades de combate.
- Um heroi possui inventario.
- Um heroi possui equipamentos.
- Um heroi pode estar vivo ou morto.
- Um heroi pode realizar ataques basicos.
- Um heroi pode usar arma principal.
- Um heroi pode usar arma secundaria quando houver uma arma equipada na mao secundaria.
- No comeco do jogo, o jogador cria os tres herois.
- Ao criar os herois, o jogador distribui pontos em atributos e pericias.
- Herois possuem raca.
- Herois nao possuem vocacao, profissao ou arquetipo.
- Herois nao possuem origem, personalidade ou alinhamento como regra de personagem.
- O nivel do heroi nao sobe diretamente.
- O nivel do heroi e calculado com base no total de pontos em atributos e pericias.
- Quando um heroi morre, ele vira fantasma.
- Um heroi fantasma precisa ir ate uma igreja para ressuscitar na cruz.
- Quando um heroi morre, ele retorna ao ponto seguro.
- Quando um heroi morre, o corpo fica disponivel por um tempo para recuperar os itens.

Inconsistencia do prototipo:

- O prototipo antigo possui nivel de heroi, mas nao confirma que o nivel seja calculado pelo total de pontos em atributos e pericias.

Decisao pendente:

- Definir quais racas existem.
- Definir quantos pontos o jogador distribui ao criar cada heroi.
- Definir a formula que calcula o nivel do heroi a partir dos pontos em atributos e pericias.
- Definir por quanto tempo o corpo fica disponivel para recuperar itens.
- Definir o que acontece com os itens que nao forem recuperados a tempo.

## Atributos

Confirmado:

- Strength.
- Agility.
- Vitality.
- Wisdom.
- Intuition.
- Charisma.

Confirmado:

- Vitality aumenta a vida maxima.
- Strength participa de armas de corte e dano de armas arremessadas.
- Agility participa de armas perfurantes, ranged e evasao.
- Wisdom participa do dano de catalyst.
- Intuition participa do acerto de catalyst.
- Charisma existe como atributo, mas seu uso nao esta confirmado.

Decisao pendente:

- Definir os nomes finais em portugues, ingles ou ambos.
- Definir se os nomes devem ser traduzidos para Forca, Agilidade, Vitalidade, Sabedoria, Intuicao e Carisma.
- Definir se Charisma tera uso social, comercial, lideranca, recrutamento, magia ou outro papel.
- Definir os valores iniciais dos atributos.
- Definir limites minimos e maximos dos atributos.

## Pericias

Confirmado:

- Alchemy.
- Anatomy.
- AnimalTaming.
- Archery.
- Armorcrafting.
- Armslore.
- Awareness.
- Bowcrafting.
- Carpentery.
- Fencing.
- Gathering.
- Healing.
- Heavyweaponship.
- Jewelcrafting.
- Leatherworking.
- Lore.
- Lumberjacking.
- Magery.
- Mercantilism.
- Military.
- Mining.
- Parry.
- Reflex.
- ResistSpells.
- Skinning.
- Stealth.
- Swordmanship.
- SpiritSpeaking.
- Tactics.
- Tailoring.
- Wrestling.

Confirmado:

- Mining e usada para minerar.
- Lumberjacking e usada para cortar madeira.
- Swordmanship e usada por armas de corte.
- Fencing e usada por armas perfurantes.
- Archery e usada por armas ranged e thrown.
- Magery e usada por catalyst.
- Wrestling e usada por armas sem categoria de dano.
- Heavyweaponship e usada por armas de esmagamento.
- Tactics contribui para evasao.
- Parry contribui para fortitude.
- Armslore contribui para dano de arma.

Inconsistencia do prototipo:

- Carpentery provavelmente precisa de validacao de grafia.
- Swordmanship provavelmente precisa de validacao de grafia.
- Heavyweaponship provavelmente precisa de validacao de grafia.

Decisao pendente:

- Definir nomes finais das pericias.
- Definir quais pericias serao de combate, coleta, fabricacao, magia, sobrevivencia, social ou conhecimento.
- Definir se todas as pericias ja entram na primeira versao do jogo.
- Definir se pericias aumentam por uso, por treinamento, por nivel, por itens ou por outro meio.

## Propriedades

Confirmado:

- Critical.
- Resiliense.
- Hit.
- PowerAttack.
- Evasion.
- PowerDefense.
- SpiritPoints.
- Life.
- ValiantPoints.
- Protection.
- Fortitude.
- AttackDamage.
- PowerDamage.
- AttackSpeed.
- CastingSpeed.
- CriticalDamage.
- ArmorPenetration.
- Reaction.
- Counter.
- Threat.

Confirmado:

- Critical aumenta a chance de acerto critico.
- Resiliense reduz a chance de receber critico.
- Hit aumenta a chance de acertar.
- Evasion aumenta a chance de evitar ataques.
- Protection reduz dano recebido.
- Fortitude reduz dano recebido por meio de resistencia fisica.
- AttackDamage aumenta dano de ataques.
- AttackSpeed acelera ataques.
- CriticalDamage aumenta dano critico.
- ArmorPenetration reduz a protecao efetiva do alvo.
- Counter aumenta chance de contra-ataque.
- Threat influencia a chance de ser escolhido como alvo.

Inconsistencia do prototipo:

- Resiliense provavelmente precisa de validacao de grafia.

Decisao pendente:

- Definir o significado final de PowerAttack.
- Definir o significado final de PowerDefense.
- Definir o significado final de SpiritPoints.
- Definir o significado final de ValiantPoints.
- Definir o significado final de CastingSpeed.
- Definir o significado final de Reaction.
- Definir se Life como propriedade aumenta vida maxima, vida atual, regeneracao ou outro efeito.

## Bonus

Confirmado:

- Existem bonus dos seguintes tipos:
  - Innate.
  - Magic.
  - Equipment.
  - Food.
  - Furniture.

Confirmado:

- Bonus Innate se somam entre si.
- Bonus Magic considera apenas o maior bonus desse tipo.
- Bonus Equipment considera apenas o maior bonus desse tipo.
- Bonus Food considera apenas o maior bonus desse tipo.
- Bonus Furniture considera apenas o maior bonus desse tipo.
- Bonus possuem valor.
- Bonus podem ter inicio e fim.

Decisao pendente:

- Definir se esta regra de acumulacao sera mantida exatamente.
- Definir se bonus negativos seguem a mesma regra.
- Definir se bonus temporarios expiram em tempo real, tempo de jogo, fim de combate ou outro criterio.

## Progressao por Uso

Confirmado:

- Atributos, pericias e propriedades possuem pontos.
- Esses pontos geram um valor efetivo.
- Atributos e pericias comecam com pontos iniciais.
- Propriedades comecam com valor inicial zero.
- Existe uma chance de aumento quando uma rolagem e feita contra um desafio relevante.
- A chance de aumento depende dos pontos atuais e da dificuldade.

Confirmado:

- Atributos usam dificuldade maior que pericias no prototipo.
- Pericias usam dificuldade menor que atributos no prototipo.
- O valor efetivo e baseado em pontos divididos por um modificador.

Decisao pendente:

- Definir se a progressao por uso sera mantida.
- Definir se atributos tambem aumentam por uso ou apenas pericias.
- Definir se propriedades podem evoluir naturalmente ou somente por bonus/equipamentos.
- Definir os valores finais de dificuldade e progressao.
- Definir se havera limite de progressao.

## Vida

Confirmado:

- Todo heroi possui vida atual.
- Todo heroi possui vida maxima.
- A vida maxima usa uma base fixa.
- Vitality aumenta a vida maxima.
- No prototipo, cada ponto efetivo de Vitality adiciona vida maxima.
- Um personagem esta vivo quando sua vida atual e maior que zero.
- Ao receber dano que levaria a vida abaixo de zero, a vida fica em zero.

Decisao pendente:

- Definir o valor final de vida base.
- Definir quanto Vitality aumenta a vida maxima.
- Definir se Life tambem altera vida maxima.
- Definir se existe regeneracao natural.
- Definir se existe cura fora de combate.

## Inventario

Confirmado:

- Um inventario possui itens.
- Itens podem ser agrupados por nome e nivel.
- Itens possuem nome.
- Itens possuem nivel.
- Itens possuem peso.
- Itens podem ser adicionados.
- Itens podem ser removidos por quantidade.
- O inventario pertence a party.
- Quando um heroi entra em outra party, ele nao leva itens do inventario da party original automaticamente.
- Quando um heroi entra em outra party, ele leva somente seu equipamento.
- Itens precisam ser levados manualmente de uma party para outra.
- Levar itens manualmente de uma party para outra pode ser uma acao programada.
- A transferencia de itens entre parties exige que as duas parties estejam no mesmo local.
- Uma party sem heroi pode transferir itens se outra party estiver no mesmo local.

Decisao pendente:

- Definir limite de peso.
- Definir limite de espacos.
- Definir se itens iguais empilham automaticamente.
- Definir se peso afeta movimento, combate ou coleta.

## Equipamentos

Confirmado:

- Existem itens equipaveis.
- Todo item equipavel ocupa um slot.
- Slots confirmados:
  - None.
  - MainHand.
  - Offhand.
  - Head.
  - Neck.
  - Chest.
  - Wrist.
  - Hand.
  - FingerLeft.
  - FingerRight.
  - Waist.
  - Feet.
  - Extra.

Confirmado:

- MainHand recebe arma principal.
- Offhand pode receber equipamento secundario.
- Chest recebe armadura.
- Equipar um item substitui o item anterior daquele slot.

Decisao pendente:

- Definir se os slots Wrist, Hand, FingerLeft, FingerRight, Waist, Feet, Neck, Head e Extra terao itens na primeira versao.
- Definir se Offhand aceita escudo, arma, foco magico ou qualquer item secundario.
- Definir se existem requisitos de atributo, nivel ou pericia para equipar.

## Armas

Confirmado:

- Armas possuem tamanho.
- Tamanhos confirmados:
  - Light.
  - Medium.
  - Heavy.

Confirmado:

- Armas possuem categoria de dano.
- Categorias confirmadas:
  - Smashing.
  - Piercing.
  - Cutting.
  - Catalyst.
  - None.
  - Ranged.
  - Thrown.

Confirmado:

- Armas Light possuem maior potencial de contra-ataque no prototipo.
- Armas Medium possuem ataque mais lento que Light e mais rapido que Heavy.
- Armas Heavy possuem maior penetracao de armadura e maior multiplicador de dano.
- Todas as armas por tamanho usam uma faixa base de dano no prototipo.

Confirmado:

- Cutting usa Swordmanship para acerto, Strength para acerto e Strength para dano.
- Piercing usa Fencing para acerto, Agility para acerto e Agility para dano.
- Ranged usa Archery para acerto, Agility para acerto e Agility para dano.
- Thrown usa Archery para acerto, Agility para acerto e Strength para dano.
- Catalyst usa Magery para acerto, Intuition para acerto e Wisdom para dano.
- None usa Wrestling para acerto, Vitality para acerto e Vitality para dano.
- Smashing usa Heavyweaponship para acerto, Vitality para acerto e Strength para dano.

Decisao pendente:

- Definir nomes finais das categorias.
- Definir se Catalyst representa cajados, varinhas, focos magicos ou outro item.
- Definir se None significa desarmado, improvisado ou criatura natural.
- Definir valores finais de dano, velocidade, acerto, penetracao e contra-ataque.

## Armaduras

Confirmado:

- Armaduras possuem categoria.
- Categorias confirmadas:
  - Light.
  - Medium.
  - Heavy.

Confirmado:

- Armadura Light tem menor protecao e maior evasao.
- Armadura Medium tem protecao e evasao intermediarias.
- Armadura Heavy tem maior protecao e menor evasao.

Decisao pendente:

- Definir valores finais das armaduras.
- Definir se armaduras influenciam velocidade de viagem.
- Definir se armaduras influenciam magia, furtividade, coleta ou outras acoes.

## Qualidade

Confirmado:

- Existem graus de qualidade.
- Graus confirmados:
  - Poor.
  - Average.
  - Common.
  - Fine.
  - Good.
  - Superior.
  - Exceptional.
  - Excellent.
  - Wonderful.
  - Master.
  - GrandMaster.
  - Godlike.

Confirmado:

- Terrenos possuem qualidade.
- A qualidade do terreno define um nivel derivado.
- No prototipo, o nivel do terreno e a posicao da qualidade na escala mais um.

Decisao pendente:

- Definir se a qualidade se aplica a itens, materiais, monstros, regioes, construcoes ou apenas terrenos.
- Definir os nomes finais dos graus de qualidade.
- Definir se Godlike deve existir no jogo final.

## Materiais

Confirmado:

- Existem materiais brutos.
- Materiais brutos sao itens.
- Materiais brutos possuem nome, nivel e peso.

Confirmado:

- Metais confirmados:
  - Iron: nivel 1, peso 3.
  - Bronze: nivel 2, peso 4.
  - Copper: nivel 3, peso 2.
  - Silver: nivel 4, peso 2.
  - Gold: nivel 5, peso 2.
  - Mythril: nivel 6, peso 3.

Confirmado:

- Madeira confirmada:
  - Oak: nivel 1, peso 4.

Inconsistencia do prototipo:

- Wood aparece agrupado junto dos metais no prototipo antigo.

Decisao pendente:

- Definir a lista final de madeiras.
- Definir se Bronze deve ser minerado diretamente ou produzido por fabricacao.
- Definir se Copper deve vir antes de Bronze na progressao.
- Definir se peso e nivel dos materiais serao mantidos.
- Definir usos de cada material em fabricacao.

## Terrenos

Confirmado:

- Terrenos possuem caminhabilidade.
- Terrenos possuem velocidade de movimento.
- Terrenos possuem qualidade.
- Terrenos possuem modificador de encontro.
- Terrenos possuem nivel derivado da qualidade.

Confirmado:

- Tipos de velocidade de movimento:
  - Forest: 70.
  - Plain: 100.
  - Mountain: 30.
  - Desert: 50.
  - Jungle: 60.
  - Road: 100.
  - Swamp: 40.
  - Snow: 50.
  - Water: 20.
  - Ice: 60.
  - Hills: 60.

Confirmado:

- Forest permite cortar madeira.
- Mountain permite minerar.
- Place permite entrar em dungeon.
- City e um tipo de Place.
- Road usa velocidade de Road.
- Plain usa velocidade de Plain.
- Mine usa velocidade de Mountain.
- Wall nao e caminhavel.

Decisao pendente:

- Definir todos os tipos finais de terreno.
- Definir se Desert, Jungle, Swamp, Snow, Water, Ice e Hills existirao no mapa inicial.
- Definir se Wall representa parede, borda do mapa, montanha intransponivel ou outro bloqueio.
- Definir se Mine sera diferente de Mountain.
- Definir se cidades sempre sao seguras.

## Regioes

Confirmado:

- Regioes possuem nome.
- Regioes podem possuir encontros.
- Regioes podem ser ponto seguro.

Decisao pendente:

- Definir o que uma regiao representa no mundo.
- Definir se regioes possuem nivel proprio ou usam qualidade dos terrenos.
- Definir se regioes possuem faccoes, biomas, eventos, cidades ou recursos especiais.
- Definir se ponto seguro impede encontros, permite cura, permite retorno ou outra funcao.

## Viagem

Confirmado:

- Uma party pode receber pontos de destino.
- Esses pontos formam uma fila de viagem.
- A party se move ate o proximo ponto.
- Ao chegar ao ponto, esse destino e removido da fila.
- Se a party tentar caminhar por terreno invalido, a viagem e cancelada.
- A velocidade de movimento depende do terreno atual.

Confirmado:

- Durante viagem, a party pode verificar se ocorre encontro.
- Encontros nao acontecem quando a party ja esta em combate.
- Encontros nao acontecem quando a party esta morta.
- Encontros acontecem durante acao de viagem.
- Encontros nao acontecem em City.
- A chance de encontro usa probabilidade base e modificador do terreno.

Decisao pendente:

- Definir a frequencia final de verificacao de encontros.
- Definir a chance base final de encontro.
- Definir se modificador positivo aumenta ou reduz encontros.
- Definir se clima, hora, regiao ou nivel da party afetam encontros.

## Acoes de Heroi

Confirmado:

- Tipos de acao:
  - Travel.
  - Mine.
  - CutWood.
  - Transferir itens entre parties.

Confirmado:

- Uma acao pode terminar por contagem.
- Uma acao pode terminar por quantidade de item.
- Uma acao pode terminar por tempo.
- Acoes podem ter tempo de execucao.
- Acoes podem registrar tempo passado.
- Acoes podem registrar quantidade executada.
- Acoes podem ser colocadas em fila.
- Uma party executa uma acao por vez.
- Acoes no mapa sao programadas por party.
- Acoes de combate sao programadas por heroi.
- Transferir itens entre parties pode ser programado como acao.
- Uma acao pode ser configurada para executar uma vez.
- Uma acao pode ser configurada para repetir para sempre.
- Uma acao pode ser configurada para repetir por uma quantidade definida de vezes.
- Uma acao pode ser configurada para repetir por tempo definido.
- Por enquanto, uma acao em loop para automaticamente quando nao consegue continuar por falha ou falta de requisito.

Confirmado:

- Mineracao usa Mining.
- Corte de madeira usa Lumberjacking.
- No prototipo, mineracao leva mais tempo que corte de madeira.

Decisao pendente:

- Definir se acoes podem ser canceladas manualmente.
- Definir se acoes continuam enquanto o jogo esta fechado.
- Definir se falha em coleta consome tempo integral.
- Definir se sucesso em coleta sempre gera um item.

## Coleta

Confirmado:

- Mountain fornece metais.
- Forest fornece madeira.
- A dificuldade de coleta depende do nivel do material.
- A chance de sucesso usa uma rolagem da pericia relevante contra uma dificuldade.
- Mining coleta metais.
- Lumberjacking coleta madeira.
- Ao obter um item, o item entra no inventario do heroi.

Confirmado:

- O material encontrado depende do nivel do terreno e de uma variacao aleatoria.
- Terrenos de maior qualidade tendem a acessar materiais de maior nivel.

Inconsistencia do prototipo:

- A selecao de material possui faixas duplicadas e deve ser validada antes de virar regra final.
- A lista de madeiras possui apenas Oak repetido.

Decisao pendente:

- Definir as chances finais de materiais.
- Definir a progressao final de materiais por terreno.
- Definir se uma coleta malsucedida pode gerar experiencia mesmo sem item.
- Definir se coleta desgasta ferramentas.

## Encontros

Confirmado:

- Um encontro possui monstros.
- Uma regiao pode conter encontros.
- Quando um encontro comeca, os monstros sao criados com base em modelos de criatura.
- O nivel do encontro usa o nivel do terreno no prototipo.

Decisao pendente:

- Definir se encontros sao aleatorios, fixos, por tabela ponderada ou por eventos.
- Definir se encontros escalam por terreno, regiao, party, distancia da cidade ou outro criterio.
- Definir se encontros podem conter recompensas alem de loot dos monstros.

## Monstros

Confirmado:

- Um monstro possui nome.
- Um monstro possui tipo de monstro.
- Um monstro possui imagem.
- Tipos de monstro confirmados:
  - Combatant.
  - Caster.
  - Agile.

Decisao pendente:

- Definir o significado de cada tipo de monstro.
- Definir se monstros usam os mesmos atributos, pericias e propriedades dos herois.
- Definir se monstros usam equipamentos reais ou valores abstratos.
- Definir se monstros podem deixar loot por tipo, por regiao ou por material.

## Combate

Confirmado:

- Combate ocorre entre party e encontro.
- Heros atacam monstros.
- Monstros atacam herois.
- Personagens vivos podem atacar.
- O combate roda ate todos os monstros morrerem ou todos os herois morrerem.
- Quando todos os monstros morrem, os herois vencem.
- Quando todos os herois morrem, os herois perdem.

Confirmado:

- O ataque basico usa tempo de recarga da arma principal.
- Se houver arma secundaria, ela possui sua propria recarga.
- AttackSpeed acelera a recuperacao dos ataques.
- PowerAttack existe como conceito, mas sua regra ainda nao esta definida.

Decisao pendente:

- Definir se combate e automatico, por ordens, por prioridades ou por habilidades programadas.
- Definir se existe posicionamento em combate.
- Definir se party pode fugir.
- Definir se derrota encerra a viagem ou gera penalidade adicional.

## Escolha de Alvo

Confirmado:

- Ataques automaticos escolhem alvo entre inimigos.
- A escolha prioriza o alvo com maior aggro.
- Aggro aumenta com Threat.
- Aggro aumenta quando o alvo tem menos vida.

Inconsistencia do prototipo:

- A formula de aggro usa percentual de vida de uma forma que precisa ser validada, pois a intencao declarada e que menos vida gere mais aggro.

Decisao pendente:

- Definir se alvos com pouca vida devem ser mais ou menos visados.
- Definir se tanque deve atrair inimigos por Threat.
- Definir se cura, dano causado ou habilidades aumentam Threat.

## Acerto e Evasao

Confirmado:

- Ataque faz uma rolagem de acerto.
- O alvo faz uma rolagem de evasao.
- O ataque acerta quando a rolagem de acerto e maior ou igual a evasao.

Confirmado:

- Acerto considera:
  - rolagem base;
  - pericia da categoria da arma;
  - atributo de acerto da categoria da arma;
  - Hit;
  - modificador de acerto do tamanho da arma.

Confirmado:

- Evasao considera:
  - rolagem base;
  - Agility;
  - Tactics;
  - Evasion;
  - evasao da armadura do peito.

Decisao pendente:

- Definir valores finais das rolagens.
- Definir se existe acerto minimo e erro minimo.
- Definir se diferenca entre acerto e evasao afeta critico ou dano.

## Dano

Confirmado:

- Dano considera dano base da arma.
- Dano considera multiplicador do tamanho da arma.
- Dano considera pericia ligada a arma.
- Dano considera Armslore.
- Dano considera AttackDamage.
- Dano considera atributo de dano da categoria da arma.
- Dano e reduzido pela fortitude do alvo.
- Dano e reduzido pela protecao do alvo depois de considerar penetracao de armadura.
- Dano minimo nao fica negativo.
- Critico multiplica o dano.

Confirmado:

- Fortitude considera:
  - Parry;
  - Vitality;
  - Fortitude.

Confirmado:

- Protection considera:
  - Protection;
  - protecao da armadura do peito.

Confirmado:

- ArmorPenetration considera:
  - penetracao da arma;
  - ArmorPenetration.

Decisao pendente:

- Definir formula final de dano.
- Definir se dano deve ser inteiro ou pode ser decimal.
- Definir se dano zero ainda conta como acerto.
- Definir se existe tipo de dano alem da categoria da arma.

## Critico

Confirmado:

- Existe chance de critico.
- Critical aumenta chance de critico.
- Resiliense reduz chance de receber critico.
- Critico aumenta dano.
- CriticalDamage aumenta o dano critico.

Confirmado:

- Existe modificador de critico pela relacao entre tamanho da arma e categoria da armadura.
- Light contra Light recebe bonus de critico no prototipo.
- Heavy contra Heavy recebe bonus de critico no prototipo.
- Medium recebe bonus menor de critico no prototipo.
- Outras combinacoes recebem penalidade no prototipo.

Decisao pendente:

- Definir se essas relacoes arma-armadura serao mantidas.
- Definir chance base final de critico.
- Definir multiplicador final de dano critico.

## Contra-Ataque

Confirmado:

- Quando um ataque erra, pode haver contra-ataque.
- Counter aumenta a chance de contra-ataque.
- A arma possui potencial de contra-ataque.
- Existe modificador de contra-ataque pela relacao entre tamanho da arma e categoria da armadura.

Confirmado:

- Light contra Heavy recebe bonus de contra-ataque no prototipo.
- Heavy contra Light recebe bonus de contra-ataque no prototipo.

Decisao pendente:

- Definir se contra-ataque usa a arma do defensor ou a arma do atacante.
- Definir se contra-ataque pode gerar outro contra-ataque.
- Definir chance base final de contra-ataque.
- Definir se escudos influenciam contra-ataque.

## Poderes e Magia

Confirmado:

- Existem conceitos relacionados a magia e poderes:
  - Magery.
  - ResistSpells.
  - SpiritSpeaking.
  - SpiritPoints.
  - PowerAttack.
  - PowerDefense.
  - PowerDamage.
  - CastingSpeed.
  - Catalyst.

Confirmado:

- PowerAttack aparece como uma acao possivel, mas sem regra definida.
- CastingSpeed existe como propriedade.
- Catalyst usa Magery, Intuition e Wisdom.

Decisao pendente:

- Definir o sistema de magia.
- Definir o sistema de poderes.
- Definir se SpiritPoints e recurso de magia, recurso espiritual ou outro.
- Definir se ValiantPoints e recurso heroico, moral, coragem ou outro.
- Definir se todo heroi pode usar magia.

## Efeitos e Condicoes

Confirmado:

- Efeitos podem ser beneficos ou prejudiciais.
- Efeitos podem ser acumulaveis.
- Efeitos possuem duracao maxima.
- Efeitos possuem contagem regressiva.
- Efeitos podem afetar personagem.
- Efeitos podem afetar arma.

Confirmado:

- Condicoes confirmadas:
  - Dead.
  - Stunned.
  - Poisoned.
  - Frozen.
  - Scared.

Inconsistencia do prototipo:

- Poison aparece como Poison em uma lista e Poisoned em outra.

Decisao pendente:

- Definir lista final de condicoes.
- Definir regras de acumulacao.
- Definir se Dead e uma condicao ou um estado separado.
- Definir se efeitos persistem apos combate.

## Dungeons

Confirmado:

- Place pode permitir entrada em dungeon.
- Uma dungeon possui encontros.
- Uma dungeon possui nivel.
- O nivel da dungeon pode vir do nivel do Place.

Decisao pendente:

- Definir como dungeons funcionam.
- Definir se dungeons sao locais fixos, eventos temporarios ou instancias.
- Definir se dungeons possuem andares, chefe, recompensas, tempo ou risco de morte.
- Definir se entrar em dungeon interrompe viagem no mapa.

## Cidades e Pontos Seguros

Confirmado:

- City e um tipo de Place.
- City pode possuir lojas.
- Encontros de viagem nao acontecem em City.
- Regioes podem ser safe spot.
- Party morta tenta voltar para o ponto seguro mais proximo.

Decisao pendente:

- Definir o que existe em uma cidade.
- Definir se lojas vendem itens, compram materiais, oferecem treinamento ou contratam herois.
- Definir como pontos seguros sao encontrados.
- Definir se ponto seguro cura, revive ou apenas protege.

## Fabricacao e Economia

Confirmado:

- Existem pericias de fabricacao:
  - Armorcrafting.
  - Bowcrafting.
  - Carpentery.
  - Jewelcrafting.
  - Leatherworking.
  - Tailoring.
  - Alchemy.

Confirmado:

- Existem materiais que podem servir para fabricacao.
- Mercantilism existe como pericia.

Decisao pendente:

- Definir se fabricacao entra na primeira versao.
- Definir receitas.
- Definir se qualidade do material afeta qualidade do item.
- Definir se Mercantilism afeta preco, negociacao, lojas ou producao.

## Interface de Jogo Enquanto Regra de Produto

Confirmado:

- O jogador precisa ver o heroi atual.
- O jogador precisa ver nome e nivel do heroi atual.
- O jogador precisa ver a fila de acoes.
- O jogador precisa ver progresso da acao atual.
- Em combate, o jogador precisa ver vida.
- Em combate, o jogador precisa ver progresso de ataque.
- Em combate, eventos de dano, cura e critico podem ser destacados.

Decisao pendente:

- Definir quais informacoes devem aparecer sempre.
- Definir quais informacoes aparecem apenas ao selecionar um heroi.
- Definir se o jogador pode configurar prioridades de combate.

## Inconsistencias Conhecidas do Prototipo

Estas informacoes existem no prototipo antigo, mas nao devem virar regra final sem decisao explicita:

- Resiliense provavelmente precisa de correcao de grafia.
- Carpentery provavelmente precisa de correcao de grafia.
- Swordmanship provavelmente precisa de correcao de grafia.
- Heavyweaponship provavelmente precisa de correcao de grafia.
- CbtTriggerType usa o mesmo valor para CriticalDamage e SpellName.
- Wood aparece agrupado junto dos metais.
- A lista de madeiras possui apenas Oak repetido.
- A selecao de materiais em Forest e Mountain possui faixas de chance duplicadas.
- A formula de aggro precisa ser validada contra a intencao de design.
- Bronze aparece como material bruto mineravel, mas pode precisar virar resultado de fabricacao.

## Decisoes Pendentes Prioritarias

Antes do documento sobre a organizacao interna do projeto, as decisoes mais importantes sao:

1. Idioma final dos nomes de atributos, pericias, propriedades, itens e categorias.
2. Significado de PowerAttack, PowerDefense, SpiritPoints, ValiantPoints, Reaction e Life.
3. Regra final de progressao por uso.
4. Formula final de combate.
5. Regras finais de morte e retorno a ponto seguro.
6. Estrutura final de inventario: por heroi, por party ou compartilhado.
7. Lista final de materiais, especialmente madeiras.
8. Significado final de cidades, regioes, pontos seguros e dungeons.
9. Se fabricacao entra na primeira versao jogavel.
10. Se o jogo sera idle, semi-idle ou comandado com interacoes frequentes.

## Regra de Governanca deste Documento

Nenhuma regra marcada como decisao pendente deve ser tratada como se estivesse decidida.

Nenhuma regra nova deve ser adicionada a este documento sem decisao explicita do criador.

Quando uma decisao for tomada, ela deve sair de "Decisao pendente" e entrar como "Confirmado".
