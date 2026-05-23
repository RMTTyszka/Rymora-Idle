# Aplicacao - Rymora Land of Heroes

Data de criacao: 2026-05-23
Regras de negocio: `docs/regras/biblia-rpg.md`
Arquitetura geral: `docs/arquitetura/visao-geral.md`

---

## 1. Proposito

O dominio **Application** coordena bootstrap, casos de uso e comunicacao entre dominios. E a camada que orquestra acoes sem possuir regras de negocio proprias.

Application nao possui:
- estado de jogo duravel (delega para os dominios)
- referencias Godot
- UI

---

## 2. Bootstrap

O fluxo de inicializacao:

1. Godot carrega cena principal (`Bootstrap.tscn`).
2. `Bootstrap` (adaptador Godot) executa `Application.Initialize()`.
3. Content loaders carregam catalogos (armas, armaduras, monstros, encontros).
4. World adapter le o tilemap e popula `WorldState`.
5. Party registry cria as parties com herois iniciais.
6. Estado inicial: `CurrentScreen = Map`, `SelectedParty = Party1`.
7. Presenters sincronizam com estado inicial.

---

## 3. Casos de uso

Casos de uso sao metodos em `Application` que coordenam multiplos dominios.

### 3.1 SelectParty

```
1. Seleciona party no registro.
2. Atualiza UIState.SelectedParty.
3. Se party em combate -> Combat presenter instancia corpos.
4. Camera presenter centraliza na party.
```

### 3.2 EnqueueAction

```
1. Recebe intencao (tipo de acao, tile alvo, parametros).
2. Se Move: calcula caminho via World, enfileira waypoints.
3. Se Mine/CutWood: enfileira acao na Party.
4. Atualiza UI (fila de acoes).
```

### 3.3 StartCombat

```
1. World seleciona encontro aleatorio para a regiao.
2. Cria CombatInstance com herois da Party + monstros.
3. Marca Party.IsInCombat = true.
4. Se Party for selecionada -> UI alterna para tela de combate.
```

### 3.4 RunCombatTurn

```
1. Para cada Party em combate, chama CombatInstance.RunTurn(deltaTime).
2. Publica eventos de combate (dano, critico, morte).
3. Se combate terminou -> chama EndCombat.
```

### 3.5 EndCombat

```
1. Se vitoria: limpa instancia, Party volta ao mapa.
2. Se derrota: Party morre, inicia retorno ao safe spot.
3. Atualiza UI.
```

### 3.6 HandlePartyDeath

```
1. Limpa fila de acoes da Party.
2. World.FindNearestSafeSpot(Position).
3. Enfileira Travel ate o safe spot.
4. Marca herois como fantasma.
```

---

## 4. Game loop

O Application e atualizado pelo Godot a cada frame:

```
Application.Update(float deltaTime):
    para cada Party:
        se Party.IsInCombat:
            RunCombatTurn(party, deltaTime)
        senao se Party.IsAlive:
            RunPartyActions(party, deltaTime)
        senao:
            RunDeathReturn(party)
```

---

## 5. Estrutura

```csharp
public sealed class Application
{
    public WorldState World { get; }
    public PartyRegistry Parties { get; }
    public UIState UI { get; }

    public void Initialize(GameConfig config, IWeaponCatalog weapons, ...);
    public void SelectParty(string partyId);
    public void EnqueueAction(string partyId, ActionRequest request);
    public void Update(float deltaTime);
    public void HandleInput(PlayerIntent intent);
}
```
