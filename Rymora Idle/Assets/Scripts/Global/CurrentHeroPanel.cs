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
    public PartyNode CurrentPartyNode { get; set; }

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

    public void UpdateCurrentInfoDisplayData(PartyNode partyNode)
    {
        CurrentPartyNode = partyNode;
        heroName.text = partyNode.Party.Hero.Name;
        heroLevel.text = partyNode.Party.Hero.Level.ToString();
        UpdateActions(partyNode);
    }

    private void UpdateActions(PartyNode partyNode)
    {
        if (partyNode.Equals(CurrentPartyNode))
        {
            foreach (var child in wayPointsPanel.GetComponentsInChildren<Text>())
            {
                Destroy(child.gameObject);
            }
            var title = Instantiate(textPrefab, Vector3.zero, transform.rotation) as Text;
            title.transform.SetParent(wayPointsPanel.transform, false);
            title.fontSize = 16;
            title.text = $"Actions";
            foreach (var action in CurrentPartyNode.NextActions)
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
        if (partyManager.CurrentPartyNode.CurrentAction != null)
        {
            if (partyManager.CurrentPartyNode.EndActionTime.HasValue &&
                partyManager.CurrentPartyNode.CurrentActionTime.HasValue &&
                partyManager.CurrentPartyNode.EndActionTime.Value != 0)
            {
                actionBar.BarValue = (float) (partyManager.CurrentPartyNode.CurrentActionTime.Value / partyManager.CurrentPartyNode.EndActionTime.Value * 100);
                actionBar.CurrentSeconds = (float)partyManager.CurrentPartyNode.CurrentActionTime.Value;
                actionBar.TotalSeconds = partyManager.CurrentPartyNode.CurrentAction.TimeToExecute;
            }
            else
            {
                actionBar.CurrentSeconds = 0;
                actionBar.BarValue = 0;
                actionBar.TotalSeconds = 0;
            }

            actionBar.Title = partyManager.CurrentPartyNode.CurrentAction.ActionName;
        }
        else
        {
            actionBar.Title = "Idle";
        }


    }
    
}
