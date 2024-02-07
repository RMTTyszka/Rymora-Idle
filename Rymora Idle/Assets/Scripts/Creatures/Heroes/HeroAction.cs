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
        public decimal TimeToExecute { get; set; }
        public bool Started { get; set; }
        public MapTerrain Terrain { get; set; }
        public HeroActionType ActionType { get; set; }
        
    }

    public enum HeroActionType
    {
        Travel = 0,
        Mine = 1,
        CutWood = 2,
    }
}