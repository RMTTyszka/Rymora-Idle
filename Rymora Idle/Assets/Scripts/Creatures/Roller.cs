using System.Collections.Generic;
using System.Linq;
using Heroes;
using UnityEngine;

public abstract class Roller
{
    protected Roller(string name, int points, int modifier)
    {
        Name = name;
        Points = points;
        Modifier = modifier;
    }

    protected string Name { get; set; }
    protected int Points { get; set; }
    protected int Modifier { get; set; }
    protected List<Bonus> Bonuses { get; set; } = new();
    public int TotalValue()
    {
        return Points + GetTotalBonus();
    }

    protected int GetTotalBonus()
    {
        var innate = Bonuses.Where(bonus => bonus.Type == BonusType.Innate).Sum(bonus => bonus.Value);
        var magic = Bonuses.Where(bonus => bonus.Type == BonusType.Magic).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
        var equipment = Bonuses.Where(bonus => bonus.Type == BonusType.Equipment).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
        var food = Bonuses.Where(bonus => bonus.Type == BonusType.Food).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
        var furniture = Bonuses.Where(bonus => bonus.Type == BonusType.Furniture).Select(bonus => bonus.Value).DefaultIfEmpty().Max();
        return innate + magic + equipment + food + furniture;
    }

    public int Roll()
    {
        int sum = 0;
        for (int x = 0; x < GetValue(); x++) {
            sum += Random.Range(0,2);
        } 
        return sum;
    }
    protected virtual int GetTotalPoints() {
        return Points + GetTotalBonus();
    }
    protected virtual int GetValue() {
        return GetTotalPoints() / Modifier;
    }
    public void AddBonus(Bonus bonus) {
        Bonuses.Add(bonus);
    }
    public void RemoveBonus(Bonus bonus) {
        Bonuses.Remove(bonus);
    }
}