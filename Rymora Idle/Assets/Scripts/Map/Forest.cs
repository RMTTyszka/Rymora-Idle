using System;
using Items.Metals;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Map
{
    [CreateAssetMenu(fileName = "Terrain", menuName = "Tiles/Terrain/Forest")]
    public class Forest : MapTerrain
    {
        public Forest()
        {
            moveSpeed = MoveSpeed.Forest;
        }
        public Wood GetMaterial()
        {
            var value = Random.Range(1, 101);
            if (value < 10)
            {
                return GetMaterialByLevel(-4);

            }    
            if (value < 20)
            {
                return GetMaterialByLevel(-3);

            }     
            if (value < 30)
            {
                return GetMaterialByLevel(-2);

            }  
            if (value < 30)
            {
                return GetMaterialByLevel(-1);
            }        
            if (value < 40)
            {
                return GetMaterialByLevel(0);
            }    
            if (value < 90)
            {
                return GetMaterialByLevel(1);
            }
            return GetMaterialByLevel(2);
            
        }

        private Wood GetMaterialByLevel(int i)
        {
            var metalIndex = Level() + i;
            metalIndex = Math.Max(metalIndex, 0);
            metalIndex = Math.Min(metalIndex, 9);
            return Woods[metalIndex];
        }

        public static Wood[] Woods => new Wood[] {new Oak(),new Oak(),new Oak(),new Oak(),new Oak(),new Oak(),new Oak(),new Oak(),new Oak(),new Oak(),new Oak(),};
    }
}