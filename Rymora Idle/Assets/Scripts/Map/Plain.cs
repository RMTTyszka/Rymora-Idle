using UnityEngine;

namespace Map
{
    [CreateAssetMenu(fileName = "Terrain", menuName = "Tiles/Terrain/Plain")]
    public class Plain : MapTerrain
    {
        public Plain()
        {
            moveSpeed = MoveSpeed.Plain;
        }
    }
}