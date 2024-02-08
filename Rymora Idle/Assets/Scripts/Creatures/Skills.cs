using System;
using Heroes;

public class Skills
{
    public SkillInstance Mining { get; set; } = new SkillInstance(Skill.Mining.ToString());
    public SkillInstance Lumberjacking { get; set; } = new SkillInstance(Skill.Lumberjacking.ToString());

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