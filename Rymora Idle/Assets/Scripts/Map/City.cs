using UnityEngine;

namespace Map
{
    [CreateAssetMenu(fileName = "Terrain", menuName = "Tiles/Terrain/City")]
    public class City : MapTerrain
    {
        public City()
        {
            moveSpeed = MoveSpeed.Road;
        }
    }
}