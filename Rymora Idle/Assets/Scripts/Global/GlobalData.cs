using Global;

public enum AttributeType{
						Strength, 
						Agility, 
						Vitality, 
						Wisdom, 
						Intuition, 
						Charisma
						};
public enum SkillType {
						Alchemy, 
						Anatomy, 
						AnimalTaming, 
						Archery, 
						Armorcrafting, 
						Armslore,
						Awareness,
						Bowcrafting,
						Carpentery,
						Fencing,
						Gathering,
						Healing,
						Heavyweaponship,
						Jewelcrafting,
						Leatherworking,
						Lore,
						Lumberjacking,
						Magery,
						Mercantilism,
						Military,
						Mining,
						Parry,
						Reflex,
						ResistSpells,
						Skinning,
						Stealth,
						Swordmanship,
						SpiritSpeaking,
						Tactics,
						Tailoring,
						Wrestling
						};

public enum Slot {
                        None,
						Mainhand,
						Offhand, 
						Head, Neck, 
						Chest, Wrist, 
						Hand, FingerLeft, 
						FingerRight, Waist, 
						Feet, 
						Extra
						};
public enum Properties {
						Critical,
						Resiliense, 
						Attack, PowerAttack, 
						Evasion, PowerDefense,SpiritPoints, 
						Life, ValiantPoints, 
						Protection, Fortitude, 
						AttackDamage, PowerDamage, AttackSpeed, CastingSpeed,  
						CriticalDamage, ArmorPen, Reaction, Counter, Threat
						};
public enum MyEvents {
						Attack,
						Evade, 
						Critical, Counter, TakeDamage, 
						Damage, Cast, 
						Death};
public enum BonusList {
    Strength,
    Agility,
    Vitality,
    Wisdom,
    Intuition,
    Charisma,

    Alchemy,
    Anatomy,
    AnimalTaming,
    Archery,
    Armorcrafting,
    Armslore,
    Awareness,
    Bowcrafting,
    Carpentery,
    Fencing,
    Gathering,
    Healing,
    Heavyweaponship,
    Jewelcrafting,
    Leatherworking,
    Lore,
    Lumberjacking,
    Magery,
    Mercantilism,
    Military,
    Mining,
    Parry,
    Reflex,
    ResistSpells,
    Skinning,
    Stealth,
    Swordmanship,
    SpiritSpeaking,
    Tactics,
    Tailoring,
    Wrestling,

    Critical,
    Resiliense,
    Attack, PowerAttack,
    Evasion, PowerDefense, SpiritPoints,
    Life, ValiantPoints,
    Protection, Fortitude,
    AttackDamage, PowerDamage, AttackSpeed, CastingSpeed,
    CriticalDamage, ArmorPen, Reaction, Counter, Threat
};

public class GlobalData {

	public static string[] Stats = {"life", "spiritPoints", "stamina"};

	public static string[] attributes = {"strength", "agility", 
									"vitality", "wisdom", 
									"intuition", "charisma"};
	public static string[] skills = {"alchemy", "anatomy", "animaltaming", "archery", "armorcrafting", "armslore",
    "awareness", "bowcrafting", "carpentery", "fencing", "gathering", "healing",
    "heavyweaponship", "jewelcrafting", "leatherworking", "lore", "lumberjacking",
    "magery", "mercantilism", "military", "mining", "parry", "reflex", "resist spells",
    "skinning", "stealth", "swordmanship", "spirit speaking", "tactics", "tailoring", "wrestling"};

	public static string[] Resists = {"weakness", "slow", "stun", "blind", "curse"};

	public static string[] Defenses = {"cutting", "smashing", "piercing", "magic"};

	public static string[] Slots = {"mainhand", "offhand", "head", "neck", "chest", "wrist",
    "hand", "finger1", "finger2", "waist", "feet", "extra"};

	public static string[] Properties = {"critical","resilience","attack","powerAttack",
                "evasion","sp", "life","st","protection","fortitude","attackDamage",
                "powerDamage","attackSpeed","cs","criticalDamage","armorPen",
                "reaction","counter"};

 	public static string background = "footMontain";

	public static int GetDifLvl(Creature main, Creature target) {
		return target.Level - main.Level;
	}
}

