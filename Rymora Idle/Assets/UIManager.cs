using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject heroDetailsContainer;

    public void ShowHeroSheet()
    {
        heroDetailsContainer.SetActive(!heroDetailsContainer.activeSelf);
    }
}
