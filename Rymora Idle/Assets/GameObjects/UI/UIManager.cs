using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject heroDetailsContainer;
    public GameManager GameManager { get; set;}
    public Camera worldCamera;
    public Camera combatCamera;

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
      
        GameManager.CurrentScreen = !combatCamera.gameObject.activeSelf ? ScreenState.Combat : ScreenState.Map;
        worldCamera.gameObject.SetActive(!worldCamera.gameObject.activeSelf);
        combatCamera.gameObject.SetActive(!combatCamera.gameObject.activeSelf);
    }
}
