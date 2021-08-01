using System;
using Map;

namespace Heroes
{
    public class HeroAction
    {
        public ActionEndType ActionEndType { get; set; }
        public int? LimitCount { get; set; }
        public int? ExecutedCount { get; set; }
        public Action Action { get; set; } 
        public Action ExecutionAction { get; set; } 
        public string ItemName { get; set; }
        public decimal? EndTime { get; set; }
        public decimal? PassedTime { get; set; }
        public int TimeToExecute { get; set; }
        public bool Started { get; set; }
        public MapTerrain Terrain { get; set; }
        public string ActionName { get; set; }
        
    }
}