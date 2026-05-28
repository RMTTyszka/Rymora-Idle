using RymoraLandOfHeroes.Core.Content;

namespace RymoraLandOfHeroes.Core.Data;

public sealed record SaveData(
    string SaveVersion,
    DateTimeOffset SavedAtUtc,
    float PlayTimeSeconds,
    string? SelectedPartyId,
    string CurrentScreen,
    IReadOnlyList<PartySaveData> Parties,
    IReadOnlyList<CombatSaveData> ActiveCombats)
{
    public const string CurrentVersion = "1";
}

public sealed record TilePositionSaveData(int X, int Y);

public sealed record PartySaveData(
    string PartyId,
    TilePositionSaveData Position,
    bool IsInCombat,
    IReadOnlyList<CreatureSaveData> Members,
    IReadOnlyList<ItemSaveData> InventoryItems,
    ActionQueueSaveData ActionQueue,
    AutomationSaveData Automation);

public sealed record CreatureSaveData(
    string Name,
    float Life,
    float MaxLife,
    string SpriteId,
    IReadOnlyDictionary<string, StatSaveData> Attributes,
    IReadOnlyDictionary<string, StatSaveData> Skills,
    IReadOnlyDictionary<string, StatSaveData> Properties,
    EquipmentSaveData Equipment);

public sealed record StatSaveData(float Points, float ValueDivisor);

public sealed record EquipmentSaveData(
    WeaponTemplate? MainHand,
    WeaponTemplate? Offhand,
    ArmorTemplate? Chest)
{
    public static EquipmentSaveData Empty { get; } = new(null, null, null);
}

public sealed record ItemSaveData(string Name, int Level, float Weight, int Quantity);

public sealed record ActionQueueSaveData(
    PartyActionStateSaveData? Current,
    IReadOnlyList<PartyActionRequestSaveData> Pending);

public sealed record PartyActionStateSaveData(
    PartyActionRequestSaveData Request,
    float CurrentTime,
    float PassedTime,
    int ExecutedCount,
    bool Started);

public sealed record PartyActionRequestSaveData(
    string ActionType,
    string EndType,
    float TimeToExecute,
    int? LimitCount,
    float? EndTime,
    string? ItemName,
    int? ItemLevel,
    float? ItemWeight,
    int? Quantity,
    string? TargetPartyId,
    TilePositionSaveData? Destination,
    IReadOnlyList<TilePositionSaveData> Path,
    string? AutomationActionId);

public sealed record AutomationSaveData(
    MacroRecordingSaveData? Recording,
    IReadOnlyList<PartyMacroSaveData> Macros,
    PartyProgramSaveData Program,
    ProgramRunnerSaveData Runner)
{
    public static AutomationSaveData Empty { get; } = new(
        Recording: null,
        Macros: Array.Empty<PartyMacroSaveData>(),
        Program: new PartyProgramSaveData("Once", 1, null, 0, Array.Empty<ProgramStepSaveData>()),
        Runner: ProgramRunnerSaveData.Idle);
}

public sealed record MacroRecordingSaveData(
    string Id,
    int NextActionNumber,
    IReadOnlyList<MacroActionSaveData> Actions);

public sealed record PartyMacroSaveData(
    string Id,
    string Name,
    IReadOnlyList<MacroActionSaveData> Actions);

public sealed record MacroActionSaveData(
    string Id,
    string Kind,
    TilePositionSaveData? Destination,
    string? ItemName,
    int? ItemLevel,
    float? ItemWeight,
    RepeatPolicySaveData? Repeat);

public sealed record RepeatPolicySaveData(string Mode, int? RepeatCount, float? Seconds);

public sealed record PartyProgramSaveData(
    string RepeatMode,
    int? RepeatCount,
    float? RepeatSeconds,
    int NextStepNumber,
    IReadOnlyList<ProgramStepSaveData> Steps);

public sealed record ProgramStepSaveData(
    string Id,
    string MacroId,
    RepeatPolicySaveData Repeat);

public sealed record ProgramRunnerSaveData(
    string State,
    string? ErrorMessage,
    IReadOnlyList<MacroActionSaveData> CurrentMacroActions,
    MacroActionSaveData? CurrentAction,
    int ProgramStepIndex,
    int MacroActionIndex,
    int StepIteration,
    int ProgramIteration,
    int ActionIteration,
    float ProgramElapsedSeconds,
    float StepElapsedSeconds,
    float ActionElapsedSeconds,
    string? CurrentExecutionId)
{
    public static ProgramRunnerSaveData Idle { get; } = new(
        State: "Idle",
        ErrorMessage: null,
        CurrentMacroActions: Array.Empty<MacroActionSaveData>(),
        CurrentAction: null,
        ProgramStepIndex: 0,
        MacroActionIndex: 0,
        StepIteration: 0,
        ProgramIteration: 0,
        ActionIteration: 0,
        ProgramElapsedSeconds: 0,
        StepElapsedSeconds: 0,
        ActionElapsedSeconds: 0,
        CurrentExecutionId: null);
}

public sealed record CombatSaveData(
    string PartyId,
    string State,
    IReadOnlyList<HeroCombatantSaveData> Heroes,
    IReadOnlyList<MonsterCombatantSaveData> Monsters);

public sealed record HeroCombatantSaveData(
    int MemberIndex,
    IReadOnlyList<WeaponCooldownSaveData> Cooldowns);

public sealed record MonsterCombatantSaveData(
    CreatureSaveData Creature,
    IReadOnlyList<WeaponCooldownSaveData> Cooldowns);

public sealed record WeaponCooldownSaveData(
    string Slot,
    WeaponTemplate Weapon,
    float CurrentCooldown,
    float TotalCooldown);
