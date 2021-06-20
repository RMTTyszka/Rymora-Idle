using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealmSelectButton : MonoBehaviour
{
    public MapComponent realm;
    public void SelectRealm()
    {
        RealmManagerService.PublishRealmSelected(realm.realmName);
    }
}
