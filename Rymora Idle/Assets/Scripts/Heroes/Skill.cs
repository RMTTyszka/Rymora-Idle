using UnityEngine;

[System.Serializable]
public class Skill : Raiser {
	private const int RaiseDifficult = 5;
	private const int Modifier = 5;
	private const int BaseValue = 5;

	public int Roll()
	{
		var rollValue = Random.Range(1, 101);
		return rollValue + GetTotalBonus();
	}

	public Skill() : base(RaiseDifficult, BaseValue, Modifier) {
	}

}
