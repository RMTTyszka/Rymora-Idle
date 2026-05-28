using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Combat;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Hero;
using RymoraLandOfHeroes.Core.Party;
using GameParty = RymoraLandOfHeroes.Core.Party.Party;

namespace RymoraLandOfHeroes.Core.Data;

public static class SaveSnapshotBuilder
{
    public static SaveData Create(GameApplication application, DateTimeOffset savedAtUtc)
    {
        var save = new SaveData(
            SaveData.CurrentVersion,
            savedAtUtc,
            application.PlayTimeSeconds,
            application.UI.SelectedPartyId,
            application.UI.CurrentScreen.ToString(),
            application.Parties.All.Select(CreateParty).ToArray(),
            application.ActiveCombats.Select(pair => CreateCombat(pair.Key, pair.Value, application.Parties.Get(pair.Key))).ToArray());

        SaveValidation.Validate(save);
        return save;
    }

    private static PartySaveData CreateParty(GameParty party)
    {
        return new PartySaveData(
            party.Id,
            CreatePosition(party.Position),
            party.IsInCombat,
            party.Members.Select(CreateCreature).ToArray(),
            party.Inventory.Items.Select(CreateItem).ToArray(),
            CreateQueue(party.ActionQueue),
            CreateAutomation(party.Automation));
    }

    private static CreatureSaveData CreateCreature(Creature creature)
    {
        return new CreatureSaveData(
            creature.Name,
            creature.Life,
            creature.MaxLife,
            creature.Sprite.Id,
            CreateStats(creature.Attributes),
            CreateStats(creature.Skills),
            CreateStats(creature.Properties),
            new EquipmentSaveData(
                creature.Equipment.MainHand,
                creature.Equipment.Offhand,
                creature.Equipment.Chest));
    }

    private static IReadOnlyDictionary<string, StatSaveData> CreateStats<TStat>(StatBlock<TStat> stats)
        where TStat : struct, Enum
    {
        return stats.All.ToDictionary(
            pair => pair.Key.ToString(),
            pair => new StatSaveData(pair.Value.Points, pair.Value.ValueDivisor));
    }

    private static ItemSaveData CreateItem(Item item)
    {
        return new ItemSaveData(item.Name, item.Level, item.Weight, item.Quantity);
    }

    private static ActionQueueSaveData CreateQueue(PartyActionQueue queue)
    {
        return new ActionQueueSaveData(
            queue.Current is null ? null : CreateActionState(queue.Current),
            queue.PendingRequests.Select(CreateRequest).ToArray());
    }

    private static PartyActionStateSaveData CreateActionState(PartyActionState state)
    {
        return new PartyActionStateSaveData(
            CreateRequest(state.Request),
            state.CurrentTime,
            state.PassedTime,
            state.ExecutedCount,
            state.Started);
    }

    private static PartyActionRequestSaveData CreateRequest(PartyActionRequest request)
    {
        return new PartyActionRequestSaveData(
            request.ActionType.ToString(),
            request.EndType.ToString(),
            request.TimeToExecute,
            request.LimitCount,
            request.EndTime,
            request.ItemName,
            request.ItemLevel,
            request.ItemWeight,
            request.Quantity,
            request.TargetPartyId,
            request.Destination is null ? null : CreatePosition(request.Destination.Value),
            request.Path?.Select(CreatePosition).ToArray() ?? Array.Empty<TilePositionSaveData>(),
            request.AutomationActionId);
    }

    private static AutomationSaveData CreateAutomation(PartyAutomation automation)
    {
        return new AutomationSaveData(
            automation.Recording is null ? null : CreateRecording(automation.Recording),
            automation.Macros.Select(CreateMacro).ToArray(),
            CreateProgram(automation.Program),
            CreateRunner(automation.Runner.CaptureState()));
    }

    private static MacroRecordingSaveData CreateRecording(MacroRecordingSession recording)
    {
        return new MacroRecordingSaveData(
            recording.Id,
            recording.NextActionNumber,
            recording.Actions.Select(CreateMacroAction).ToArray());
    }

    private static PartyMacroSaveData CreateMacro(PartyMacro macro)
    {
        return new PartyMacroSaveData(
            macro.Id,
            macro.Name,
            macro.Actions.Select(CreateMacroAction).ToArray());
    }

    private static MacroActionSaveData CreateMacroAction(MacroAction action)
    {
        return action switch
        {
            MoveToMacroAction move => new MacroActionSaveData(
                action.Id,
                action.Kind.ToString(),
                CreatePosition(move.Destination),
                ItemName: null,
                ItemLevel: null,
                ItemWeight: null,
                Repeat: null),
            GatherMacroAction gather => new MacroActionSaveData(
                action.Id,
                action.Kind.ToString(),
                Destination: null,
                gather.ItemName,
                gather.ItemLevel,
                gather.ItemWeight,
                CreateRepeat(gather.Repeat)),
            _ => throw new ArgumentOutOfRangeException(nameof(action), "Unknown Macro action type.")
        };
    }

    private static PartyProgramSaveData CreateProgram(PartyProgram program)
    {
        return new PartyProgramSaveData(
            program.Repeat.Mode.ToString(),
            program.Repeat.RepeatCount,
            program.Repeat.Seconds,
            program.NextStepNumber,
            program.Steps.Select(CreateProgramStep).ToArray());
    }

    private static ProgramStepSaveData CreateProgramStep(ProgramStep step)
    {
        return new ProgramStepSaveData(step.Id, step.MacroId, CreateRepeat(step.Repeat));
    }

    private static RepeatPolicySaveData CreateRepeat(RepeatPolicy repeat)
    {
        return new RepeatPolicySaveData(repeat.Mode.ToString(), repeat.RepeatCount, repeat.Seconds);
    }

    private static ProgramRunnerSaveData CreateRunner(ProgramRunnerRuntimeState state)
    {
        return new ProgramRunnerSaveData(
            state.State.ToString(),
            state.ErrorMessage,
            state.CurrentMacroActions.Select(CreateMacroAction).ToArray(),
            state.CurrentAction is null ? null : CreateMacroAction(state.CurrentAction),
            state.ProgramStepIndex,
            state.MacroActionIndex,
            state.StepIteration,
            state.ProgramIteration,
            state.ActionIteration,
            state.ProgramElapsedSeconds,
            state.StepElapsedSeconds,
            state.ActionElapsedSeconds,
            state.CurrentExecutionId);
    }

    private static CombatSaveData CreateCombat(string partyId, CombatInstance combat, GameParty party)
    {
        return new CombatSaveData(
            partyId,
            combat.State.ToString(),
            combat.Heroes.Select(combatant => CreateHeroCombatant(combatant, party)).ToArray(),
            combat.Monsters.Select(CreateMonsterCombatant).ToArray());
    }

    private static HeroCombatantSaveData CreateHeroCombatant(Combatant combatant, GameParty party)
    {
        var memberIndex = FindMemberIndex(party, combatant.Creature);
        return new HeroCombatantSaveData(memberIndex, CreateCooldowns(combatant).ToArray());
    }

    private static MonsterCombatantSaveData CreateMonsterCombatant(Combatant combatant)
    {
        return new MonsterCombatantSaveData(CreateCreature(combatant.Creature), CreateCooldowns(combatant).ToArray());
    }

    private static IEnumerable<WeaponCooldownSaveData> CreateCooldowns(Combatant combatant)
    {
        yield return CreateCooldown("MainHand", combatant.MainHandCooldown);
        if (combatant.OffhandCooldown is not null)
        {
            yield return CreateCooldown("Offhand", combatant.OffhandCooldown);
        }
    }

    private static WeaponCooldownSaveData CreateCooldown(string slot, WeaponCooldown cooldown)
    {
        return new WeaponCooldownSaveData(slot, cooldown.Weapon, cooldown.CurrentCooldown, cooldown.TotalCooldown);
    }

    private static int FindMemberIndex(GameParty party, Creature creature)
    {
        for (var index = 0; index < party.Members.Count; index++)
        {
            if (ReferenceEquals(party.Members[index], creature))
            {
                return index;
            }
        }

        throw new InvalidOperationException($"Combat hero is not a member of party {party.Id}.");
    }

    private static TilePositionSaveData CreatePosition(TilePosition position)
    {
        return new TilePositionSaveData(position.X, position.Y);
    }
}
