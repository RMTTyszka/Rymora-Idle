using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharCanvas : MonoBehaviour {
	public Transform attackBar;
	public Transform castingBar;
	public Transform lifeBar;
	public Transform spiritsBar;
	private  Slider attackSlider;
	private  Slider lifeSlider;
	private  Slider spiritSlider;
	private  Slider castingSlider;
	public Combatant combatant;
	public EffectManager effectManager;
	// Use this for initialization
	void Start () {
		attackSlider = attackBar.GetComponent<Slider>();
		lifeSlider = lifeBar.GetComponent<Slider>();
		spiritSlider = spiritsBar.GetComponent<Slider>();
	}

	public void SetCreature(Creature creature)
	{
		combatant = creature.Combatant;
		effectManager.Creature = creature;
	}

	// Update is called once per frame
	void Update () {
		/*attackBar.gameObject.SetActive(true); 
		castingBar.gameObject.SetActive(true); */
		attackSlider.maxValue = combatant.Creature.Equipment.MainHand.Status().AttackSpeed;
		attackSlider.value = attackSlider.maxValue - combatant.MainWeaponCooldown;
			/*
			if (combatant.chargedPower != null) {
				castingSlider.maxValue = combatant.chargedPower.castingTime;
				castingSlider.value = castingSlider.maxValue - combatant.castingTimer;
			} else {
				castingSlider.value = 0;
			}*/
		lifeSlider.maxValue = combatant.MaxLife;
		lifeSlider.value = combatant.Life;
	}
}
