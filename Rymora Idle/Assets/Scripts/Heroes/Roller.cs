using System.Collections.Generic;
using System.Linq;
using Heroes;
using UnityEngine;

public class Roller{
	protected int Points { get; set; }
	private int TotalBonus { get; set; }
	public List<Bonus> Bonuses { get; set; }
	private int Modifier { get; set; }

	public Roller(int val, int modifier) {
		Points = val;
		Modifier = modifier;
		Bonuses = new List<Bonus>();
		TotalBonus = GetTotalBonus();
	}
	public virtual int RollForModifier() {
		//Debug.Log("roller roll called");
		int sum = 0;
		for (int x = 0; x < Value; x++) {
			sum += Random.Range(0,2);
		} 
		return sum;
	}
	public int Roll()
	{
		var rollValue = Random.Range(1, 101);
		return rollValue + TotalPoints;
	}
	public int GetTotalBonus()
	{
		var innate = Bonuses.Where(bonus => bonus.Type == BonusType.Innate).Sum(bonus => bonus.Value);
		var magic = Bonuses.Where(bonus => bonus.Type == BonusType.Magic).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
		var equipment = Bonuses.Where(bonus => bonus.Type == BonusType.Equipment).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
		var food = Bonuses.Where(bonus => bonus.Type == BonusType.Consumable).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
		var furniture = Bonuses.Where(bonus => bonus.Type == BonusType.Furniture).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
		return innate + magic + equipment + food + furniture;
	}

	public virtual int TotalPoints => Points + TotalBonus;
	public virtual int Value => TotalPoints / Modifier;
	//public virtual int GetMod(int newMod) {
		//return GetValue()/newMod;
	//}

	public void AddBonus(Bonus bonus) {
		Bonuses.Add(bonus);
		TotalBonus = GetTotalBonus();
	}
	public void RemoveBonus(Bonus bonus) {
		Bonuses.Remove(bonus);
		TotalBonus = GetTotalBonus();
	}

}
