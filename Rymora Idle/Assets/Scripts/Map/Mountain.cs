using UnityEngine;

namespace Map
{
    [CreateAssetMenu(fileName = "Terrain", menuName = "Tiles/Terrain/Mountain")]
    public class Mountain : MapTerrain
    {
        public Mountain()
        {
            moveSpeed = MoveSpeed.Mountain;
        }
    }
}