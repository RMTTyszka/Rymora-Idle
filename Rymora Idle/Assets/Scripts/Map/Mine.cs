using UnityEngine;

namespace Map
{
    [CreateAssetMenu(fileName = "Terrain", menuName = "Tiles/Terrain/Mine")]
    public class Mine : MapTerrain
    {
        public Mine()
        {
            moveSpeed = MoveSpeed.Mountain;
        }
    }
}