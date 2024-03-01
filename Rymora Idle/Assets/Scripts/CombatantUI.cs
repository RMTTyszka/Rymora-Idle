using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CombatantUI : MonoBehaviour {
	public Transform attackBar;
	public Transform castingBar;
	public Transform lifeBar;
	public Transform spiritsBar;
	private  Slider attackSlider;
	private  Slider lifeSlider;
	private  Slider spiritSlider;
	private  Slider castingSlider;
	public Creature combatant;
	public EffectManager effectManager;
	[FormerlySerializedAs("CBTpref")] public GameObject cBTpref;
	// Use this for initialization
	void Start () {
		attackSlider = attackBar.GetComponent<Slider>();
		lifeSlider = lifeBar.GetComponent<Slider>();
		spiritSlider = spiritsBar.GetComponent<Slider>();
	}

	public void SetCreature(Creature creature)
	{
		combatant = creature;
		effectManager.Creature = creature;
	}

	public void InitCBT(string text, CbtTriggerType trigger, bool isCounter = false) {
		GameObject CBT = Instantiate(cBTpref);
		RectTransform CBTRect = CBT.GetComponent<RectTransform>();
		CBT.transform.SetParent(transform);
		CBTRect.transform.localPosition = cBTpref.transform.localPosition;
		//CBTRect.transform.localScale = CBTpref.transform.localScale;
		CBTRect.transform.localRotation = cBTpref.transform.localRotation;
		CBT.GetComponent<Text>().text = text;

		CBT.GetComponent<Animator>().SetTrigger(trigger.ToString());
		if (isCounter) {
			CBT.GetComponent<Text>().color = new Color(0.2f, 0.3f, 0.4f);
		}
		Destroy(CBT, 4f);
	}
	// Update is called once per frame
	void Update () {
		/*attackBar.gameObject.SetActive(true); 
		castingBar.gameObject.SetActive(true); */
		attackSlider.maxValue = combatant.Equipment.MainHand.Status().AttackSpeed;
		attackSlider.value = attackSlider.maxValue - combatant.Combatant.MainWeaponCooldown;
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
