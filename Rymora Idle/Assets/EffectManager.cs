using System.Collections;
using System.Collections.Generic;
using Global;
using UnityEngine;

public class EffectManager : MonoBehaviour {

    private Creature C => CreatureSpawner.Creature;

    private List<Effects> effects;
    public CreatureSpawner CreatureSpawner { get; set; }


    // Use this for initialization
    void Start () {
        CreatureSpawner = transform.root.GetComponent<CreatureSpawner>();
        effects = new List<Effects>();
    }
	
    // Update is called once per frame
    void Update () {
		
    }

    public void RemoveEffect(Effects effect) {
        foreach (Effects ef in effects) {
            if (ef == effect) {
                effects.Remove(effect);
                Destroy(effect.gameObject);
                break;
            }
        }
    }
    public bool StackEffect(Effects effect) {
        foreach (Effects ef in effects) {
            if (ef.name == effect.name) {
                ef.maxDuration += effect.maxDuration;
                return true;
            }
        }
        return false;
    }


    public void AddEffect(Effects effect) {
        if (effect.isStackable){
            if (StackEffect(effect)) {
                Destroy(effect.gameObject);
                return;
            } 
        }
        effects.Add(effect);
        effects.Sort(delegate(Effects x, Effects y) {
            return (x.countdown).CompareTo(y.countdown);
        });
        effect.Effect(C);
		
        int count = 0;
        foreach(Effects ef in effects) {
            ef.transform.SetParent(transform);
            //ef.transform.parent = transform;
            RectTransform rect = ef.GetComponent<RectTransform>();
            rect.localScale = new Vector3(1,1,1);
            ef.transform.SetSiblingIndex(count);
            count++;
        }
    }
}
