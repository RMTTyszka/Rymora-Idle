using System;
using Items.Metals;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Map
{
    [CreateAssetMenu(fileName = "Terrain", menuName = "Tiles/Terrain/Mountain")]
    public class Mountain : MapTerrain
    {
        public Mountain()
        {
            moveSpeed = MoveSpeed.Mountain;
        }
        public Metal GetMetal()
        {
            var value = Random.Range(1, 101);
            if (value < 10)
            {
                return GetMetalByLevel(-4);

            }    
            if (value < 20)
            {
                return GetMetalByLevel(-3);

            }     
            if (value < 30)
            {
                return GetMetalByLevel(-2);

            }  
            if (value < 30)
            {
                return GetMetalByLevel(-1);
            }        
            if (value < 40)
            {
                return GetMetalByLevel(0);
            }    
            if (value < 90)
            {
                return GetMetalByLevel(1);
            }
            return GetMetalByLevel(2);
            
        }

        private Metal GetMetalByLevel(int i)
        {
            var metalIndex = Level() + i;
            metalIndex = Math.Max(metalIndex, 0);
            metalIndex = Math.Min(metalIndex, 9);
            return Metals[metalIndex];
        }

        public static Metal[] Metals => new Metal[] {new Iron(), new Bronze(), new Copper(), new Silver(), new Gold(), new Mythril()};
    }
}