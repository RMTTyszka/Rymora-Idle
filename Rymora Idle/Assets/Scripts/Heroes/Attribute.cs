[System.Serializable]
public class Attribute: Raiser {
	private const int RaiseDifficulty = 10;
	private const int Modifier = 5;
	private const int BaseValue = 5;
	public Attribute() : base(RaiseDifficulty, BaseValue, Modifier) {
	}	
	public Attribute(int baseValue) : base(RaiseDifficulty, baseValue, Modifier) {
		
	}

	
}
