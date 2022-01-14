using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject heroDetailsContainer;
    public GameObject heroTacticsContainer;
    public GameManager GameManager { get; set;}
    public Camera worldCamera;
    public bool CombatOn { get; set; }

    private void Start()
    {
        GameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void ShowHeroSheet()
    {
        heroTacticsContainer.SetActive(false);
        heroDetailsContainer.SetActive(!heroDetailsContainer.activeSelf);
    }    
      public void ShowHeroTactics()
    {
        heroDetailsContainer.SetActive(false);
        heroTacticsContainer.SetActive(!heroTacticsContainer.activeSelf);
    }  
    public void ToggleCombat()
    {

        if (CombatOn)
        {
            GameManager.CurrentScreen = ScreenState.Map;
            worldCamera.gameObject.SetActive(true);
            foreach (var party in GameManager.PartyManager.parties)
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
        GameManager.PartyManager.CurrentPartyNode.combatCamera.gameObject.SetActive(true);
    }
}
