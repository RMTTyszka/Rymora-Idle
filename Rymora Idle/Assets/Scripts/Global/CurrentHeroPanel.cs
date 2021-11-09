using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Heroes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class CurrentHeroPanel : MonoBehaviour
{


    public PartyManager partyManager;
    public Party CurrentParty { get; set; }

    public Text heroName;
    public Text heroLevel;
    public GameObject wayPointsPanel;
    public Text textPrefab;
    public ProgressBar actionBar;
    
    // Start is called before the first frame update
    void Start()
    {
        partyManager.OnHeroSelected += UpdateCurrentInfoDisplayData;
        partyManager.OnActionUpdated += UpdateActions;
    }

    public void UpdateCurrentInfoDisplayData(Party party)
    {
        CurrentParty = party;
        heroName.text = party.Hero.Name;
        heroLevel.text = party.Hero.Level.ToString();
        UpdateActions(party);
    }

    private void UpdateActions(Party party)
    {
        if (party.Equals(CurrentParty))
        {
            foreach (var child in wayPointsPanel.GetComponentsInChildren<Text>())
            {
                Destroy(child.gameObject);
            }
            var title = Instantiate(textPrefab, Vector3.zero, transform.rotation) as Text;
            title.transform.SetParent(wayPointsPanel.transform, false);
            title.fontSize = 16;
            title.text = $"Actions";
            foreach (var action in CurrentParty.NextActions)
            {
                var tempTextBox = Instantiate(textPrefab, Vector3.zero, transform.rotation) as Text;
                //Parent to the panel
                tempTextBox.transform.SetParent(wayPointsPanel.transform, false);
                //Set the text box's text element font size and style:
                tempTextBox.fontSize = 12;
                //Set the text box's text element to the current textToDisplay:
                tempTextBox.text = $"{action.ActionName}";
            }   
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (partyManager.CurrentParty.CurrentAction != null)
        {
            if (partyManager.CurrentParty.EndActionTime.HasValue &&
                partyManager.CurrentParty.CurrentActionTime.HasValue &&
                partyManager.CurrentParty.EndActionTime.Value != 0)
            {
                actionBar.BarValue = (float) (partyManager.CurrentParty.CurrentActionTime.Value / partyManager.CurrentParty.EndActionTime.Value * 100);
                actionBar.CurrentSeconds = (float)partyManager.CurrentParty.CurrentActionTime.Value;
                actionBar.TotalSeconds = partyManager.CurrentParty.CurrentAction.TimeToExecute;
            }
            else
            {
                actionBar.CurrentSeconds = 0;
                actionBar.BarValue = 0;
                actionBar.TotalSeconds = 0;
            }

            actionBar.Title = partyManager.CurrentParty.CurrentAction.ActionName;
        }
        else
        {
            actionBar.Title = "Idle";
        }


    }
    
}
