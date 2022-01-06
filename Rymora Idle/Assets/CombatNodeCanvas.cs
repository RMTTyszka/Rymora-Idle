using System.Collections;
using System.Collections.Generic;
using Global;
using UnityEngine;
using UnityEngine.UI;

public class CombatNodeCanvas : MonoBehaviour
{
    public Transform attackBar;
    public Transform castingBar;
    private Slider attackSlider;
    private Slider castingSlider;
    public Slider lifeSlider;
    public Slider spiritSlider;
    private Creature combatChar => CreatureSpawner.Creature;
    public CombatManager combatManager;

    public CreatureSpawner CreatureSpawner { get; set; }

    // Use this for initialization
    void Start () {
        CreatureSpawner = transform.GetComponentInParent<CreatureSpawner>();
        attackSlider = attackBar.GetComponent<Slider>();
        castingSlider = castingBar.GetComponent<Slider>();
    }
	
    // Update is called once per frame
    void Update () {
        if (combatManager.CombatStarted) {
            attackBar.gameObject.SetActive(true); 
            castingBar.gameObject.SetActive(true); 
            attackSlider.maxValue = combatChar.Equipment.MainWeapon.AttackSpeed;
            attackSlider.value = attackSlider.maxValue - combatChar.Equipment.MainWeaponAttackCooldown;

/*            if (combatChar.chargedPower != null) {
                castingSlider.maxValue = combatChar.chargedPower.castingTime;
                castingSlider.value = castingSlider.maxValue - combatChar.castingTimer;
            } else {
                castingSlider.value = 0;
            }*/
        } else {
            attackBar.gameObject.SetActive(false);
            castingBar.gameObject.SetActive(false); 
			
        }

        lifeSlider.value = combatChar.LifePercent / 100f;
    }
}
