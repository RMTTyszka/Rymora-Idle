using RymoraLandOfHeroes.Core.Application;
using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Combat;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Configuration;
using RymoraLandOfHeroes.Core.Content;
using RymoraLandOfHeroes.Core.Hero;
using RymoraLandOfHeroes.Core.Party;
using RymoraLandOfHeroes.Core.World;
using GameParty = RymoraLandOfHeroes.Core.Party.Party;

namespace RymoraLandOfHeroes.Core.Data;

public static class SaveRestorer
{
    public static GameApplication Restore(
        SaveData save,
        WorldState world,
        GameConfig config,
        Func<CreatureTemplate, Creature> monsterFactory,
        IRandomSource? randomSource = null)
    {
        SaveValidation.Validate(save);

        var parties = save.Parties.Select(RestoreParty).ToArray();
        var application = new GameApplication(
            world,
            parties,
            config,
            monsterFactory,
            randomSource,
            save.PlayTimeSeconds);

        if (save.SelectedPartyId is not null)
        {
            application.SelectParty(save.SelectedPartyId);
        }

        foreach (var combat in save.ActiveCombats)
        {
            application.RestoreActiveCombat(combat.PartyId, RestoreCombat(combat, application.Parties.Get(combat.PartyId), config, randomSource));
        }

        application.UI.CurrentScreen = ParseEnum<Screen>(save.CurrentScreen);

        return application;
    }

    private static GameParty RestoreParty(PartySaveData save)
    {
        var inventory = new Inventory();
        foreach (var item in save.InventoryItems)
        {
            inventory.AddItem(new Item(item.Name, item.Level, item.Weight, item.Quantity));
        }

        var party = new GameParty(save.PartyId, RestorePosition(save.Position), inventory)
        {
            IsInCombat = save.IsInCombat
        };

        foreach (var member in save.Members)
        {
            party.AddMember(RestoreCreature(member));
        }

        party.ActionQueue.Restore(
            save.ActionQueue.Current is null ? null : RestoreActionState(save.ActionQueue.Current),
            save.ActionQueue.Pending.Select(RestoreRequest));

        party.Automation.Restore(
            save.Automation.Macros.Select(RestoreMacro),
            save.Automation.Recording is null ? null : RestoreRecording(save.Automation.Recording),
            RestoreRepeat(save.Automation.Program.RepeatMode, save.Automation.Program.RepeatCount, save.Automation.Program.RepeatSeconds),
            save.Automation.Program.Steps.Select(RestoreProgramStep),
            save.Automation.Program.NextStepNumber,
            RestoreRunner(save.Automation.Runner));

        return party;
    }

    private static Creature RestoreCreature(CreatureSaveData save)
    {
        return Creature.Restore(
            save.Name,
            RestoreStats<AttributeType>(save.Attributes),
            RestoreStats<SkillType>(save.Skills),
            RestoreStats<PropertyType>(save.Properties),
            new Equipment
            {
                MainHand = save.Equipment.MainHand,
                Offhand = save.Equipment.Offhand,
                Chest = save.Equipment.Chest
            },
            new SpriteReference(save.SpriteId),
            save.Life,
            save.MaxLife);
    }

    private static StatBlock<TStat> RestoreStats<TStat>(IReadOnlyDictionary<string, StatSaveData> save)
        where TStat : struct, Enum
    {
        var stats = save.ToDictionary(
            pair => ParseEnum<TStat>(pair.Key),
            pair => new StatInstance(pair.Value.Points, pair.Value.ValueDivisor));
        return StatBlock<TStat>.FromInstances(stats);
    }

    private static PartyActionState RestoreActionState(PartyActionStateSaveData save)
    {
        return PartyActionState.Restore(
            RestoreRequest(save.Request),
            save.CurrentTime,
            save.PassedTime,
            save.ExecutedCount,
            save.Started);
    }

    private static PartyActionRequest RestoreRequest(PartyActionRequestSaveData save)
    {
        return new PartyActionRequest(
            ParseEnum<PartyActionType>(save.ActionType),
            ParseEnum<PartyActionEndType>(save.EndType),
            save.TimeToExecute,
            save.LimitCount,
            save.EndTime,
            save.ItemName,
            save.ItemLevel,
            save.ItemWeight,
            save.Quantity,
            save.TargetPartyId,
            save.Destination is null ? null : RestorePosition(save.Destination),
            save.Path.Select(RestorePosition).ToArray(),
            save.AutomationActionId);
    }

    private static MacroRecordingSession RestoreRecording(MacroRecordingSaveData save)
    {
        return new MacroRecordingSession(save.Id, save.Actions.Select(RestoreMacroAction), save.NextActionNumber);
    }

    private static PartyMacro RestoreMacro(PartyMacroSaveData save)
    {
        return new PartyMacro(save.Id, save.Name, save.Actions.Select(RestoreMacroAction));
    }

    private static MacroAction RestoreMacroAction(MacroActionSaveData save)
    {
        var kind = ParseEnum<MacroActionKind>(save.Kind);
        return kind switch
        {
            MacroActionKind.MoveTo => new MoveToMacroAction(save.Id, RestorePosition(save.Destination!)),
            MacroActionKind.Mine or MacroActionKind.CutWood => new GatherMacroAction(
                save.Id,
                kind,
                save.ItemName!,
                save.ItemLevel!.Value,
                save.ItemWeight!.Value,
                RestoreRepeat(save.Repeat!)),
            _ => throw new ArgumentOutOfRangeException(nameof(save), "Unknown Macro action kind.")
        };
    }

    private static ProgramStep RestoreProgramStep(ProgramStepSaveData save)
    {
        return new ProgramStep(save.Id, save.MacroId, RestoreRepeat(save.Repeat));
    }

    private static ProgramRunnerRuntimeState RestoreRunner(ProgramRunnerSaveData save)
    {
        return new ProgramRunnerRuntimeState(
            ParseEnum<ProgramRunnerState>(save.State),
            save.ErrorMessage,
            save.CurrentMacroActions.Select(RestoreMacroAction).ToArray(),
            save.CurrentAction is null ? null : RestoreMacroAction(save.CurrentAction),
            save.ProgramStepIndex,
            save.MacroActionIndex,
            save.StepIteration,
            save.ProgramIteration,
            save.ActionIteration,
            save.ProgramElapsedSeconds,
            save.StepElapsedSeconds,
            save.ActionElapsedSeconds,
            save.CurrentExecutionId);
    }

    private static CombatInstance RestoreCombat(CombatSaveData save, GameParty party, GameConfig config, IRandomSource? randomSource)
    {
        var heroes = save.Heroes.Select(hero => RestoreHeroCombatant(hero, party)).ToArray();
        var monsters = save.Monsters.Select(RestoreMonsterCombatant).ToArray();
        return CombatInstance.Restore(heroes, monsters, config.Combat, randomSource);
    }

    private static Combatant RestoreHeroCombatant(HeroCombatantSaveData save, GameParty party)
    {
        var cooldowns = RestoreCooldowns(save.Cooldowns);
        return Combatant.Restore(party.Members[save.MemberIndex], cooldowns.MainHand, cooldowns.Offhand);
    }

    private static Combatant RestoreMonsterCombatant(MonsterCombatantSaveData save)
    {
        var cooldowns = RestoreCooldowns(save.Cooldowns);
        return Combatant.Restore(RestoreCreature(save.Creature), cooldowns.MainHand, cooldowns.Offhand);
    }

    private static (WeaponCooldown MainHand, WeaponCooldown? Offhand) RestoreCooldowns(IReadOnlyList<WeaponCooldownSaveData> save)
    {
        WeaponCooldown? mainHand = null;
        WeaponCooldown? offhand = null;
        foreach (var cooldown in save)
        {
            var restored = WeaponCooldown.Restore(cooldown.Weapon, cooldown.CurrentCooldown, cooldown.TotalCooldown);
            if (cooldown.Slot == "MainHand")
            {
                mainHand = restored;
            }
            else if (cooldown.Slot == "Offhand")
            {
                offhand = restored;
            }
        }

        return (mainHand ?? throw new InvalidOperationException("Saved combatant missing MainHand cooldown."), offhand);
    }

    private static RepeatPolicy RestoreRepeat(RepeatPolicySaveData save)
    {
        return RestoreRepeat(save.Mode, save.RepeatCount, save.Seconds);
    }

    private static RepeatPolicy RestoreRepeat(string mode, int? count, float? seconds)
    {
        return ParseEnum<RepeatMode>(mode) switch
        {
            RepeatMode.Once => RepeatPolicy.Once,
            RepeatMode.Forever => RepeatPolicy.Forever,
            RepeatMode.Count => RepeatPolicy.Count(count!.Value),
            RepeatMode.Duration => RepeatPolicy.Duration(seconds!.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), "Unknown repeat mode.")
        };
    }

    private static TilePosition RestorePosition(TilePositionSaveData save)
    {
        return new TilePosition(save.X, save.Y);
    }

    private static TEnum ParseEnum<TEnum>(string value)
        where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: false, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Unknown {typeof(TEnum).Name} value in save: {value}.");
    }
}
