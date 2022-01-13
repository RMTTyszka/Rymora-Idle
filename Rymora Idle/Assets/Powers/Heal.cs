using System.Collections;
using System.Collections.Generic;
using Global;
using UnityEngine;
[CreateAssetMenu(menuName = "Powers/Heal")]
public class Heal : Power {

    public int baseHeal;
    public float healMult;

    public override void usePower (Creature caster, Creature[] targets)
    {
        int lvl = targets[0].Level;
        float healAmount =  caster.Attributes.RollForModifier(AttributeType.Wisdom, lvl);
        healAmount += caster.Skills.RollForModifier(SkillType.Healing, lvl);
        healAmount += caster.Skills.RollForModifier(SkillType.Magery, lvl);
        healAmount *= healMult;
        healAmount += Random.Range(baseHeal/2f, baseHeal*1.5f);

        targets[0].TakeHealing(healAmount);
        //base.usePower (caster, targets);
    }
}
