using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Map
{
    [CreateAssetMenu(fileName = "Terrain", menuName = "Tiles/Terrain/City")]
    public class City : Place
    {
        public List<string> shops;
        public City()
        {
            moveSpeed = MoveSpeed.Road;
        }
    }
}