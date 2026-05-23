using RymoraLandOfHeroes.Core.Combat;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;
using RymoraLandOfHeroes.Core.Content;
using RymoraLandOfHeroes.Core.Hero;
using RymoraLandOfHeroes.Core.Party;
using RymoraLandOfHeroes.Core.World;
using GameParty = RymoraLandOfHeroes.Core.Party.Party;

namespace RymoraLandOfHeroes.Core.Application;

public sealed class GameApplication
{
    private readonly Dictionary<string, CombatInstance> _combats = new();
    private readonly Func<CreatureTemplate, Creature> _monsterFactory;
    private readonly IRandomSource _randomSource;

    public GameApplication(
        WorldState world,
        IEnumerable<GameParty> parties,
        GameConfig config,
        Func<CreatureTemplate, Creature> monsterFactory,
        IRandomSource? randomSource = null)
    {
        World = world;
        Parties = new PartyRegistry(parties);
        Config = config;
        UI = new UIState();
        _monsterFactory = monsterFactory;
        _randomSource = randomSource ?? new SystemRandomSource();
    }

    public WorldState World { get; }
    public PartyRegistry Parties { get; }
    public UIState UI { get; }
    public GameConfig Config { get; }
    public IReadOnlyDictionary<string, CombatInstance> ActiveCombats => _combats;

    public void SelectParty(string partyId)
    {
        Parties.Select(partyId);
        UI.SelectedPartyId = partyId;
        SyncScreen();
    }

    public void HandleInput(PlayerIntent intent)
    {
        switch (intent)
        {
            case SelectPartyIntent selectParty:
                SelectParty(selectParty.PartyId);
                break;
            case EnqueueActionIntent enqueueAction:
                EnqueueAction(enqueueAction.PartyId, enqueueAction.Request);
                break;
            case ExecuteMapActionIntent:
                throw new NotSupportedException("ExecuteMapActionIntent needs item/action parameters; use EnqueueActionIntent for now.");
            default:
                throw new ArgumentOutOfRangeException(nameof(intent), "Unknown player intent.");
        }
    }

    public bool EnqueueAction(string partyId, PartyActionRequest request)
    {
        var party = Parties.Get(partyId);
        var prepared = PrepareAction(party, request);
        if (prepared is null)
        {
            return false;
        }

        party.ActionQueue.Enqueue(prepared);
        return true;
    }

    public void Update(float deltaTime)
    {
        if (deltaTime < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time cannot be negative.");
        }

        foreach (var party in Parties.All)
        {
            if (party.IsInCombat)
            {
                RunCombatTurn(party, deltaTime);
                continue;
            }

            if (party.IsDefeated)
            {
                HandlePartyDeath(party);
                continue;
            }

            RunPartyActions(party, deltaTime);
        }

        SyncScreen();
    }

    public CombatInstance StartCombat(string partyId, EncounterTemplate encounter)
    {
        var party = Parties.Get(partyId);
        var monsters = encounter.Monsters.Select(_monsterFactory).ToArray();
        var combat = new CombatInstance(
            party.Members.Where(member => member.IsAlive),
            monsters,
            Config.Combat,
            _randomSource);

        _combats[party.Id] = combat;
        party.IsInCombat = true;
        SyncScreen();
        return combat;
    }

    private PartyActionRequest? PrepareAction(GameParty party, PartyActionRequest request)
    {
        if (request.ActionType != PartyActionType.Travel)
        {
            return request;
        }

        if (request.Destination is null)
        {
            return null;
        }

        var path = World.FindPath(party.Position, request.Destination.Value);
        if (path.Count == 0 && party.Position != request.Destination.Value)
        {
            return null;
        }

        return request with
        {
            EndType = PartyActionEndType.ByCount,
            LimitCount = path.Count,
            Path = path
        };
    }

    private void RunPartyActions(GameParty party, float deltaTime)
    {
        var current = party.ActionQueue.StartNextIfIdle();
        if (current is null)
        {
            return;
        }

        if (current.IsComplete(GetCurrentItemQuantity(party, current.Request)))
        {
            party.ActionQueue.CompleteCurrentIfFinished(GetCurrentItemQuantity(party, current.Request));
            return;
        }

        if (!CanExecuteCurrentAction(party, current.Request))
        {
            return;
        }

        current.AddProgress(deltaTime);
        if (!current.IsReadyToExecute)
        {
            return;
        }

        switch (current.Request.ActionType)
        {
            case PartyActionType.Travel:
                ExecuteTravel(party, current);
                break;
            case PartyActionType.Mine:
                ExecuteCollect(party, current, requiresMining: true);
                break;
            case PartyActionType.CutWood:
                ExecuteCollect(party, current, requiresMining: false);
                break;
            case PartyActionType.TransferItem:
                ExecuteTransfer(party, current);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(current.Request.ActionType), "Unknown party action type.");
        }
    }

    private static bool CanExecuteCurrentAction(GameParty party, PartyActionRequest request)
    {
        return request.ActionType == PartyActionType.TransferItem || party.CanRunMapActions;
    }

    private void ExecuteTravel(GameParty party, PartyActionState state)
    {
        var path = state.Request.Path ?? Array.Empty<TilePosition>();
        if (state.ExecutedCount >= path.Count)
        {
            party.ActionQueue.CompleteCurrentIfFinished();
            return;
        }

        party.Position = path[state.ExecutedCount];
        state.MarkExecuted();
        party.ActionQueue.CompleteCurrentIfFinished();

        if (World.ShouldTriggerEncounter(party.Position, Config.EncounterProbability, Config.EncounterPolicy))
        {
            StartCombat(party.Id, World.SelectRandomEncounter(party.Position));
        }
    }

    private void ExecuteCollect(GameParty party, PartyActionState state, bool requiresMining)
    {
        var terrain = World.GetTerrain(party.Position);
        if ((requiresMining && !terrain.AllowsMining) || (!requiresMining && !terrain.AllowsWoodcutting))
        {
            party.ActionQueue.Clear();
            return;
        }

        var request = state.Request;
        if (request.ItemName is null || request.ItemLevel is null || request.ItemWeight is null)
        {
            party.ActionQueue.Clear();
            return;
        }

        var skill = requiresMining
            ? party.Leader!.Skills[SkillType.Mining].GetValue()
            : party.Leader!.Skills[SkillType.Lumberjacking].GetValue();
        var difficulty = Config.Collection.DifficultyBase
            + request.ItemLevel.Value * Config.Collection.DifficultyPerMaterialLevel;
        var roll = _randomSource.NextInclusive(1, 100) + skill;

        if (roll >= difficulty)
        {
            party.Inventory.AddItem(new Item(request.ItemName, request.ItemLevel.Value, request.ItemWeight.Value, 1));
        }

        state.MarkExecuted();
        party.ActionQueue.CompleteCurrentIfFinished(GetCurrentItemQuantity(party, request));
    }

    private void ExecuteTransfer(GameParty party, PartyActionState state)
    {
        var request = state.Request;
        if (request.TargetPartyId is null || request.ItemName is null || request.ItemLevel is null || request.Quantity is null)
        {
            party.ActionQueue.Clear();
            return;
        }

        var targetParty = Parties.Get(request.TargetPartyId);
        if (targetParty.Position != party.Position)
        {
            party.ActionQueue.Clear();
            return;
        }

        var item = party.Inventory.GetItem(request.ItemName, request.ItemLevel.Value);
        if (item is null || item.Quantity < request.Quantity.Value)
        {
            party.ActionQueue.Clear();
            return;
        }

        if (!party.Inventory.RemoveItem(item.Name, item.Level, request.Quantity.Value))
        {
            party.ActionQueue.Clear();
            return;
        }

        targetParty.Inventory.AddItem(new Item(item.Name, item.Level, item.Weight, request.Quantity.Value));
        state.MarkExecuted();
        party.ActionQueue.CompleteCurrentIfFinished();
    }

    private void RunCombatTurn(GameParty party, float deltaTime)
    {
        if (!_combats.TryGetValue(party.Id, out var combat))
        {
            party.IsInCombat = false;
            return;
        }

        combat.RunTurn(deltaTime);
        if (combat.State == CombatState.Ongoing)
        {
            return;
        }

        _combats.Remove(party.Id);
        party.IsInCombat = false;

        if (combat.State == CombatState.MonstersVictory)
        {
            HandlePartyDeath(party);
        }
    }

    private void HandlePartyDeath(GameParty party)
    {
        party.ActionQueue.Clear();
        party.Position = World.FindNearestSafeSpotPosition(party.Position);
        party.IsInCombat = false;
    }

    private int GetCurrentItemQuantity(GameParty party, PartyActionRequest request)
    {
        return request.ItemName is null || request.ItemLevel is null
            ? 0
            : party.Inventory.GetQuantity(request.ItemName, request.ItemLevel.Value);
    }

    private void SyncScreen()
    {
        UI.CurrentScreen = Parties.Selected?.IsInCombat == true ? Screen.Combat : Screen.Map;
    }
}
