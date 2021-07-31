using System;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Items.Metals;
using UnityEngine;
using Random = UnityEngine.Random;

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