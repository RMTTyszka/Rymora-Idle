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
public sealed record AddMacroToProgramIntent(string PartyId, string MacroId, RepeatPolicy Repeat) : PlayerIntent;
public sealed record MoveProgramStepIntent(string PartyId, string StepId, int NewIndex) : PlayerIntent;
public sealed record RemoveProgramStepIntent(string PartyId, string StepId) : PlayerIntent;
public sealed record RenameMacroIntent(string PartyId, string MacroId, string Name) : PlayerIntent;
public sealed record RemoveMacroActionIntent(string PartyId, string MacroId, string ActionId) : PlayerIntent;
public sealed record MoveMacroActionIntent(string PartyId, string MacroId, string ActionId, int NewIndex) : PlayerIntent;
public sealed record SetMoveActionDestinationIntent(string PartyId, string MacroId, string ActionId, TilePosition Destination) : PlayerIntent;
public sealed record SetGatherActionRepeatIntent(string PartyId, string MacroId, string ActionId, RepeatPolicy Repeat) : PlayerIntent;
public sealed record SetProgramRepeatIntent(string PartyId, RepeatPolicy Repeat) : PlayerIntent;
public sealed record SetProgramStepRepeatIntent(string PartyId, string StepId, RepeatPolicy Repeat) : PlayerIntent;
