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
           if (WayPoints.Any())
            {
                var waypoint = WayPoints.First();
                if (Vector3.Distance(waypoint, transform.position) <= 0.05f)
                {
                    WayPoints.RemoveAt(0);
                    partyManager.PublishWayPointUpdated(this);
                    transform.position = waypoint;
                }
                else
                {
                    var x = mapManager.map.WorldToCell(transform.position);
                    MapTerrain currentTile = mapManager.map.GetTile(x) as MapTerrain;
                    if (currentTile == null)
                    {
                        WayPoints.Clear();
                        partyManager.PublishWayPointUpdated(this);
                    }
                    else
                    {
                        var newPosition = Vector3.MoveTowards(transform.position, waypoint, (float)Speed(currentTile.moveSpeed) * Time.deltaTime);
                        var newTile = mapManager.map.GetTile(mapManager.map.WorldToCell(newPosition)) as MapTerrain;
                        if (newTile != null)
                        {
                            transform.position = newPosition;
                        }
                        else
                        {
                            WayPoints.Clear();
                            partyManager.PublishWayPointUpdated(this);
                        }

                    }

                }

            }
/*            if (WayPoints.Any())
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
            }*/
        }


        public float Speed(MoveSpeed moveSpeed)
        {
            return 1 * ((float)moveSpeed / 100);
        }
    }
}
