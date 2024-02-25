using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Image))]
public class Effect : MonoBehaviour {

    public enum EffectList {Dead, Stunned, Poisoned, Frozen, Scared};

    public bool isHarmful;
    public bool isStackable;
    public float maxDuration;
    public float countdown;
    public Combatant targetChar = null;
    public WeaponInstance targetWeap = null;

    public virtual void Start() {
    }

    // Update is called once per frame
    public virtual void Update () {
		
    }

    public virtual  void RemoveEffect() {

    }
}