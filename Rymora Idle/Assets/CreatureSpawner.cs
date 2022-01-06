using System;
using Global;
using UnityEngine;
using UnityEngine.UI;

public class CreatureSpawner : MonoBehaviour
{
    public Creature Creature { get; set; }
    public GameObject CBTpref;

    // Start is called before the first frame update
    void Start()
    {
        InitCBT(12.ToString(), "Heal");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Add(Creature creature)
    {
        Creature = creature;
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Creature.Image;
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
        CBT.transform.SetParent(transform.Find("CombatNodeCanvas"));
        CBTRect.transform.localPosition = CBTpref.transform.localPosition;
    //    CBTRect.transform.localScale = CBTpref.transform.localScale;
        CBTRect.transform.localRotation = CBTpref.transform.localRotation;
        CBT.GetComponent<Text>().text = text;

        CBT.GetComponent<Animator>().SetTrigger(trigger);
        Destroy(CBT, 2f);
    }
}
