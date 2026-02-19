using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertArea : MonoBehaviour
{
    GunController gunController;
    Transform player;

    void Start()
    {
        gunController = GunController.instance;
        player = GunController.instance.transform;
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.transform.CompareTag("Thrown"))
        {
           if(col.gameObject.GetComponent<Grenade>().thrownByZombie)
            gunController.AlertAreaBombEntered(col.transform); 
        }
    }

    public void BombExploded()
    {
        gunController.AlertAreaBombExploded();
    }

 

}
