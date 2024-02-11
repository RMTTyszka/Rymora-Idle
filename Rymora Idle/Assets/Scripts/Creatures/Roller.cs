using System.Collections.Generic;
using System.Linq;
using Heroes;
using UnityEngine;

public abstract class Roller
{
    protected Roller(string name, int points, int modifier, float difficult)
    {
        Name = name;
        Points = points;
        Modifier = modifier;
        Difficult = difficult;
    }
    public string Name { get; set; }
    private float Difficult { get; set; }
    private int Count { get; set; } = 0;
    private int Points { get; set; }
    private int Modifier { get; set; }
    private List<Bonus> Bonuses { get; set; } = new();
    
    public void AddBonus(Bonus bonus) {
        Bonuses.Add(bonus);
    }
    public void RemoveBonus(Bonus bonus) {
        Bonuses.Remove(bonus);
    }
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
    public virtual int GetValue (int lvl)
    {
        RollForRaise(lvl);
        return GetValue();
    }
    public virtual int Roll(int lvl)
    {
        RollForRaise(lvl);
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
    private void RollForRaise(int lvl) {
        if (lvl > 0 && lvl >= Points/5) {
            float chance = 1000f/((Points/5f)*Difficult);
            float roll = Random.Range(0f, 10001f);
            // Debug.Log(roll + " " + chance);
            if (roll <= chance) {
                Points++;
                // owner. UpdateStats();
                Debug.Log(Name + " aumentou, agora é " + Points);
                chance = (1000f/((Points/5f)*Difficult))/10000f*100;
            }
        }
    }

}