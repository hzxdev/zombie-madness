using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedWeapon : MonoBehaviour
{
    Rigidbody rigid;
    public Weapon weapon;
    public int unreloadedAmmo;
    public int ammoLeftInMag;
     void Awake()
    {
        unreloadedAmmo = weapon.maxTotalAmmo - weapon.maxMagAmmo;
        ammoLeftInMag =   weapon.maxMagAmmo;
        rigid = GetComponent<Rigidbody>();
    }

    

    public void Throw(Vector3 direction)
    {
        rigid.AddForce(direction * 400);
    }

}
