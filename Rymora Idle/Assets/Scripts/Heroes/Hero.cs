using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heroes
{
    public class Hero : MonoBehaviour
    {
        public new string Name { get; set; }
        public int Level{ get; set; }
    
        public Inventory Inventory { get; set; }
        
        public List<Vector3> Waypoints { get; set; }

        public Hero()
        {
            Inventory = new Inventory();
            Waypoints = new List<Vector3>();
        }

        void Update()
        {
            if (Waypoints.Any())
            {
                var waypoint = Waypoints.First();
                transform.position = Vector3.MoveTowards(transform.position, waypoint, (float)Speed() * Time.deltaTime);
            }
        }


        public decimal Speed()
        {
            return 1;
        }
    }
}
