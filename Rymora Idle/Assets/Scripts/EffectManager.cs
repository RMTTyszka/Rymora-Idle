using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour {

    public Creature Creature { get; set; }

    private List<Effect> Effect;


    // Use this for initialization
    void Start () {
        Effect = new List<Effect>();
    }
	
    // Update is called once per frame
    void Update () {
		
    }

    public void RemoveEffect(Effect effect) {
        foreach (Effect ef in Effect) {
            if (ef == effect) {
                Effect.Remove(effect);
                Destroy(effect.gameObject);
                break;
            }
        }
    }
    public bool StackEffect(Effect effect) {
        foreach (Effect ef in Effect) {
            if (ef.name == effect.name) {
                ef.maxDuration += effect.maxDuration;
                return true;
            }
        }
        return false;
    }


    public void AddEffect(Effect effect) {
        if (effect.isStackable){
            if (StackEffect(effect)) {
                Destroy(effect.gameObject);
                return;
            } 
        }
        Effect.Add(effect);
        Effect.Sort(delegate(Effect x, Effect y) {
            return (x.countdown).CompareTo(y.countdown);
        });
       // effect.Effect(C.Combatant);
		
        int count = 0;
        foreach(Effect ef in Effect) {
            ef.transform.SetParent(transform);
            //ef.transform.parent = transform;
            RectTransform rect = ef.GetComponent<RectTransform>();
            rect.localScale = new Vector3(1,1,1);
            ef.transform.SetSiblingIndex(count);
            count++;
        }
    }
}