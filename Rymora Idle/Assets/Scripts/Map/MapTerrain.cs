using System;
using Global;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Map
{
	public abstract class MapTerrain : Tile 
	{
		public bool isWalkable = true;
		public MoveSpeed moveSpeed;
		public QualityGrade quality = QualityGrade.Common;
		public Guid Id { get; set; }
		
		public int Level()
		{
			return (int)quality + 1;
		}

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
