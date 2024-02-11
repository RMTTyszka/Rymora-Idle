using System;
using Heroes;

public class Skills
{
    public SkillInstance Alchemy { get; set; } = new SkillInstance(Skill.Alchemy.ToString());
    public SkillInstance Anatomy { get; set; } = new SkillInstance(Skill.Anatomy.ToString());
    public SkillInstance AnimalTaming { get; set; } = new SkillInstance(Skill.AnimalTaming.ToString());
    public SkillInstance Archery { get; set; } = new SkillInstance(Skill.Archery.ToString());
    public SkillInstance Armorcrafting { get; set; } = new SkillInstance(Skill.Armorcrafting.ToString());
    public SkillInstance Armslore { get; set; } = new SkillInstance(Skill.Armslore.ToString());
    public SkillInstance Awareness { get; set; } = new SkillInstance(Skill.Awareness.ToString());
    public SkillInstance Bowcrafting { get; set; } = new SkillInstance(Skill.Bowcrafting.ToString());
    public SkillInstance Carpentery { get; set; } = new SkillInstance(Skill.Carpentery.ToString());
    public SkillInstance Fencing { get; set; } = new SkillInstance(Skill.Fencing.ToString());
    public SkillInstance Gathering { get; set; } = new SkillInstance(Skill.Gathering.ToString());
    public SkillInstance Healing { get; set; } = new SkillInstance(Skill.Healing.ToString());
    public SkillInstance Heavyweaponship { get; set; } = new SkillInstance(Skill.Heavyweaponship.ToString());
    public SkillInstance Jewelcrafting { get; set; } = new SkillInstance(Skill.Jewelcrafting.ToString());
    public SkillInstance Leatherworking { get; set; } = new SkillInstance(Skill.Leatherworking.ToString());
    public SkillInstance Lore { get; set; } = new SkillInstance(Skill.Lore.ToString());
    public SkillInstance Lumberjacking { get; set; } = new SkillInstance(Skill.Lumberjacking.ToString());
    public SkillInstance Magery { get; set; } = new SkillInstance(Skill.Magery.ToString());
    public SkillInstance Mercantilism { get; set; } = new SkillInstance(Skill.Mercantilism.ToString());
    public SkillInstance Military { get; set; } = new SkillInstance(Skill.Military.ToString());
    public SkillInstance Mining { get; set; } = new SkillInstance(Skill.Mining.ToString());
    public SkillInstance Parry { get; set; } = new SkillInstance(Skill.Parry.ToString());
    public SkillInstance Reflex { get; set; } = new SkillInstance(Skill.Reflex.ToString());
    public SkillInstance ResistSpells { get; set; } = new SkillInstance(Skill.ResistSpells.ToString());
    public SkillInstance Skinning { get; set; } = new SkillInstance(Skill.Skinning.ToString());
    public SkillInstance Stealth { get; set; } = new SkillInstance(Skill.Stealth.ToString());
    public SkillInstance Swordmanship { get; set; } = new SkillInstance(Skill.Swordmanship.ToString());
    public SkillInstance SpiritSpeaking { get; set; } = new SkillInstance(Skill.SpiritSpeaking.ToString());
    public SkillInstance Tactics { get; set; } = new SkillInstance(Skill.Tactics.ToString());
    public SkillInstance Tailoring { get; set; } = new SkillInstance(Skill.Tailoring.ToString());
    public SkillInstance Wrestling { get; set; } = new SkillInstance(Skill.Wrestling.ToString());

    public Skills()
    {
    }

    public SkillInstance Get(Skill skill)
    {
        return GetType().GetProperty(skill.ToString())!.GetValue(this) as SkillInstance;
    }

    public static decimal MineTime = 3;
    public static decimal CutWoodTime = 2;
}