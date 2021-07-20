using UnityEngine;

namespace Map
{
    [CreateAssetMenu(fileName = "Terrain", menuName = "Tiles/Terrain/Forest")]
    public class Forest : MapTerrain
    {
        public Forest()
        {
            moveSpeed = MoveSpeed.Forest;
        }
    }
}