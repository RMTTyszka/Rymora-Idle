using System.Collections.Generic;
using Global;
using UnityEngine;

public abstract class Power : ScriptableObject {

	//public  string name;
	public float castingTime;
	public int manaCost;
	public int numberTarget;
	public string description;
	public float range;
    

	public List<CombatActionResult> Cast (Creature caster, Creature target) {
		Creature[] targets = {target};
		return Cast(caster,targets);
	}
	public virtual List<CombatActionResult> Cast(Creature caster, Creature[] targets) {
		return new List<CombatActionResult>();
	}



    
}
