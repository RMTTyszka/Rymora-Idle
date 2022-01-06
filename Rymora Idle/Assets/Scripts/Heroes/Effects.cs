using System.Collections;
using System.Collections.Generic;
using Global;
using Items.Equipables.Weapons;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Image))]
public class Effects : MonoBehaviour {

	public enum EffectList {Dead, Stunned, Poisoned, Frozen, Scared};

	public bool isHarmful;
	public bool isStackable;
	public float maxDuration;
	public float countdown;
	public Creature targetChar = null;
	public Weapon targetWeap = null;

	public virtual void Start() {
	}
	// Use this for initialization
	public virtual void Effect() {
		
	}
	public virtual void Effect(Creature target) {

	}

	// Update is called once per frame
	public virtual void Update () {
		
	}

	public virtual  void RemoveEffect() {

	}
}
