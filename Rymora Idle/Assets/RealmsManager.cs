using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class RealmsManager : MonoBehaviour
{

    public List<MapComponent> maps;
    
    
    // Start is called before the first frame update
    void Start()
    {
        SubscribeToMapChanged();
        UpdateMapView("Rymora");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SubscribeToMapChanged()
    {
        RealmManagerService.OnRealmSelected += UpdateMapView;
    }

    private void UpdateMapView(string mapName)
    {
        foreach (var mapComponent in maps)
        {
            if (mapComponent.realmName.Equals(mapName))
            {
                mapComponent.gameObject.SetActive(true);
            }
            else
            {
                mapComponent.gameObject.SetActive(false);
            }
        }
    }
}
