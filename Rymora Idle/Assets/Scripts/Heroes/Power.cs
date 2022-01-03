using Global;
using UnityEngine;

public abstract class Power : ScriptableObject {

	//public  string name;
	public float castingTime;
	public int manaCost;
	public int numberTarget;
	public string description;
	public float range;
    

	public void usePower (Creature caster, Creature target) {
		Creature[] targets = {target};
		usePower(caster,targets);
	}
	public virtual void usePower(Creature caster, Creature[] targets) {
		
	}



    
}
