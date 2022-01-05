using System;

namespace Global
{
    public class Skills
    {
        public Skill Alchemy { get; set; }
        public Skill Anatomy { get; set; }
        public Skill AnimalTaming { get; set; }
        public Skill Archery { get; set; }
        public Skill Armorcrafting { get; set; }
        public Skill Armslore { get; set; }
        public Skill Awareness { get; set; }
        public Skill Bowcrafting { get; set; }
        public Skill Carpentery { get; set; }
        public Skill Fencing { get; set; }
        public Skill Gathering { get; set; }
        public Skill Healing { get; set; }
        public Skill Heavyweaponship { get; set; }
        public Skill Jewelcrafting { get; set; }
        public Skill Leatherworking { get; set; }
        public Skill Lore { get; set; }
        public Skill Lumberjacking { get; set; }
        public Skill Magery { get; set; }
        public Skill Mercantilism { get; set; }
        public Skill Military { get; set; }
        public Skill Mining { get; set; }
        public Skill Parry { get; set; }
        public Skill Reflex { get; set; }
        public Skill ResistSpells { get; set; }
        public Skill Skinning { get; set; }
        public Skill Stealth { get; set; }
        public Skill Swordmanship { get; set; }
        public Skill SpiritSpeaking { get; set; }
        public Skill Tactics { get; set; }
        public Skill Tailoring { get; set; }
        public Skill Wrestling { get; set; }

        public Skills()
        {
            Alchemy  = new Skill();
            Anatomy  = new Skill();
            AnimalTaming  = new Skill();
            Archery  = new Skill();
            Armorcrafting  = new Skill();
            Armslore  = new Skill();
            Awareness  = new Skill();
            Bowcrafting  = new Skill();
            Carpentery  = new Skill();
            Fencing  = new Skill();
            Gathering  = new Skill();
            Healing  = new Skill();
            Heavyweaponship  = new Skill();
            Jewelcrafting  = new Skill();
            Leatherworking  = new Skill();
            Lore  = new Skill();
            Lumberjacking  = new Skill();
            Magery  = new Skill();
            Mercantilism  = new Skill();
            Military  = new Skill();
            Mining  = new Skill();
            Parry  = new Skill();
            Reflex  = new Skill();
            ResistSpells  = new Skill();
            Skinning  = new Skill();
            Stealth  = new Skill();
            Swordmanship  = new Skill();
            SpiritSpeaking  = new Skill();
            Tactics  = new Skill();
            Tailoring  = new Skill();
            Wrestling  = new Skill();

        }

        public int RollForModifier(SkillType skill, int targetLevel)
        {
            switch (skill)
            {
                case SkillType.Alchemy:
                    return Alchemy.RollForModifier(targetLevel);
                case SkillType.Anatomy:
                    return Anatomy.RollForModifier(targetLevel);
                case SkillType.AnimalTaming:
                    return AnimalTaming.RollForModifier(targetLevel);
                case SkillType.Archery:
                    return Archery.RollForModifier(targetLevel);
                case SkillType.Armorcrafting:
                    return Armorcrafting.RollForModifier(targetLevel);
                case SkillType.Armslore:
                    return Armslore.RollForModifier(targetLevel);
                case SkillType.Awareness:
                    return Awareness.RollForModifier(targetLevel);
                case SkillType.Bowcrafting:
                    return Bowcrafting.RollForModifier(targetLevel);
                case SkillType.Carpentery:
                    return Carpentery.RollForModifier(targetLevel);
                case SkillType.Fencing:
                    return Fencing.RollForModifier(targetLevel);
                case SkillType.Gathering:
                    return Gathering.RollForModifier(targetLevel);
                case SkillType.Healing:
                    return Healing.RollForModifier(targetLevel);
                case SkillType.Heavyweaponship:
                    return Heavyweaponship.RollForModifier(targetLevel);
                case SkillType.Jewelcrafting:
                    return Jewelcrafting.RollForModifier(targetLevel);
                case SkillType.Leatherworking:
                    return Leatherworking.RollForModifier(targetLevel);
                case SkillType.Lore:
                    return Lore.RollForModifier(targetLevel);
                case SkillType.Lumberjacking:
                    return Lumberjacking.RollForModifier(targetLevel);
                case SkillType.Magery:
                    return Magery.RollForModifier(targetLevel);
                case SkillType.Mercantilism:
                    return Mercantilism.RollForModifier(targetLevel);
                case SkillType.Military:
                    return Military.RollForModifier(targetLevel);
                case SkillType.Mining:
                    return Mining.RollForModifier(targetLevel);
                case SkillType.Parry:
                    return Parry.RollForModifier(targetLevel);
                case SkillType.Reflex:
                    return Reflex.RollForModifier(targetLevel);
                case SkillType.ResistSpells:
                    return ResistSpells.RollForModifier(targetLevel);
                case SkillType.Skinning:
                    return Skinning.RollForModifier(targetLevel);
                case SkillType.Stealth:
                    return Stealth.RollForModifier(targetLevel);
                case SkillType.Swordmanship:
                    return Swordmanship.RollForModifier(targetLevel);
                case SkillType.SpiritSpeaking:
                    return SpiritSpeaking.RollForModifier(targetLevel);
                case SkillType.Tactics:
                    return Tactics.RollForModifier(targetLevel);
                case SkillType.Tailoring:
                    return Tailoring.RollForModifier(targetLevel);
                case SkillType.Wrestling:
                    return Wrestling.RollForModifier(targetLevel);
                default:
                    throw new ArgumentOutOfRangeException(nameof(skill), skill, null);
            }
        }
    }
}