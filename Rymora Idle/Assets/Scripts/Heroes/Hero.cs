using System.Collections.Generic;
using System.Linq;
using Map;
using UnityEngine;

namespace Heroes
{
    public class Hero : MonoBehaviour
    {
        public new string Name { get; set; }
        public int Level{ get; set; }
    
        public Inventory Inventory { get; set; }

        public List<Vector3> WayPoints;
        public MonsterMove Move { get; set; }

        [SerializeField] private MapManager mapManager;
        [SerializeField] private PartyManager partyManager;

        public Hero()
        {
            Inventory = new Inventory();
            WayPoints = new List<Vector3>();
        }

        void Start()
        {
            Move = GetComponent<MonsterMove>();
            if (string.IsNullOrWhiteSpace(Name)) Name = gameObject.name;
            print(Name);
            if (Level == 0) Level= 1;
        }

        void Update()
        {
/*            if (WayPoints.Any())
            {
                var waypoint = WayPoints.First();
                if (transform.position.Equals(waypoint))
                {
                    WayPoints.RemoveAt(0);
                    partyManager.PublishWayPointUpdated(this);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, waypoint, (float)Speed() * Time.deltaTime);
                }

            }*/
            if (WayPoints.Any())
            {
                var waypoint = WayPoints.First();
                if (transform.position.Equals(waypoint))
                {
                    WayPoints.RemoveAt(0);
                    partyManager.PublishWayPointUpdated(this);
                    Move.target = Vector3.zero;
                }
                else
                {
                    Move.target = waypoint;
                }
            }
        }


        public float Speed()
        {
            var x = mapManager.map.WorldToCell(transform.position);
            MapTerrain currentTile = mapManager.map.GetTile(x) as MapTerrain;
            return 0.01f * ((float)currentTile.moveSpeed / 100);
        }
    }
}
