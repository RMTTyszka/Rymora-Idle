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
        [SerializeField] public MapTerrain CurrentTile;

        [SerializeField] private MapManager mapManager;
        [SerializeField] private PartyManager partyManager;
        
        public Skills Skills { get; set; }

        public Hero()
        {
            Inventory = new Inventory();
            WayPoints = new List<Vector3>();
            Skills = new Skills();
        }

        void Start()
        {
            Move = GetComponent<MonsterMove>();
            if (string.IsNullOrWhiteSpace(Name)) Name = gameObject.name;
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
                    CurrentTile = mapManager.map.GetTile(x) as MapTerrain;
                    if (CurrentTile == null)
                    {
                        WayPoints.Clear();
                        partyManager.PublishWayPointUpdated(this);
                    }
                    else
                    {
                        var newPosition = Vector3.MoveTowards(transform.position, waypoint, (float)Speed(CurrentTile.moveSpeed) * Time.deltaTime);
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

        public void Mine()
        {
            if (CurrentTile.CanMine())
            {
                var mine = CurrentTile as Mountain;
                var metal = mine.GetMetal();
                var rollValue = Skills.Mine.Roll();
                var difficult = metal.Level * 10 + 50;
                print(rollValue);
                print(difficult);
                if (rollValue > difficult)
                {
                    Inventory.AddItem(metal);
                    print(Inventory.Items[0].Name);
                }
            }
        }

    }

    public class Skill
    {
        public Skill()
        {
            Bonuses = new List<Bonus>();
        }
            public int Value { get; set; }
            public List<Bonus> Bonuses { get; set; }

            public int TotalValue()
            {
                return Value + GetTotalBonus();
            }

            public int GetTotalBonus()
            {
                var innate = Bonuses.Where(bonus => bonus.Type == BonusType.Innate).Sum(bonus => bonus.Value);
                var magic = Bonuses.Where(bonus => bonus.Type == BonusType.Magic).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
                var equipment = Bonuses.Where(bonus => bonus.Type == BonusType.Equipment).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
                var food = Bonuses.Where(bonus => bonus.Type == BonusType.Food).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
                var furniture = Bonuses.Where(bonus => bonus.Type == BonusType.Furniture).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
                return innate + magic + equipment + food + furniture;
            }

            public int Roll()
            {
                var rollValue = Random.Range(1, 101);
                return rollValue + GetTotalBonus();
            }
    }

    public class Bonus
    {
        public int Value { get; set; }
        public BonusType Type { get; set; }
        public float StartedAt { get; set; }
        public float ExpiresAt { get; set; }
    }

    public enum BonusType
    {
        Innate = 0,
        Magic = 1,
        Equipment = 2,
        Food = 3,
        Furniture = 4
    }
}
