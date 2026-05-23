using RymoraLandOfHeroes.Core.Common;

namespace RymoraLandOfHeroes.Core.World;

public sealed record WorldTile(TilePosition Position, TerrainData Terrain, RegionData Region);
