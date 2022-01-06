using System.Collections.Generic;
using System.Linq;
using Global;
using Heroes;

public abstract class Equipable : Item
{
    public Slot Slot;
    public List<Bonus> Bonuses;

    public Equipable()
    {
	    Bonuses = new List<Bonus>();
    }

    public void Equip(Creature target) {
		
    }
    public void Unequip(Creature target) {
		
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
}
