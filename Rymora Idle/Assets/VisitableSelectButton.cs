using System.Collections;
using System.Collections.Generic;
using Areas;
using UnityEngine;

public class VisitableSelectButton : MonoBehaviour
{
    public Visitable visitable;
    public void SelectRealm()
    {
        RealmManagerService.PublishVisitableSelected(visitable.name);
    }
}
