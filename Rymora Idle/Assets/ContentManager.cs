using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ContentManager
{
    public static void PublishVisitableSelected(string visitableName)
    {
        RealmManagerService.PublishVisitableSelected(visitableName);
    }
}
