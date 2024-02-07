using System;
using Global;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Map
{
	public abstract class RegionTile : Tile
	{
		[FormerlySerializedAs("name")] public string regionName;
	}
}
