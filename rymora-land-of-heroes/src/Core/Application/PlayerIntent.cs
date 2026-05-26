using RymoraLandOfHeroes.Core.Automation;
using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.Core.Application;

public abstract record PlayerIntent;

public sealed record SelectPartyIntent(string PartyId) : PlayerIntent;

public sealed record EnqueueActionIntent(string PartyId, PartyActionRequest Request) : PlayerIntent;

public sealed record ExecuteMapActionIntent(string PartyId, TilePosition Position, PartyActionType ActionType) : PlayerIntent;

public sealed record StartRecordingMacroIntent(string PartyId) : PlayerIntent;

public sealed record SaveRecordingMacroIntent(string PartyId, string Name) : PlayerIntent;

public sealed record CancelRecordingMacroIntent(string PartyId) : PlayerIntent;
public sealed record RecordMoveActionIntent(string PartyId, TilePosition Target) : PlayerIntent;
public sealed record RecordGatherActionIntent(string PartyId, TilePosition Target, MacroActionKind Kind, string ItemName, int ItemLevel, float ItemWeight) : PlayerIntent;
public sealed record PlayProgramIntent(string PartyId) : PlayerIntent;
public sealed record PauseProgramIntent(string PartyId) : PlayerIntent;
public sealed record StopProgramIntent(string PartyId) : PlayerIntent;
