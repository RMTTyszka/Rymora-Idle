using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatActionMenu : MonoBehaviour
{
    public CombatActionRow CombatAction { get; set; }

    public Dropdown propertyDropdown;
    public Dropdown conditionDropdown;
    public Dropdown valueDropdown;
    public Dropdown powerDropdown;
    
    private Dictionary<string, CombatActionTargetProperty> PropertyByName { get; set; }

    public CombatActionMenu()
    {
        CombatAction = new CombatActionRow();
        PropertyByName = new Dictionary<string, CombatActionTargetProperty>();
    }
    // Start is called before the first frame update
    void Start()
    {

        propertyDropdown.options.Clear();
        foreach (var property in Enum.GetNames(typeof(CombatActionTargetProperty)))
        {
            PropertyByName.Add(property, Enum.Parse<CombatActionTargetProperty>(property));
            propertyDropdown.options.Add(new Dropdown.OptionData
            {
                text = property
            }); 
        }

    }

    public void PropertySelected(int property)
    {
        CombatAction.targetProperty = PropertyByName[property];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
