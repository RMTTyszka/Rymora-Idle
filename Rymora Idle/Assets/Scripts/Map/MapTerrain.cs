using System;
using Global;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Map
{
	public class MapTerrain : Tile {

		public bool isWalkable = true;
		public MoveSpeed moveSpeed;
		public QualityGrade quality = QualityGrade.Common;

		public bool CanMine()
		{
			return this is Mountain;
		}	
		public bool CanCutWood()
		{
			return this is Forest;
		}
	}
}
