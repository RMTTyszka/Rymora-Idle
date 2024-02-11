using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public int encounterProbability = 20;
    public int encounterInterval = 3;
    public static List<WeaponTemplate> UnarmedWeapons;
    public static List<WeaponTemplate> Weapons;
    public static List<ArmorTemplate> Armors { get; set; }

    private void Start()
    {
        Armors = Resources.LoadAll<ArmorTemplate>("Itens/Armors").ToList();
        Weapons = Resources.LoadAll<WeaponTemplate>("Itens/Weapons").ToList();
        UnarmedWeapons = Weapons.Where(w => w.damageCategory == WeaponsDamageCategory.None).ToList();

    }
}
