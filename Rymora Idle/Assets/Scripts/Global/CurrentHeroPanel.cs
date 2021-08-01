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
    public Hero CurrentHero { get; set; }

    public Text heroName;
    public Text heroLevel;
    public GameObject wayPointsPanel;
    public Text textPrefab;
    public ProgressBar actionBar;
    
    // Start is called before the first frame update
    void Start()
    {
        partyManager.OnHeroSelected += UpdateCurrentInfoDisplayData;
        partyManager.OnWayPointChanged += UpdateWayPoints;
    }

    public void UpdateCurrentInfoDisplayData(Hero hero)
    {
        CurrentHero = hero;
        heroName.text = hero.Name;
        heroLevel.text = hero.Level.ToString();
        UpdateWayPoints(hero);
    }

    private void UpdateWayPoints(Hero hero)
    {
        if (hero.Equals(CurrentHero))
        {
            foreach (var child in wayPointsPanel.GetComponentsInChildren<Text>())
            {
                Destroy(child.gameObject);
            }
            var title = Instantiate(textPrefab, Vector3.zero, transform.rotation) as Text;
            title.transform.SetParent(wayPointsPanel.transform, false);
            title.fontSize = 16;
            title.text = $"Way Points";
            foreach (var wayPoint in CurrentHero.WayPoints)
            {
                var tempTextBox = Instantiate(textPrefab, Vector3.zero, transform.rotation) as Text;
                //Parent to the panel
                tempTextBox.transform.SetParent(wayPointsPanel.transform, false);
                //Set the text box's text element font size and style:
                tempTextBox.fontSize = 12;
                //Set the text box's text element to the current textToDisplay:
                tempTextBox.text = $"{Math.Round(wayPoint.x)} / {Math.Round(wayPoint.y)}";
            }   
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (partyManager.CurrentHero.CurrentAction != null && partyManager.CurrentHero.EndActionTime.HasValue && partyManager.CurrentHero.CurrentActionTime.HasValue && partyManager.CurrentHero.EndActionTime.Value != 0)
        {
            actionBar.BarValue = (float) (partyManager.CurrentHero.CurrentActionTime.Value / partyManager.CurrentHero.EndActionTime.Value * 100);
        }

    }
    
}
