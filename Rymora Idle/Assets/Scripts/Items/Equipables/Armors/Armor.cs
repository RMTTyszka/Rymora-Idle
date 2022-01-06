using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Items.Equipables.Weapons;
using Random = UnityEngine.Random;

public class Armor : Equipable {

	public enum ArmorSize {Heavy, Medium, Light};
	public enum ArmorStats {Protection, Evasion};
	public enum ArmorBases {None, Cloth, Robe, Leather, Chainmail, StuddedLeather, Halfplate, Fullplate, Scale, Splitmail};

	public static Dictionary<ArmorSize, Dictionary<ArmorStats, int>> ArmorData = new Dictionary<ArmorSize, Dictionary<ArmorStats, int>>() {
		//{ArmorCat.None, new Dictionary<ArmorStats, int>() {
			//{ArmorStats.Protection, 1},
			//{ArmorStats.Evasion, 5}
		//}},
		{ArmorSize.Light, new Dictionary<ArmorStats, int>() {
			{ArmorStats.Protection, 3},
			{ArmorStats.Evasion, 0}
		}},
		{ArmorSize.Medium, new Dictionary<ArmorStats, int>() {
			{ArmorStats.Protection, 5},
			{ArmorStats.Evasion, 0}
		}},
		{ArmorSize.Heavy, new Dictionary<ArmorStats, int>() {
			{ArmorStats.Protection, 14},
			{ArmorStats.Evasion, 0}
		}},
	};

	public static ArmorBases FromSize(ArmorSize size)
	{
		var list = ArmorList[size].Keys.ToList();
		var armor = list[Random.Range(0, list.Count())];
		return armor;
	}

	public static Dictionary<ArmorSize, Dictionary<ArmorBases, Dictionary<string, Weapon.WeaponsDamageType>>> ArmorList = new Dictionary<ArmorSize, Dictionary<ArmorBases, Dictionary<string, Weapon.WeaponsDamageType>>>() {
		//{ArmorCat.None, new Dictionary<ArmorBases, Dictionary<string, Weapon.WeaponsDamageType>>(){
			//{ArmorBases.None, new Dictionary<string, Weapon.WeaponsDamageType>() {
				//{"weak", Weapon.WeaponsDamageType.None},
					//	{"strong", Weapon.WeaponsDamageType.None}}}}},
		{ArmorSize.Light, new Dictionary<ArmorBases, Dictionary<string, Weapon.WeaponsDamageType>>(){
				{ArmorBases.Cloth, new Dictionary<string, Weapon.WeaponsDamageType>() {
					{"weak", Weapon.WeaponsDamageType.Smashing},
					{"strong", Weapon.WeaponsDamageType.Piercing}}},
				{ArmorBases.Leather, new Dictionary<string, Weapon.WeaponsDamageType>() {
					{"weak", Weapon.WeaponsDamageType.Piercing},
					{"strong", Weapon.WeaponsDamageType.Cutting}}},
				{ArmorBases.Robe, new Dictionary<string, Weapon.WeaponsDamageType>() {
					{"weak", Weapon.WeaponsDamageType.Cutting},
					{"strong", Weapon.WeaponsDamageType.Smashing}}}}},
		{ArmorSize.Medium, new Dictionary<ArmorBases, Dictionary<string, Weapon.WeaponsDamageType>>(){
				{ArmorBases.Chainmail, new Dictionary<string, Weapon.WeaponsDamageType>() {
					{"weak", Weapon.WeaponsDamageType.Smashing},
					{"strong", Weapon.WeaponsDamageType.Piercing}}},
				{ArmorBases.StuddedLeather, new Dictionary<string, Weapon.WeaponsDamageType>() {
					{"weak", Weapon.WeaponsDamageType.Cutting},
					{"strong", Weapon.WeaponsDamageType.Smashing}}},
				{ArmorBases.Halfplate, new Dictionary<string, Weapon.WeaponsDamageType>() {
					{"weak", Weapon.WeaponsDamageType.Piercing},
					{"strong", Weapon.WeaponsDamageType.Cutting}}}}},
		{ArmorSize.Heavy, new Dictionary<ArmorBases, Dictionary<string, Weapon.WeaponsDamageType>>(){
				{ArmorBases.Fullplate, new Dictionary<string, Weapon.WeaponsDamageType>() {
					{"weak", Weapon.WeaponsDamageType.Smashing},
					{"strong", Weapon.WeaponsDamageType.Piercing}}},
				{ArmorBases.Scale, new Dictionary<string, Weapon.WeaponsDamageType>() {
					{"weak", Weapon.WeaponsDamageType.Cutting},
					{"strong", Weapon.WeaponsDamageType.Smashing}}},
				{ArmorBases.Splitmail, new Dictionary<string, Weapon.WeaponsDamageType>() {
					{"weak", Weapon.WeaponsDamageType.Piercing},
					{"strong", Weapon.WeaponsDamageType.Cutting}}}}}
		
	};

	public ArmorBases BaseArmor { get; set; }
	public ArmorSize Size { get; set; }
	public int Protection { get; set; }
	public int Evasion { get; set; }

	public Armor(){
        Slot = Slot.Chest;
		getArmorStats(BaseArmor, Level);

	}

	public static Armor FromBaseArmor(ArmorBases armorBase, int level)
	{
		var armor = new Armor();
		armor.Level = level;
		switch (armorBase)
		{
			case ArmorBases.None:
				armor = PopulateSizeProperties(armor, ArmorSize.Light);
				break;
			case ArmorBases.Cloth:
				armor = PopulateSizeProperties(armor, ArmorSize.Light);
				break;
			case ArmorBases.Robe:
				armor = PopulateSizeProperties(armor, ArmorSize.Light);
				break;
			case ArmorBases.Leather:
				armor = PopulateSizeProperties(armor, ArmorSize.Light);
				break;
			case ArmorBases.Chainmail:
				armor = PopulateSizeProperties(armor, ArmorSize.Medium);
				break;
			case ArmorBases.StuddedLeather:
				armor = PopulateSizeProperties(armor, ArmorSize.Medium);
				break;
			case ArmorBases.Halfplate:
				armor = PopulateSizeProperties(armor, ArmorSize.Medium);
				break;
			case ArmorBases.Fullplate:
				armor = PopulateSizeProperties(armor, ArmorSize.Heavy);
				break;
			case ArmorBases.Scale:
				armor = PopulateSizeProperties(armor, ArmorSize.Heavy);
				break;
			case ArmorBases.Splitmail:
				armor = PopulateSizeProperties(armor, ArmorSize.Heavy);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(armorBase), armorBase, null);
		}

		return armor;
	}

	private static Armor PopulateSizeProperties(Armor armor, ArmorSize size)
	{
		switch (size)
		{
			case ArmorSize.Heavy:
				armor.Size = ArmorSize.Heavy;
				armor.Protection = 14;
				armor.Evasion = 0;
				break;
			case ArmorSize.Medium:
				armor.Size = ArmorSize.Medium;
				armor.Protection = 5;
				armor.Evasion = 0;
				break;
			case ArmorSize.Light:
				armor.Size = ArmorSize.Light;
				armor.Protection = 3;
				armor.Evasion = 0;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(size), size, null);
		}

		return armor;
	}

	public void getArmorStats(ArmorBases baseArmor, int lvl) {
		this.BaseArmor = baseArmor;
		this.Level = lvl;
		foreach (KeyValuePair<ArmorSize, Dictionary<ArmorBases, Dictionary<string, Weapon.WeaponsDamageType>>> cat in ArmorList ){
			foreach(KeyValuePair<ArmorBases, Dictionary<string, Weapon.WeaponsDamageType>> armorBase in cat.Value ){
				if ( baseArmor == armorBase.Key) {
					Size = cat.Key;
				}
			}
		}
		Protection = ArmorData[Size][ArmorStats.Protection] * lvl;
		Evasion = ArmorData[Size][ArmorStats.Evasion];
	}

	public static Armor GetArmorByCat(ArmorSize size, int lvl) {
		Armor armor = new Armor();
		armor.Size = size;
		armor.BaseArmor = ArmorList[size].Keys.ElementAt((int)UnityEngine.Random.Range(0, ArmorList[size].Count-1));
		armor.getArmorStats(armor.BaseArmor, lvl);

		return armor;
	}
}
