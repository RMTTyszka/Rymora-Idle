using UnityEngine;

namespace Map
{
    [CreateAssetMenu(fileName = "Terrain", menuName = "Tiles/Terrain/Road")]
    public class Road : MapTerrain
    {
        public Road()
        {
            moveSpeed = MoveSpeed.Road;
        }
    }
}