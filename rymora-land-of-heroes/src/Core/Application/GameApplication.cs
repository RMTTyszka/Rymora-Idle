using RymoraLandOfHeroes.Core.Automation;
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
    private int _nextRecordingNumber;

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
            case StartRecordingMacroIntent startRecording:
                StartRecordingMacro(startRecording.PartyId);
                break;
            case SaveRecordingMacroIntent saveRecording:
                SaveRecordingMacro(saveRecording.PartyId, saveRecording.Name);
                break;
            case CancelRecordingMacroIntent cancelRecording:
                CancelRecordingMacro(cancelRecording.PartyId);
                break;
            case RecordMoveActionIntent recordMove:
                RecordMoveAction(recordMove.PartyId, recordMove.Target);
                break;
            case RecordGatherActionIntent recordGather:
                RecordGatherAction(recordGather.PartyId, recordGather.Target, recordGather.Kind, recordGather.ItemName, recordGather.ItemLevel, recordGather.ItemWeight);
                break;
            case PlayProgramIntent playProgram:
                PlayProgram(playProgram.PartyId);
                break;
            case PauseProgramIntent pauseProgram:
                PauseProgram(pauseProgram.PartyId);
                break;
            case StopProgramIntent stopProgram:
                StopProgram(stopProgram.PartyId);
                break;
            case AddMacroToProgramIntent addMacroToProgram:
                AddMacroToProgram(addMacroToProgram.PartyId, addMacroToProgram.MacroId, addMacroToProgram.Repeat);
                break;
            case MoveProgramStepIntent moveProgramStep:
                MoveProgramStep(moveProgramStep.PartyId, moveProgramStep.StepId, moveProgramStep.NewIndex);
                break;
            case RemoveProgramStepIntent removeProgramStep:
                RemoveProgramStep(removeProgramStep.PartyId, removeProgramStep.StepId);
                break;
            case RenameMacroIntent renameMacro:
                RenameMacro(renameMacro.PartyId, renameMacro.MacroId, renameMacro.Name);
                break;
            case RemoveMacroActionIntent removeMacroAction:
                RemoveMacroAction(removeMacroAction.PartyId, removeMacroAction.MacroId, removeMacroAction.ActionId);
                break;
            case MoveMacroActionIntent moveMacroAction:
                MoveMacroAction(moveMacroAction.PartyId, moveMacroAction.MacroId, moveMacroAction.ActionId, moveMacroAction.NewIndex);
                break;
            case SetMoveActionDestinationIntent setMoveActionDestination:
                SetMoveActionDestination(setMoveActionDestination.PartyId, setMoveActionDestination.MacroId, setMoveActionDestination.ActionId, setMoveActionDestination.Destination);
                break;
            case SetGatherActionRepeatIntent setGatherActionRepeat:
                SetGatherActionRepeat(setGatherActionRepeat.PartyId, setGatherActionRepeat.MacroId, setGatherActionRepeat.ActionId, setGatherActionRepeat.Repeat);
                break;
            case SetProgramRepeatIntent setProgramRepeat:
                SetProgramRepeat(setProgramRepeat.PartyId, setProgramRepeat.Repeat);
                break;
            case SetProgramStepRepeatIntent setProgramStepRepeat:
                SetProgramStepRepeat(setProgramStepRepeat.PartyId, setProgramStepRepeat.StepId, setProgramStepRepeat.Repeat);
                break;
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

    public void PlayProgram(string partyId)
    {
        var party = Parties.Get(partyId);
        party.Automation.Runner.Play();
    }

    public void PauseProgram(string partyId)
    {
        Parties.Get(partyId).Automation.Runner.Pause();
    }

    public void StopProgram(string partyId)
    {
        Parties.Get(partyId).Automation.Runner.Stop();
    }

    public void StartRecordingMacro(string partyId)
    {
        var party = Parties.Get(partyId);
        party.Automation.StartRecording(NextMacroId(party));
    }

    public void RecordMoveAction(string partyId, TilePosition target)
    {
        var recording = Parties.Get(partyId).Automation.Recording
            ?? throw new InvalidOperationException("No Macro recording session is active.");
        recording.RecordMove(target);
    }

    public void RecordGatherAction(string partyId, TilePosition target, MacroActionKind kind, string itemName, int itemLevel, float itemWeight)
    {
        var recording = Parties.Get(partyId).Automation.Recording
            ?? throw new InvalidOperationException("No Macro recording session is active.");
        recording.RecordGather(target, kind, itemName, itemLevel, itemWeight);
    }

    public PartyMacro SaveRecordingMacro(string partyId, string name)
    {
        return Parties.Get(partyId).Automation.SaveRecording(name);
    }

    public void CancelRecordingMacro(string partyId)
    {
        Parties.Get(partyId).Automation.CancelRecording();
    }

    public ProgramStep AddMacroToProgram(string partyId, string macroId, RepeatPolicy repeat)
    {
        var party = Parties.Get(partyId);
        party.Automation.GetMacro(macroId);
        return party.Automation.Program.AddStep(macroId, repeat);
    }

    public void MoveProgramStep(string partyId, string stepId, int newIndex)
    {
        Parties.Get(partyId).Automation.Program.MoveStep(stepId, newIndex);
    }

    public void RemoveProgramStep(string partyId, string stepId)
    {
        Parties.Get(partyId).Automation.Program.RemoveStep(stepId);
    }

    public void RenameMacro(string partyId, string macroId, string name)
    {
        Parties.Get(partyId).Automation.GetMacro(macroId).Rename(name);
    }

    public void RemoveMacroAction(string partyId, string macroId, string actionId)
    {
        Parties.Get(partyId).Automation.GetMacro(macroId).RemoveAction(actionId);
    }

    public void MoveMacroAction(string partyId, string macroId, string actionId, int newIndex)
    {
        Parties.Get(partyId).Automation.GetMacro(macroId).MoveAction(actionId, newIndex);
    }

    public void SetGatherActionRepeat(string partyId, string macroId, string actionId, RepeatPolicy repeat)
    {
        Parties.Get(partyId).Automation.GetMacro(macroId).SetGatherActionRepeat(actionId, repeat);
    }

    public void SetMoveActionDestination(string partyId, string macroId, string actionId, TilePosition destination)
    {
        Parties.Get(partyId).Automation.GetMacro(macroId).SetMoveActionDestination(actionId, destination);
    }

    public void SetProgramRepeat(string partyId, RepeatPolicy repeat)
    {
        Parties.Get(partyId).Automation.Program.SetProgramRepeat(repeat);
    }

    public void SetProgramStepRepeat(string partyId, string stepId, RepeatPolicy repeat)
    {
        Parties.Get(partyId).Automation.Program.SetStepRepeat(stepId, repeat);
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
                FailActiveAutomationForDefeatedParty(party);
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
        if (party.ActionQueue.IsIdle)
        {
            QueueNextAutomationAction(party);
        }

        var current = party.ActionQueue.StartNextIfIdle();
        if (current is null)
        {
            return;
        }

        if (current.IsComplete(GetCurrentItemQuantity(party, current.Request)))
        {
            var completed = party.ActionQueue.CompleteCurrentIfFinished(GetCurrentItemQuantity(party, current.Request));
            NotifyAutomationActionCompleted(party, completed);
            return;
        }

        if (!CanExecuteCurrentAction(party, current.Request))
        {
            if (current.Request.AutomationActionId is not null)
            {
                party.Automation.Runner.Fail("Party cannot run map actions.");
                party.ActionQueue.Clear();
            }

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

    private string NextMacroId(GameParty party)
    {
        do
        {
            _nextRecordingNumber++;
        }
        while (party.Automation.TryGetMacro($"macro-{_nextRecordingNumber}") is not null);

        return $"macro-{_nextRecordingNumber}";
    }

    private void QueueNextAutomationAction(GameParty party)
    {
        var execution = party.Automation.Runner.TryStartNextAction(party.Automation);
        if (execution is null)
        {
            return;
        }

        var request = CreateRequestForAutomationAction(party, execution);
        if (request is null)
        {
            return;
        }

        party.ActionQueue.Enqueue(request);
    }

    private PartyActionRequest? CreateRequestForAutomationAction(GameParty party, MacroActionExecution execution)
    {
        switch (execution.Action)
        {
            case MoveToMacroAction move:
                var travel = new PartyActionRequest(
                    PartyActionType.Travel,
                    PartyActionEndType.ByCount,
                    Config.Travel.ActionTime,
                    Destination: move.Destination,
                    AutomationActionId: execution.ExecutionId);
                var prepared = PrepareAction(party, travel);
                if (prepared is null)
                {
                    party.Automation.Runner.Fail($"No path to destination ({move.Destination.X}, {move.Destination.Y}).");
                }

                return prepared;

            case GatherMacroAction gather:
                return CreateGatherRequestForAutomationAction(party, gather, execution.ExecutionId);

            default:
                party.Automation.Runner.Fail($"Unsupported Macro action: {execution.Action.Kind}.");
                return null;
        }
    }

    private PartyActionRequest? CreateGatherRequestForAutomationAction(GameParty party, GatherMacroAction gather, string executionId)
    {
        var terrain = World.GetTerrain(party.Position);
        if (gather.Kind == MacroActionKind.Mine && !terrain.AllowsMining)
        {
            party.Automation.Runner.Fail($"Party cannot mine at ({party.Position.X}, {party.Position.Y}).");
            return null;
        }

        if (gather.Kind == MacroActionKind.CutWood && !terrain.AllowsWoodcutting)
        {
            party.Automation.Runner.Fail($"Party cannot cut wood at ({party.Position.X}, {party.Position.Y}).");
            return null;
        }

        return new PartyActionRequest(
            gather.Kind == MacroActionKind.Mine ? PartyActionType.Mine : PartyActionType.CutWood,
            PartyActionEndType.ByCount,
            gather.Kind == MacroActionKind.Mine ? Config.Collection.MiningActionTime : Config.Collection.WoodcuttingActionTime,
            LimitCount: 1,
            ItemName: gather.ItemName,
            ItemLevel: gather.ItemLevel,
            ItemWeight: gather.ItemWeight,
            AutomationActionId: executionId);
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
            var completed = party.ActionQueue.CompleteCurrentIfFinished();
            NotifyAutomationActionCompleted(party, completed);
            return;
        }

        party.Position = path[state.ExecutedCount];
        state.MarkExecuted();
        var travelCompleted = party.ActionQueue.CompleteCurrentIfFinished();
        NotifyAutomationActionCompleted(party, travelCompleted);

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
            FailAutomationIfCurrentActionIsAutomated(party, state, "Collection action failed its current tile requirements.");
            party.ActionQueue.Clear();
            return;
        }

        var request = state.Request;
        if (request.ItemName is null || request.ItemLevel is null || request.ItemWeight is null)
        {
            FailAutomationIfCurrentActionIsAutomated(party, state, "Collection action failed its current tile requirements.");
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
        var completed = party.ActionQueue.CompleteCurrentIfFinished(GetCurrentItemQuantity(party, request));
        NotifyAutomationActionCompleted(party, completed);
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
        var completed = party.ActionQueue.CompleteCurrentIfFinished();
        NotifyAutomationActionCompleted(party, completed);
    }

    private static void NotifyAutomationActionCompleted(GameParty party, PartyActionState? completed)
    {
        if (completed?.Request.AutomationActionId is null)
        {
            return;
        }

        party.Automation.Runner.CompleteAction(completed.Request.AutomationActionId, completed.PassedTime);
    }

    private static void FailAutomationIfCurrentActionIsAutomated(GameParty party, PartyActionState state, string message)
    {
        if (state.Request.AutomationActionId is not null)
        {
            party.Automation.Runner.Fail(message);
        }
    }

    private static void FailActiveAutomationForDefeatedParty(GameParty party)
    {
        if (party.Automation.Runner.State is ProgramRunnerState.Running
            or ProgramRunnerState.PauseRequested
            or ProgramRunnerState.Paused
            or ProgramRunnerState.StopRequested)
        {
            party.Automation.Runner.Fail("Party cannot run map actions.");
        }
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
