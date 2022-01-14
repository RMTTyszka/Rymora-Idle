using System;
using System.Collections;
using System.Collections.Generic;
using Global;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Powers/Heal")]
public class Heal : Power {

    public int baseHeal;
    public float healMult;

    public override List<CombatActionResult> Cast(Creature caster, Creature[] targets)
    {
        var actions = new List<CombatActionResult>();
        int lvl = targets[0].Level;
        float healAmount =  caster.Attributes.RollForModifier(AttributeType.Wisdom, lvl);
        healAmount += caster.Skills.RollForModifier(SkillType.Healing, lvl);
        healAmount += caster.Skills.RollForModifier(SkillType.Magery, lvl);
        healAmount *= healMult;
        healAmount += Random.Range(baseHeal/2f, baseHeal*1.5f);
        healAmount = (float) Math.Round(healAmount);

        targets[0].TakeHealing(healAmount);
        var healAction = new CombatActionResult
        {
            Value = healAmount,
            Target = targets[0],
            Performer = caster,
            ActionType = CombatActionType.Heal
        };
        actions.Add(healAction);
        return actions;
        //base.usePower (caster, targets);
    }
}
