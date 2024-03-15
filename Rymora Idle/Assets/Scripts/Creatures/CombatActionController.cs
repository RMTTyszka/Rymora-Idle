using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatActionController : MonoBehaviour
{
    public List<CombatActionRow> Rows = new List<CombatActionRow>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum CombatActionTargetProperty {Life, Mana, Armor, Effect};
public enum CombatActionCondition {Lesser, Higher, Equal};
//public enum AIEffects = Effects.EffectList;
public enum CombatActionValues {_100 = 100, _90 = 90,  _80 = 80, _70 = 70, _60 = 60, _50 = 50, _40 = 40, _30 = 30, _20 = 20, _10 = 10, _0 = 0,
    Dead, Stunned, Frozen, Poison};
[System.Serializable]
public class CombatActionRow {

    public CombatActionTargetProperty test;
    public CombatActionCondition condition;
    public CombatActionValues value;
    public CombatActionRow(string targets, string tests, string conditions, string values, string power ) 
    {
    }

    public CombatActionRow() 
    {

    }

}