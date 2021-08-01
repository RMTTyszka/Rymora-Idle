using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heroes
{
    public class Skill
    {
        public Skill()
        {
            Bonuses = new List<Bonus>();
        }
        public int Value { get; set; }
        public List<Bonus> Bonuses { get; set; }

        public int TotalValue()
        {
            return Value + GetTotalBonus();
        }

        public int GetTotalBonus()
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
            var rollValue = Random.Range(1, 101);
            return rollValue + GetTotalBonus();
        }
    }
}