using UnityEngine;

[System.Serializable]
public class Skill : Raiser {
	private const int RaiseDifficult = 5;
	private const int Modifier = 5;
	private const int BaseValue = 5;



	public Skill() : base(RaiseDifficult, BaseValue, Modifier) {
	}

}
