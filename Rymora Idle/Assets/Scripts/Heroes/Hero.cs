using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Items.Metals;
using Map;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

namespace Heroes
{
    public class Hero : MonoBehaviour
    {
        public string Name { get; set; }
        public int Level{ get; set; }
    
        public Inventory Inventory { get; set; }

        public List<Vector3> WayPoints;
        public MonsterMove Move { get; set; }
        [SerializeField] public MapTerrain CurrentTile;

        [SerializeField] private MapManager mapManager;
        [SerializeField] private PartyManager partyManager;
        
        public decimal? CurrentActionTime{ get; set; } 
        public decimal? EndActionTime { get; set; } 
        public decimal ActionPerformance { get; set; } 
        public Metal CurrentMaterial { get; set; }
        public HeroAction CurrentAction { get; set; }
        public Queue<HeroAction> NextActions { get; set; }

        public Skills Skills { get; set; }

        public Hero()
        {
            Inventory = new Inventory();
            WayPoints = new List<Vector3>();
            Skills = new Skills();
            ActionPerformance = 1m;
            NextActions = new Queue<HeroAction>();
        }

        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(Name)) Name = gameObject.name;
            if (Level == 0) Level= 1;
        }

        void Start()
        {
            Move = GetComponent<MonsterMove>();

        }

        void Update()
        {
            TryAction();
        }



        private void TryAction()
        {
            ProcessNextAction();

            if (CurrentAction != null)
            {
                if (CurrentAction.Started)
                {
                    CurrentActionTime += (decimal)Time.deltaTime * ActionPerformance;
                    if (CurrentActionTime >= EndActionTime)
                    {
                        CurrentAction.Started = false;
                        CurrentAction.ExecutionAction.Invoke();
                        ResetAction();
                    }  
                }
                else
                {
                    CurrentAction.Started = true;
                    CurrentAction.Action.Invoke();
                }

            }
        }
        public void InitiateMovement(Vector3 waypoint)
        {
            WayPoints.Add(waypoint);
            NextActions.Enqueue(new HeroAction
            {
                ActionEndType = ActionEndType.ByCount,
                Action = TryMove,
                LimitCount = 1,
                ExecutionAction = TryMove,
                TimeToExecute = 0,
                ActionName = "Travel"
            });
        }       

        private void TryMove()
        {
            if (WayPoints.Any())
            {
                var waypoint = WayPoints.First();
                var isCloseEnough = Vector3.Distance(waypoint, transform.position) <= 0.05f;
                if (isCloseEnough)
                {
                    WayPoints.RemoveAt(0);
                    partyManager.PublishActionsUpdated(this);
                    transform.position = waypoint;
                    CurrentAction = null;
                }
                else
                {
                    var x = mapManager.map.WorldToCell(transform.position);
                    CurrentTile = mapManager.map.GetTile(x) as MapTerrain;
                    if (CurrentTile == null)
                    {
                        WayPoints.Clear();
                        partyManager.PublishActionsUpdated(this);
                        CurrentAction = null;
                        NextActions.Clear();
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
                            partyManager.PublishActionsUpdated(this);
                            CurrentAction = null;
                            NextActions.Clear();
                        }
                    }

                }

            }
        }


        public float Speed(MoveSpeed moveSpeed)
        {
            return 1 * ((float)moveSpeed / 100);
        }
        public void InitiateMining(HeroAction heroAction)
        {
            heroAction.Action = TryMine;
            heroAction.ExecutionAction = Mine;
            heroAction.TimeToExecute = Skills.MineTime;
            NextActions.Enqueue(heroAction);
        }
        public void TryMine()
        {
            if (CurrentTile.CanMine())
            {
                var mine = CurrentTile as Mountain;
                var metal = mine.GetMetal();
                CurrentMaterial = metal;
                var rollValue = Skills.Mine.Roll();
                var difficult = metal.Level * 10 + 50;
                var actionProficient = rollValue - difficult;
                ActionPerformance = actionProficient / 100 + 1;
                CurrentActionTime = 0;
                EndActionTime = Skills.MineTime;
                CurrentAction.ExecutionAction = Mine;
            }
        }
        public void Mine()
        {
            var rollValue = Skills.Mine.Roll();
            var difficult = CurrentMaterial.Level * 10 + 50;
            if (rollValue > difficult)
            {
                AddItem(CurrentMaterial);
            }

            CurrentAction.PassedTime += (decimal)Time.deltaTime;
            CurrentAction.ExecutedCount++;
        }

        public void AddItem(Item item)
        {
            Inventory.AddItem(CurrentMaterial);
            partyManager.PublishInventoryUpdate(this);
            print($"Acquired a {Inventory.Items[0].Name}");
        }

        public void ResetAction()
        {
            ActionPerformance = 1;
            CurrentActionTime = 0;
        }

        public void ProcessNextAction()
        {
            if (CurrentAction == null && NextActions.Any())
            {
                CurrentAction = NextActions.Dequeue();
                CurrentActionTime = 0;
                EndActionTime = CurrentAction.TimeToExecute;
            }

            if (CurrentAction != null)
            {
                switch (CurrentAction.ActionEndType)
                {
                    case ActionEndType.ByCount:
                        if (CurrentAction.ExecutedCount >= CurrentAction.LimitCount)
                        {
                            CurrentAction = null;
                        }
                        break;
                    case ActionEndType.ByItemQuantity:
                        break;
                    case ActionEndType.ByTime:
                        if (CurrentAction.PassedTime >= CurrentAction.EndTime)
                        {
                            CurrentAction = null;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }



    }
}
