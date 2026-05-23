using RymoraLandOfHeroes.Core.Common;
using RymoraLandOfHeroes.Core.Party;

namespace RymoraLandOfHeroes.Core.Application;

public abstract record PlayerIntent;

public sealed record SelectPartyIntent(string PartyId) : PlayerIntent;

public sealed record EnqueueActionIntent(string PartyId, PartyActionRequest Request) : PlayerIntent;

public sealed record ExecuteMapActionIntent(string PartyId, TilePosition Position, PartyActionType ActionType) : PlayerIntent;
