using UnityEngine;

namespace Map
{
    [CreateAssetMenu(fileName = "Terrain", menuName = "Tiles/Terrain/Mountain")]
    public class MountainTile : MapTerrain
    {
        public MountainTile()
        {
            moveSpeed = MoveSpeed.Mountain;
        }
    }
}