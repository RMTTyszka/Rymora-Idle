using System;
using System.Collections.Generic;
using System.Linq;
using Global;
using Items;
using Items.Metals;
using Map;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Heroes
{
    public class Party : MonoBehaviour
    {
        public CombatInstance CombatInstance { get; set; }
        public GameData GameData { get; set; }
        public CombatManager CombatManager { get; set; }
        public Creature Hero => Members.FirstOrDefault();

        public List<Creature> Members { get; set; } = new();

        public List<Vector3> WayPoints;
        public MapMover Move { get; set; }
        private MapTerrain CurrentTerrain { get; set; }
        private Region CurrentRegion { get; set; }

        private MapManager MapManager;
        private PartyManager PartyManager;

        public decimal? CurrentActionTime{ get; set; } 
        public decimal? EndActionTime { get; set; } 
        public decimal ActionPerformance { get; set; } 
        public RawMaterial CurrentMaterial { get; set; }
        public HeroAction CurrentAction { get; set; }
        public Queue<HeroAction> NextActions { get; set; }
        
        public float TimeFromLastEncounterCheck { get; set; }


        public Party()
        {
            Members.Add(new Creature());
            WayPoints = new List<Vector3>();
            ActionPerformance = 1m;
            NextActions = new Queue<HeroAction>();
        }

        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(Hero.Name)) Hero.Name = gameObject.name;
            if (Hero.Level == 0) Hero.Level= 1;
        }

        private void Awake()
        {
            MapManager = FindAnyObjectByType<MapManager>();
            PartyManager = FindAnyObjectByType<PartyManager>();
            GameData = FindAnyObjectByType<GameData>();
            CombatManager = FindAnyObjectByType<CombatManager>();
        }

        void Start()
        {
            Move = GetComponent<MapMover>();

        }

        void Update()
        {
            CheckForEncounter();
            TryAction();
        }

        private void CheckForEncounter()
        {
            var shouldCheckForEncounter = !InCombat;
            shouldCheckForEncounter = shouldCheckForEncounter && CurrentAction?.ActionType is HeroActionType.Travel;
            shouldCheckForEncounter = shouldCheckForEncounter && CurrentTerrain is not City;
            if (shouldCheckForEncounter)
            {
                TimeFromLastEncounterCheck -= Time.deltaTime;
                if (TimeFromLastEncounterCheck < 0)
                {
                    DoCheckEncounter();
                    TimeFromLastEncounterCheck += GameData.encounterInterval;
                }
            }
        }

        private void DoCheckEncounter()
        {
            var encounterRateModifier = CurrentTerrain.encounterRateModifier;
            var random = Random.Range(1, 101);
            random += encounterRateModifier;
            if (random <= GameData.encounterProbability)
            {
                Debug.Log("Initiating Encounter");
                InCombat = true;
                StartEncounter();
            }
        }

        private void StartEncounter()
        {
            var encounter = CurrentRegion.encounters.ElementAtOrDefault(Random.Range(0, CurrentRegion.encounters.Count));
            if (encounter is null)
            {
                InCombat = false;
                return;
            }

            var level = CurrentTerrain.Level();
            InCombat = true;
            CombatInstance = CombatManager.StartCombat(this, encounter, level);
            
        }

        public bool InCombat { get; set; }


        private void TryAction()
        {
            if (InCombat)
            {
                return;
            }

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
                ActionType = HeroActionType.Travel,
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
                    PartyManager.PublishActionsUpdated(this);
                    transform.position = waypoint;
                    CurrentAction = null;
                }
                else
                {
                    var position = MapManager.terrainMap.WorldToCell(transform.position);
                    CurrentTerrain = MapManager.terrainMap.GetTile(position) as MapTerrain;
                    CurrentRegion = MapManager.regionMap.GetTile(position) as Region;
                    if (CurrentTerrain is null)
                    {
                        WayPoints.Clear();
                        PartyManager.PublishActionsUpdated(this);
                        CurrentAction = null;
                        NextActions.Clear();
                    }
                    else
                    {
                        var newPosition = Vector3.MoveTowards(transform.position, waypoint, (float)Speed(CurrentTerrain.moveSpeed) * Time.deltaTime);
                        var newTile = MapManager.terrainMap.GetTile(MapManager.terrainMap.WorldToCell(newPosition)) as MapTerrain;
                        if (newTile is not null)
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
        public void TryCutWood()
        {
            if (CurrentTerrain.CanCutWood())
            {
                var terrain = CurrentTerrain as Forest;
                var material = terrain.GetMaterial();
                CurrentMaterial = material;
                var rollValue = Hero.Skills.Lumberjacking.Roll(0);
                var difficult = material.Level * 10 + 50;
                var actionProficient = rollValue - difficult;
                ActionPerformance = actionProficient / 100 + 1;
                CurrentActionTime = 0;
                EndActionTime = Skills.CutWoodTime;
                CurrentAction.ExecutionAction = CutWood;
            }
        }
        public void InitiateCuttingWood(HeroAction heroAction)
        {
            heroAction.Action = TryCutWood;
            heroAction.ExecutionAction = CutWood;
            heroAction.TimeToExecute = Skills.CutWoodTime;
            NextActions.Enqueue(heroAction);
        }
        public void TryMine()
        {
            if (CurrentTerrain.CanMine())
            {
                var mine = CurrentTerrain as Mountain;
                var metal = mine.GetMaterial();
                CurrentMaterial = metal;
                var rollValue = Hero.Skills.Get(Skill.Mining).Roll(0);
                var difficult = metal.Level * 10 + 50;
                var actionProficient = rollValue - difficult;
                ActionPerformance = actionProficient / 100 + 1;
                CurrentActionTime = 0;
                EndActionTime = Skills.MineTime;
                CurrentAction.ExecutionAction = Mine;
            }
        }
        public bool RollForChallenge(Skill e, int difficult, int challengeLevel)
        {
            var rollValue = Random.Range(1, 101);
            var skillValue = Hero.Skills.Get(e).GetValue(challengeLevel);
            rollValue += skillValue;
            return rollValue > difficult;
        }

        public void Mine()
        {
            var difficult = CurrentMaterial.Level * 10 + 50;
            var success = RollForChallenge(Skill.Mining, difficult, CurrentMaterial.Level);
            if (success)
            {
                AddItem(CurrentMaterial);
            }

            CurrentAction.PassedTime += (decimal)Time.deltaTime;
            CurrentAction.ExecutedCount++;
        }      
        public void CutWood()
        {
            var difficult = CurrentMaterial.Level * 10 + 50;
            var success = RollForChallenge(Skill.Lumberjacking, difficult, CurrentMaterial.Level);
            if (success)
            {
                AddItem(CurrentMaterial);
            }

            CurrentAction.PassedTime += (decimal)Time.deltaTime;
            CurrentAction.ExecutedCount++;
        }

        public void AddItem(Item item)
        {
            Hero.Inventory.AddItem(item);
            PartyManager.PublishInventoryUpdate(this);
            print($"Acquired a {item.Name}");
        }      
        public void RemoveItem(Item item, int quantity)
        {
            Hero.Inventory.RemoveItem(item, quantity);
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
