using System.Collections.Generic;
using System.Linq;

public class Combatant
{
    public Creature Creature { get; set; }
    public int Life { get; set; }
    
    public float MainWeaponCooldown { get; set; }
    public float SecondaryWeaponCooldown { get; set; }
    public int MaxLife 
    {
        get 
        {
            int baseLife = 500;
            int vitLife = Creature.Attributes.Get(Attribute.Vitality).GetValue(0) * 10;
            return baseLife + vitLife;
        }
    }

    public float LifePercent => ((float)Life)/((float)MaxLife)*100;
    
    public Creature GetTargetForAutoAttack(List<Creature> enemies, List<Creature> allies, TargetType targetType)
    {
        // Get target from a list of possible targets, ahd pick the one with the highest aggro
        var targets = targetType == TargetType.Enemy ? enemies : allies;
        if (targets == null) {
            return null;
        }
        var target = targets.OrderByDescending(t => t.Combatant.GetAggro()).First();
        return target;
    }
    public float GetAggro() {
        // Less health == More Aggro, and bonus
        return ((1f-LifePercent)*10 + Creature.Properties.Get(Property.Threat).GetValue(0));
    }
}

public enum TargetType
{
    Enemy = 0,
    Ally = 1,
    Self = 2
}