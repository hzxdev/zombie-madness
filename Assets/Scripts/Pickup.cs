using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum PickupType
{
    AddHealth,
    AddArmor,
    Weapon
    
}

[CreateAssetMenu(fileName = "NewPickup", menuName = "New Pickup")]
public class Pickup : ScriptableObject
{
  
    public string pickupName;
    public int pickupId;
    public PickupType pickupType;
    public int value;
    public GameObject prefab, particlePrefab;



}

