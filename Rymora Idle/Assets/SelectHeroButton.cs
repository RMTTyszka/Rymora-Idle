using System;
using System.Collections;
using System.Collections.Generic;
using Global;
using Newtonsoft.Json;
using UnityEngine;

public class SelectHeroButton : MonoBehaviour
{
    public PartyManager partyManager;

    public int heroIndex;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void HeroSelected()
    {
        Debug.Log(JsonConvert.SerializeObject(partyManager.heroes[heroIndex]));
        partyManager.PublishHeroSelected(partyManager.heroes[heroIndex]);
    }



}
