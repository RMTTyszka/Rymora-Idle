using System;
using System.Threading;
using Global;
using UnityEngine;
using UnityEngine.UI;

public class CreatureSpawner : MonoBehaviour
{
    public Creature Creature { get; set; }
    public GameObject CBTpref;
    public bool Active { get; set; }
    public SpriteRenderer SpriteRenderer { get; set; }
    public CombatNodeCanvas CombatNodeCanvas { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        CombatNodeCanvas = GetComponentInChildren<CombatNodeCanvas>(true);
        Reset();

    }

    public void Reset()
    {
        if (gameObject.activeSelf)
        {
            Creature = null;
            SpriteRenderer.sprite = null;
            CombatNodeCanvas.gameObject.SetActive(false);  
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Add(Creature creature)
    {
        Creature = creature;
        SpriteRenderer.sprite = Creature.Image;
        CombatNodeCanvas.gameObject.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.33f);
    }

    public void ProcessAction(CombatActionResult actionResult)
    {
        switch (actionResult.ActionType)
        {
            case CombatActionType.PhysicalDamage:
                InitCBT(actionResult.Value.ToString(), actionResult.ActionType.ToString());
                break;
            case CombatActionType.CriticalDamage:
                InitCBT(actionResult.Value.ToString(), actionResult.ActionType.ToString());
                break;
            case CombatActionType.Evade:
                InitCBT(actionResult.Value.ToString(), actionResult.ActionType.ToString());
                break;
            case CombatActionType.Heal:
                InitCBT(actionResult.Value.ToString(), actionResult.ActionType.ToString());
                break;
            case CombatActionType.MagicDamage:
                InitCBT(actionResult.Value.ToString(), actionResult.ActionType.ToString());
                break;
            case CombatActionType.CounterPhysicalDamage:
                InitCBT(actionResult.Value.ToString(), actionResult.ActionType.ToString());
                break;
            case CombatActionType.CounterCriticalDamage:
                InitCBT(actionResult.Value.ToString(), actionResult.ActionType.ToString());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public void InitCBT(string text, string trigger) {
        GameObject CBT = Instantiate(CBTpref);
        RectTransform CBTRect = CBT.GetComponent<RectTransform>();
        CBT.transform.SetParent(CombatNodeCanvas.transform);
        CBTRect.transform.localPosition = CBTpref.transform.localPosition;
    //    CBTRect.transform.localScale = CBTpref.transform.localScale;
        CBTRect.transform.localRotation = CBTpref.transform.localRotation;
        CBT.GetComponent<Text>().text = text;

        CBT.GetComponent<Animator>().SetTrigger(trigger);
        Destroy(CBT, 2f);
    }
}
