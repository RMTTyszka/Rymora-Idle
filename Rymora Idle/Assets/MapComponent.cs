using System.Collections;
using System.Collections.Generic;
using Areas.Realms;
using UnityEngine;

public class MapComponent : MonoBehaviour
{
    public string realmName;
    public Realm Realm { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        Realm = new Realm
        {
            Name = realmName
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hide()
    {
        
    }
}
