using System;

public class Equipment
{
    public WeaponInstance MainHand { get; set; }
    public Equipable Offhand { get; set; }
    public Equipable Head { get; set; }
    public Equipable Neck { get; set; }
    public ArmorInstance Chest { get; set; }
    public Equipable Wrist { get; set; }
    public Equipable Hand { get; set; }
    public Equipable FingerLeft { get; set; }
    public Equipable FingerRight { get; set; }
    public Equipable Waist { get; set; }
    public Equipable Feet { get; set; }
    public Equipable Extra { get; set; }

    public Equipable Equip(Equipable item)
    {
        Equipable previousItem;
        switch (item.Slot)
        {
            case Slot.None:
                return null;
            case Slot.MainHand:
                previousItem = MainHand;
                MainHand = item as WeaponInstance;
                break;
            case Slot.Offhand:
                previousItem = Offhand;
                Offhand = item;
                break;
            case Slot.Head:
                previousItem = Head;
                Head = item;
                break;
            case Slot.Neck:
                previousItem = Neck;
                Neck = item;
                break;
            case Slot.Chest:
                previousItem = Chest;
                Chest = item as ArmorInstance;
                break;
            case Slot.Wrist:
                previousItem = Wrist;
                Wrist = item;
                break;
            case Slot.Hand:
                previousItem = Hand;
                Hand = item;
                break;
            case Slot.FingerLeft:
                previousItem = FingerLeft;
                FingerLeft = item;
                break;
            case Slot.FingerRight:
                previousItem = FingerRight;
                FingerRight = item;
                break;
            case Slot.Waist:
                previousItem = Waist;
                Waist = item;
                break;
            case Slot.Feet:
                previousItem = Feet;
                Feet = item;
                break;
            case Slot.Extra:
                previousItem = Extra;
                Extra = item;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return previousItem;
    }

}