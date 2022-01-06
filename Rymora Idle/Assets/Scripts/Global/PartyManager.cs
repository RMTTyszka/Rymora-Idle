using System.Collections.Generic;
using System.Linq;
using Heroes;
using Items.Equipables.Weapons;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField]
    public List<PartyNode> parties;

    public PartyNode CurrentPartyNode { get; set; }
    
    
    public Camera MapCamera { get; set; }

    public delegate void HeroSelected(PartyNode partyNode);

    public event HeroSelected OnHeroSelected;
    
    public delegate void ActionUpdated(PartyNode partyNode);

    public event ActionUpdated OnActionUpdated; 
    
    public delegate void InventoryUpdate(PartyNode partyNode);

    public event InventoryUpdate OnInventoryUpdate;



    private void Start()
    {
        MapCamera = Camera.main;
        foreach (var partyNode in parties)
        {
            var image = partyNode.GetComponent<SpriteRenderer>();
            partyNode.Party.Hero.Image = image.sprite;
            partyNode.Party.Hero.Equipment.MainWeapon =
                Weapon.FromSizeAndDamageType(Weapon.WeaponsSize.Medium, Weapon.WeaponsDamageType.Cutting);
        }
        PublishHeroSelected(parties.First());
    }

    public void PublishHeroSelected(PartyNode partyNode)
    {
        CurrentPartyNode = partyNode;
        OnHeroSelected?.Invoke(partyNode);
        Vector3 position = Vector3Int.FloorToInt(new Vector3(partyNode.gameObject.transform.position.x, partyNode.gameObject.transform.position.y, MapCamera.transform.position.z));
        MapCamera.transform.position = position;
    }

    public void PublishActionsUpdated(PartyNode partyNode)
    {
        OnActionUpdated?.Invoke(partyNode);
    }   
    public void PublishInventoryUpdate(PartyNode partyNode)
    {
        if (partyNode == CurrentPartyNode)
        {
            OnInventoryUpdate?.Invoke(partyNode);
        }
    }

}
