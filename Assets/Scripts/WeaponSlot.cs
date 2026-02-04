using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponSlotType
{
    Primary = 1,
    Secondary = 2,
    Melee = 3,
    Projectile = 4
}


[System.Serializable]
public class WeaponSlot
{

    public WeaponSlotType slotType;
    public Weapon weapon;
    public int magAmmo, unloadedAmmo;
}
