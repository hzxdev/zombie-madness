using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum WeaponType
{
    Rifle,
    Sniper,
    Shotgun,
    Submachine,
    Pistol,
    Projectile,
    Melee
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "New Weapon")]
public class Weapon : ScriptableObject
{

    [Header("General Properties")]
    public string weaponName;
    public int weaponId;
    public WeaponType weaponType;
    public Sprite weaponIcon;
    public Sprite weaponRealModelIcon;
    [Header("Combat Properties")]
    public int maxTotalAmmo;
    public int maxMagAmmo;
    public int headDamage; 
    public int bodyDamage;
    public int limbDamage;
    public int bulletCount;
    public float cooldown, range, standingSpread, movingSpread, verticalRecoil, horizontalRecoil;
    public bool holdUse;
    [Header("Animation Settings")]
    public AnimatorOverrideController animatorOverride;
    public float rightHandXOffset;
    public float rightHandZOffset;
    [Header("Other")]
    public GameObject droppedPrefab;
    public string description;
    public int pickupAmmo;
    public int magPrice;
    public float zombieSlowDownTime;
    public int unlockDay;
    public GameObject thrownProjectileObj;


}
