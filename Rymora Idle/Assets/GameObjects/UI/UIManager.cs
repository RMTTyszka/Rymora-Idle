using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    public GameObject statusContainer;
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
        statusContainer.SetActive(!statusContainer.activeSelf);
    }  
    public void ToggleCombat()
    {
        heroDetailsContainer.SetActive(!heroDetailsContainer.activeSelf);
        GameManager.CurrentScreen = !combatCamera.gameObject.activeSelf ? ScreenState.Combat : ScreenState.Map;
        worldCamera.gameObject.SetActive(!worldCamera.gameObject.activeSelf);
        combatCamera.gameObject.SetActive(!combatCamera.gameObject.activeSelf);
    }
}
