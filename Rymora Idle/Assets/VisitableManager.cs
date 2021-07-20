using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Areas;
using Newtonsoft.Json;
using UnityEngine;

public class VisitableManager : MonoBehaviour
{

    public List<VisitableComponent> visitables;

    public VisitableManager()
    {
        visitables = new List<VisitableComponent>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        SubscribeToVisitableChanged();
        EnableDefaultVisitable();
    }

    void OnEnable()
    {
        EnableDefaultVisitable();
    }

    void EnableDefaultVisitable()
    {
        UpdateVisitableView(visitables.FirstOrDefault()?.visitable.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SubscribeToVisitableChanged()
    {
        RealmManagerService.OnRealmSelected += UpdateVisitableView;
    }

    private void UpdateVisitableView(string visitableName)
    {
        foreach (var visitable in visitables)
        {
            if (visitable.visitable.name.Equals(visitableName))
            {
                visitable.gameObject.SetActive(true);
            }
            else
            {
                visitable.gameObject.SetActive(false);
            }
        }
    }
}
