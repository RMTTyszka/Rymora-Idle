using System.Collections.Generic;
using System.Linq;
using Global;
using Heroes;
using Items.Equipables.Weapons;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField]
    public List<PartyNode> parties;

    public PartyNode CurrentPartyNode { get; set; }
    public Creature CurrentHero { get; set; }
    
    
    public Camera MapCamera { get; set; }

    public delegate void PartySelected(PartyNode partyNode);
    public delegate void HeroSelected(Creature hero);

    public event PartySelected OnPartySelected;
    public event HeroSelected OnHeroSelected;
    
    public delegate void ActionUpdated(PartyNode partyNode);

    public event ActionUpdated OnActionUpdated; 
    
    public delegate void InventoryUpdate(Creature hero);

    public event InventoryUpdate OnInventoryUpdate;


    private void Awake()
    {
        var index = 0;
        foreach (var partyNode in parties)
        {
            var image = partyNode.GetComponent<SpriteRenderer>();
            partyNode.Party.Hero.Image = image.sprite;
            partyNode.Party.Hero.Name = $"Hero {++index}";
            InitiateHero(partyNode.Party.Hero);
        }
    }

    private void Start()
    {
        MapCamera = Camera.main;
        PublishPartySelected(parties.First());
    }

    private void InitiateHero(Creature hero)
    {
        hero.Level = 1;
        hero.Equipment.MainWeapon =
                Weapon.FromSizeAndDamageType(Weapon.WeaponsSize.Medium, Weapon.WeaponsDamageType.Cutting, 1);
        hero.CurrentLife = hero.MaxLife();
    }

    public void PublishPartySelected(PartyNode partyNode)
    {
        CurrentPartyNode = partyNode;
        OnPartySelected?.Invoke(partyNode);
        PublishHeroSelected(CurrentPartyNode.Party.Hero);
        Vector3 position = Vector3Int.FloorToInt(new Vector3(partyNode.gameObject.transform.position.x, partyNode.gameObject.transform.position.y, MapCamera.transform.position.z));
        MapCamera.transform.position = position;
    }
    public void PublishPartySelected(int heroIndex)
    {
        PublishPartySelected(parties[heroIndex]);
    }  
    public void PublishHeroSelected(Creature hero)
    {
        CurrentHero = hero;
        OnHeroSelected?.Invoke(hero);
    }

    public void PublishActionsUpdated(PartyNode partyNode)
    {
        OnActionUpdated?.Invoke(partyNode);
    }   
    public void PublishInventoryUpdate(Creature hero)
    {
        if (hero == CurrentHero)
        {
            OnInventoryUpdate?.Invoke(hero);
        }
    }

}
