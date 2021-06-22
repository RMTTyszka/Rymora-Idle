using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RealmManagerService
{
    public delegate void RealmSelected(string mapName);

    public static event RealmSelected OnRealmSelected;
    
    
    // Start is called before the first frame update

    public static void PublishVisitableSelected(string mapName)
    {
        OnRealmSelected?.Invoke(mapName);
    }
}
