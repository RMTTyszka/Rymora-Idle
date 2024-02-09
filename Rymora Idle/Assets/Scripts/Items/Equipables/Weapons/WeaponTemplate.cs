using Items.Equipables.Weapons;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Itens/Weapon")]
public class WeaponTemplate : ScriptableObject
{
    public WeaponSize size;
    public WeaponsDamageCategory damageCategory;
}