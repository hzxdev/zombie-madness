using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Weapon wep;

    private void Start()
    {
        Collider alertArea = GameObject.Find("AlertArea").GetComponent<Collider>();

        for (int i = 0; i < GetComponents<Collider>().Length; i++)
        {
            Physics.IgnoreCollision(GetComponents<Collider>()[i], alertArea);
        }
    }

}
