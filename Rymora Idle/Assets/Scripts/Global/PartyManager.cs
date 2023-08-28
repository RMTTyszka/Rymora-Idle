using System.Collections.Generic;
using Heroes;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    
    public Camera MapCamera { get; set; }

    public delegate void HeroSelected(Party party);

    public event HeroSelected OnHeroSelected;
    
    public delegate void ActionUpdated(Party party);

    public event ActionUpdated OnActionUpdated; 
    
    public delegate void InventoryUpdate(Party party);

    public event InventoryUpdate OnInventoryUpdate;

    [SerializeField]
    public List<Party> heroes;

    public Party CurrentParty { get; set; }

    private void Start()
    {
        MapCamera = Camera.main;
    }

    public void PublishHeroSelected(Party party)
    {
        CurrentParty = party;
        OnHeroSelected?.Invoke(party);
        Vector3 position = Vector3Int.FloorToInt(new Vector3(party.gameObject.transform.position.x, party.gameObject.transform.position.y, MapCamera.transform.position.z));
        MapCamera.transform.position = position;
    }

    public void PublishActionsUpdated(Party party)
    {
        OnActionUpdated?.Invoke(party);
    }   
    public void PublishInventoryUpdate(Party party)
    {
        if (party == CurrentParty)
        {
            OnInventoryUpdate?.Invoke(party);
        }
    }

}
