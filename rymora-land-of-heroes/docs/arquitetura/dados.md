# Dados - Rymora Land of Heroes

Data de criacao: 2026-05-23
Regras de negocio: `docs/regras/biblia-rpg.md`
Arquitetura geral: `docs/arquitetura/visao-geral.md`

---

## 1. Proposito

O dominio **Data** cobre save/load, configuracao do jogo e progressao persistente.

---

## 2. Save

### 2.1 Formato

Serializacao em JSON (ou Godot Resource format). 1 arquivo de save por jogador.

### 2.2 Conteudo do save

```csharp
public sealed record SaveData
{
    public string SaveVersion;
    public DateTime SavedAt;
    public float PlayTime;

    // Parties
    public List<PartySaveData> Parties;

    // Itens no chao (corpo de heroi morto)
    public List<CorpseSaveData> Corpses;

    // Progressao global
    public Dictionary<string, float> GlobalProperties; // ValiantPoints, etc
}
```

```csharp
public sealed record PartySaveData
{
    public string PartyId;
    public Vector2I Position;
    public bool IsAlive;
    public List<HeroSaveData> Members;
    public List<ActionSaveData> ActionQueue;
    public InventorySaveData Inventory;
}
```

### 2.3 Quando salvar

- Automatico: a cada N segundos (configuravel).
- Manual: botao de save na UI (se houver).
- Ao fechar o jogo.

---

## 3. Config

### 3.1 Game config (global, nao salva por jogador)

```csharp
public sealed record GameConfig
{
    public float EncounterProbability; // chance base de encontro
    public float EncounterInterval; // tempo entre checks de encontro
    public float CorpseDecayTime; // tempo ate corpo desaparecer
}
```

Carregado de um arquivo de recurso Godot (`.tres` ou `.json`).

### 3.2 Player settings (preferencias)

- Idioma (futuro).
- Volume.
- Resolucao (futuro).
