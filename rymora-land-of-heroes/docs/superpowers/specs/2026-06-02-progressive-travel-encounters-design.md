# Design - Progressive Travel Encounters

Data: 2026-06-02
Status: draft

## 1. Objetivo

Ajustar encontros de viagem para a v1 do prototipo com estas regras:

- chance base baixa e configuravel;
- modificadores de terreno, regiao e zona continuam afetando a chance;
- chance cresce com o tempo continuo de viagem da party;
- acumulacao reseta apos um encontro;
- acumulacao reseta ao entrar em safe spot ou cidade.

O fluxo continua tile-based no Core. A verificacao de encontro continua acontecendo quando a party avanca para o proximo tile logico.

## 2. Fora de escopo

- mudar combate;
- mudar UI de combate;
- adicionar dungeons;
- adicionar tela/config de balanceamento in-game;
- reabrir `EncounterInterval` como sistema separado nesta mudanca.

## 3. Comportamento proposto

Cada party passa a manter um estado de risco acumulado de viagem.

- enquanto a party executa `Travel` fora de safe spot/cidade, o tempo de viagem acumulado aumenta;
- ao entrar em um novo tile valido de viagem, o jogo calcula a chance efetiva de encontro;
- a chance efetiva soma:
  - chance base global;
  - modificador do terreno, conforme politica atual;
  - modificador da regiao;
  - modificador da zona;
  - bonus temporal acumulado;
- o bonus temporal cresce linearmente por segundo de viagem;
- o bonus temporal possui teto configuravel;
- se a chance efetiva ficar menor que zero, ela e truncada para zero;
- se a chance efetiva passar do teto maximo permitido, ela e truncada para cem.

Formula v1:

```text
temporalBonus = min(travelSecondsSinceReset * encounterRampPerSecond, encounterRampMaxBonus)
effectiveChance = clamp(baseEncounterProbability + terrainModifierAppliedByPolicy + regionModifier + zoneModifier + temporalBonus, 0, 100)
```

## 4. Reset da acumulacao

O acumulador de risco deve resetar nestes casos:

1. quando um encontro inicia;
2. quando a party entra em tile de safe spot;
3. quando a party entra em tile de cidade.

O acumulador nao reseta:

- ao terminar uma acao de travel sem encontro;
- ao mudar de terreno;
- ao mudar de regiao;
- ao mudar de zona.

Isso preserva a ideia de perigo crescente ao ficar muito tempo viajando fora de areas seguras.

## 5. Estrutura tecnica

### 5.1 Core

Adicionar estado de risco de encontro por party no Core.

Proposta minima:

```csharp
public sealed class PartyEncounterState
{
    public float TravelSecondsSinceReset { get; private set; }

    public void AddTravelTime(float deltaTime);
    public void Reset();
}
```

`Party` passa a possuir esse estado como parte do runtime persistente.

### 5.2 Config

Para manter mudanca pequena, este slice nao refatora a estrutura do config para um bloco novo.

`GameConfig` continua com os campos atuais e ganha mais dois valores flat:

```csharp
public sealed record GameConfig(
    float EncounterProbability,
    float EncounterInterval,
    EncounterPolicy EncounterPolicy,
    float EncounterRampPerSecond,
    float EncounterRampMaxBonus,
    ...);
```

Regras deste slice:

- `EncounterProbability` continua sendo a chance base global;
- `EncounterRampPerSecond` define quanto a chance sobe por segundo de viagem acumulada;
- `EncounterRampMaxBonus` define o teto do bonus temporal;
- `EncounterInterval` permanece no modelo atual, sem ganhar papel novo nesta mudanca.

### 5.3 Application

`GameApplication` continua fazendo o check em `ExecuteTravel`.

Fluxo:

1. party avanca um tile;
2. se tile atual e safe spot/cidade, resetar acumulacao e nao iniciar encontro;
3. caso contrario, calcular chance efetiva com bonus temporal;
4. rolar encontro;
5. se encontro iniciou, resetar acumulacao;
6. se nao iniciou, manter acumulacao para proximos tiles;
7. em cada update enquanto `Travel` esta em andamento e party esta viajando fora de combate, somar tempo ao acumulador.

Requisito importante: acumulacao de tempo deve refletir tempo real de viagem, nao quantidade de tiles. Se `Travel.ActionTime` mudar, o risco por segundo continua coerente.

### 5.4 World

`WorldState` continua responsavel por combinar modificadores do tile atual e avaliar se area pode gerar encontro.

Decisao deste slice:

- `WorldState.ShouldTriggerEncounter(...)` continua existindo;
- o metodo passa a receber a chance base ja acrescida do bonus temporal acumulado;
- `WorldState` continua aplicando modificador de terreno pela `EncounterPolicy`, somando regiao e zona, bloqueando safe spot/cidade e fazendo o roll final.

## 6. Persistencia

O save deve incluir estado de acumulacao de encontro por party.

Mudancas:

- `PartySaveData` ganha um campo numerico `TravelSecondsSinceEncounterReset`;
- `SaveData.CurrentVersion` deve ser incrementado, porque o formato persistido mudou.

Salvar:

- `TravelSecondsSinceEncounterReset` por party.

Restaurar:

- ao carregar save, a party retoma o mesmo risco acumulado que tinha antes.

Validacao:

- nao permitir valor negativo.

## 7. Balanceamento inicial

Diretriz de balanceamento da v1:

- chance base deve ser baixa;
- modificadores existentes continuam relevantes;
- rampa temporal deve aumentar o risco de forma perceptivel, mas nao imediata;
- teto deve impedir escalada explosiva.

Este slice nao fixa numeros finais na documentacao. Os valores ficam em `assets/data/game_config.json` e podem ser ajustados depois.

## 8. Testes

Adicionar ou ajustar testes para cobrir:

1. chance efetiva cresce com tempo acumulado de viagem;
2. bonus temporal respeita teto;
3. modificador de terreno continua aplicado conforme `EncounterPolicy`;
4. modificadores de regiao e zona continuam somando;
5. safe spot bloqueia encontro e reseta acumulacao;
6. cidade bloqueia encontro e reseta acumulacao;
7. encontro iniciado reseta acumulacao;
8. estado de acumulacao entra no save/load;
9. validacao de save falha para acumulacao negativa.

## 9. Impacto em docs existentes

Atualizar depois da implementacao:

- `docs/proximos-passos.md`;
- `docs/arquitetura/dados.md`;
- `docs/arquitetura/mundo.md`;
- `docs/arquitetura/aplicacao.md`.

Tambem registrar que a prioridade antiga sobre `EncounterInterval` ficou adiada enquanto a v1 usa check por tile com risco crescente por tempo.

## 10. Decisoes fechadas neste design

- check de encontro continua por tile;
- chance base deve ser baixa;
- terreno, regiao e zona continuam afetando a chance;
- acumulacao cresce com tempo, nao por tile;
- curva da v1 e linear com teto;
- reset acontece apos encontro;
- reset acontece ao entrar em safe spot/cidade.
