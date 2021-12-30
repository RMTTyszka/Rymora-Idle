using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject heroDetailsContainer;
    public GameManager GameManager { get; set;}
    public Camera worldCamera;
    public bool CombatOn { get; set; }

    private void Start()
    {
        GameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void ShowHeroSheet()
    {
        heroDetailsContainer.SetActive(!heroDetailsContainer.activeSelf);
    }  
    public void ToggleCombat()
    {

        if (CombatOn)
        {
            GameManager.CurrentScreen = ScreenState.Map;
            worldCamera.gameObject.SetActive(true);
            foreach (var party in GameManager.partyManager.parties)
            {
                party.combatCamera.gameObject.SetActive(false);
            }
            CombatOn = false;
        }
        else
        {
            GameManager.CurrentScreen = ScreenState.Combat;
            worldCamera.gameObject.SetActive(false);
            SetCombatCamera();
 
            CombatOn = true;
        }

    }

    private void SetCombatCamera()
    {
        GameManager.partyManager.CurrentPartyNode.combatCamera.gameObject.SetActive(true);
    }
}
