using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Itens/Weapon")]
public class WeaponTemplate : ItemTemplate
{
    public WeaponSize size;
    public WeaponsDamageCategory damageCategory;
}
