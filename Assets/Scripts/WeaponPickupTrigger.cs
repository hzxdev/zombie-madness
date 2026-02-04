using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickupTrigger : MonoBehaviour
{

    public GunController gunController;
    public Collider playerCol;

    private void Start()
    {
        Physics.IgnoreCollision(playerCol, GetComponent<BoxCollider>());
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.transform.CompareTag("DroppedWeapon"))
        {
            DroppedWeapon dw = col.transform.GetComponent<DroppedWeapon>();
            gunController.DroppedWeaponEnteredPickupRange(dw);
            
        }
    }

    /*
    void OnTriggerStay(Collider col)
    {
        if (col.transform.CompareTag("DroppedWeapon"))
        {
            DroppedWeapon dw = col.transform.GetComponent<DroppedWeapon>();
            gunController.DroppedWeaponEnteredPickupRange(dw);

        }
       // Debug.Log(col.transform.name);
    }*/

    void OnCollisionExit(Collision col)
    {

    }
}
