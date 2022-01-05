using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Items.Metals;
using Map;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Heroes
{
    public class PartyNode : MonoBehaviour
    {
        public Party Party { get; set; }

        public CombatManager combatManager;

        public List<Vector3> WayPoints { get; set; }
        private MapMover Move { get; set; }
        public MapTerrain CurrentTile { get; set; }

        private MapManager MapManager { get; set; }
        private PartyManager PartyManager { get; set; }
        public Camera combatCamera;
        
        public decimal? CurrentActionTime{ get; set; } 
        public decimal? EndActionTime { get; set; } 
        public decimal ActionPerformance { get; set; } 
        public Metal CurrentMaterial { get; set; }
        public HeroAction CurrentAction { get; set; }
        public Queue<HeroAction> NextActions { get; set; }

        public bool InCombat { get; set; }

        public PartyNode()
        {
            Party = new Party();
            WayPoints = new List<Vector3>();
            ActionPerformance = 1m;
            NextActions = new Queue<HeroAction>();
        }

        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(Party.Hero.Name)) Party.Hero.Name = gameObject.name;
            if (Party.Hero.Level == 0) Party.Hero.Level= 1;
        }

        void Start()
        {
            Move = GetComponent<MapMover>();
            MapManager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
            PartyManager = GameObject.FindGameObjectWithTag("PartyManager").GetComponent<PartyManager>();

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
                var currentPosition = transform.position;
                var waypoint = WayPoints.First();
                var isCloseEnough = Vector3.Distance(waypoint, transform.position) <= 0.05f;
                if (isCloseEnough)
                {
                    WayPoints.RemoveAt(0);
                    PartyManager.PublishActionsUpdated(this);
                    transform.position = waypoint;
                    CurrentAction = null;
                }
                else
                {
                    var x = MapManager.map.WorldToCell(currentPosition);
                    var newTile = MapManager.map.GetTile<MapTerrain>(x);
                    var distanceSinceLastCombatCheck = LastCombatCheckPosition - currentPosition;
                    if (distanceSinceLastCombatCheck.sqrMagnitude > 0.5)
                    {
                        Debug.Log($"distance between checks = {distanceSinceLastCombatCheck.sqrMagnitude}");
                        LastCombatCheckPosition = currentPosition;
                        CheckCombat();
                    }
                    CurrentTile = newTile;
                    if (InCombat)
                    {
                        return;
                    }

                    if (CurrentTile == null)
                    {
                        WayPoints.Clear();
                        PartyManager.PublishActionsUpdated(this);
                        CurrentAction = null;
                        NextActions.Clear();
                    }
                    else
                    {
                        var newPosition = Vector3.MoveTowards(currentPosition, waypoint, (float)Speed(CurrentTile.moveSpeed) * Time.deltaTime);
                        if (newTile != null)
                        {
                            transform.position = newPosition;
                        }
                        else
                        {
                            WayPoints.Clear();
                            PartyManager.PublishActionsUpdated(this);
                            CurrentAction = null;
                            NextActions.Clear();
                        }
                    }

                }

            }
        }

        public Vector3 LastCombatCheckPosition { get; set; }

        private void CheckCombat()
        {
            var roll = Random.Range(1, 101);
            Debug.Log($"{roll}");
            if (roll > 10)
            {
                 InCombat = InitiateCombat();
            }
        }


        public bool InitiateCombat()
        {
            var encounterNodes = GameObject.FindGameObjectsWithTag("EncounterNode").Select(e => e.GetComponent<EncounterTile>());
            EncounterTile encounterTemplate = null;
            var closestDistanceSqr = Mathf.Infinity;
            var currentPosition = transform.position;
            foreach(var encounterNode in encounterNodes)
            {
                var directionToTarget = encounterNode.transform.position - currentPosition;
                var dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget > 0.25) continue;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    encounterTemplate = encounterNode;
                    Debug.Log("FoundEncounter");
                }
            }

            if (encounterTemplate != null)
            {
                var encounter = encounterTemplate.encounters[Random.Range(0, encounterTemplate.encounters.Count - 1)];
                combatManager.InitiateCombat(encounter);
                return true;
            }

            return false;



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
                var rollValue = Party.Hero.Skills.Mining.Roll(metal.Level);
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
            var rollValue = Party.Hero.Skills.Mining.Roll();
            var difficult = CurrentMaterial.Difficulty;
            if (rollValue > difficult)
            {
                AddItem(CurrentMaterial);
            }

            CurrentAction.PassedTime += (decimal)Time.deltaTime;
            CurrentAction.ExecutedCount++;
        }

        public void AddItem(Item item)
        {
            Party.Hero.Inventory.AddItem(item);
            PartyManager.PublishInventoryUpdate(this);
            print($"Acquired a {item.Name}");
        }      
        public void RemoveItem(Item item, int quantity)
        {
            Party.Hero.Inventory.RemoveItem(item, quantity);
            PartyManager.PublishInventoryUpdate(this);
            print($"Removed a {item.Name}");
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