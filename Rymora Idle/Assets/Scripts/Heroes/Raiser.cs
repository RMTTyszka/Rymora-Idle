using UnityEngine;

public class Raiser : Roller {

	
	private int RaiseDifficult { get; set; }


	public Raiser(int diff, int baseVal, int modifier) : base(baseVal, modifier) {
		RaiseDifficult = diff;
	}
	public int RollForModifier(int lvl) {
		//Debug.Log("Raiser roll called");
		RollForRaise(lvl);
		return base.RollForModifier();
	}	
	public int Roll(int lvl) {
		//Debug.Log("Raiser roll called");
		RollForRaise(lvl);
		return base.Roll();
	}

	private void RollForRaise(int lvl) {
		if ( lvl >= Points/5) {
			var chance = 1000/((Points / 5) * RaiseDifficult);
			var roll = Random.Range(0, 10001);
			//Debug.Log(roll + " " + chance);
			if (roll <= chance) {
				//value++;
		//		owner.UpdateStats();
				Debug.Log(GetType() + " aumentou, agora é " + Points);
				//chance = (1000f/((value/5f)*difficult))/10000f*100;
				//Debug.Log(chance);
			}
		}
	}
}
