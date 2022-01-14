using System;
using System.Collections.Generic;
using System.Linq;
using Global;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CombatActionComponent : MonoBehaviour
{
    public Dropdown targetSelect;
    public Dropdown checkerSelect;
    public Dropdown conditionSelect;
    public Dropdown valueSelect;
    public Dropdown powerSelect;
    public HeroTactics HeroTactics { get; set; }
    public int Index { get; set; }
    public CombatAction Action { get; set; }
    public List<Power> Powers { get; set; }

    public CombatActionComponent()
    {
        Powers = new List<Power>();
    }

    private void Start()
    {
        HeroTactics = GetComponentInParent<HeroTactics>();
        
        Action = Action ?? new CombatAction();
        Action.power = Action.power ? Action.power : Powers.FirstOrDefault();
        targetSelect.ClearOptions();
        var targetOptions = (Enum.GetValues(typeof(TargetType)) as TargetType[]).Select(e => new Dropdown.OptionData{text = e.ToString()}).ToList();
        targetSelect.AddOptions(targetOptions);
        targetSelect.value = (int)Action.target;
        
        checkerSelect.ClearOptions();
        var checkersOptions = (Enum.GetValues(typeof(CombatAIChecker)) as CombatAIChecker[]).Select(e => new Dropdown.OptionData{text = e.ToString()}).ToList();
        checkerSelect.AddOptions(checkersOptions);   
        checkerSelect.value = (int)Action.checker;
  
        valueSelect.ClearOptions();
        var valueOptions = (Enum.GetValues(typeof(CombatAIValues)) as CombatAIValues[]).Select(e => new Dropdown.OptionData{text = e.ToString()}).ToList();
        valueSelect.AddOptions(valueOptions);   
        valueSelect.value = (int)Action.value;

        conditionSelect.ClearOptions();
        var conditionOptions = (Enum.GetValues(typeof(CombatAICondition)) as CombatAICondition[]).Select(e => new Dropdown.OptionData{text = e.ToString()}).ToList();
        conditionSelect.AddOptions(conditionOptions);    
        conditionSelect.value = (int)Action.condition;
   
        powerSelect.ClearOptions();
        var powersOptions = Powers.Select(e => new Dropdown.OptionData{text = e.name.ToString()}).ToList();
        powerSelect.AddOptions(powersOptions); 
    }

    public void TargetSelected()
    {
        Action.target = ((TargetType[]) Enum.GetValues(typeof(TargetType)))[targetSelect.value];
        Debug.Log(JsonUtility.ToJson(Action));
    }   
    public void CheckerSelected()
    {
        Action.checker = ((CombatAIChecker[]) Enum.GetValues(typeof(CombatAIChecker)))[checkerSelect.value];
        Debug.Log(JsonUtility.ToJson(Action));
    }    
    public void ConditionSelected()
    {
        Action.condition = ((CombatAICondition[]) Enum.GetValues(typeof(CombatAICondition)))[conditionSelect.value];
        Debug.Log(JsonUtility.ToJson(Action));
    }   
    public void ValueSelected()
    {
        Action.value = ((CombatAIValues[]) Enum.GetValues(typeof(CombatAIValues)))[valueSelect.value];
        Debug.Log(JsonUtility.ToJson(Action));
    }    
    public void PowerSelected()
    {
        Action.power = Powers[powerSelect.value];
        Debug.Log(JsonUtility.ToJson(Action));
    }
    public void RemoveRow()
    {
        HeroTactics.RemoveRow(Index);
    }
}