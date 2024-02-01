using UnityEngine;

public class ActionButton : MonoBehaviour
{
    public ActionEnum action;
}

public enum ActionEnum
{
    Move = 0,
    Mine = 1,
    CutWood = 2,
    EnterDungeon = 3,
    
}
