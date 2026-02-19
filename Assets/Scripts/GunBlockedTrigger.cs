using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBlockedTrigger : MonoBehaviour
{
    GunController gunController;
    Collider boxCol;
    public bool gunBarrelNearTrigger;
    public LayerMask layerMask;
    public string lastColName, lastColTransName;

    private void Start()
    {
        gunController = GameObject.FindGameObjectWithTag("Player").GetComponent<GunController>();
      //  boxCol = GetComponent<BoxCollider>();
    //    Physics.IgnoreCollision(boxCol, GameObject.FindGameObjectWithTag("Player").GetComponent<CapsuleCollider>());


    }

    private void Update()
    {
        if(Physics.CheckSphere(transform.position, 0.25f, layerMask, QueryTriggerInteraction.Ignore))
        {
           
                gunBarrelNearTrigger = true;
                gunController.gunBarrelNearTrigger = true;
           
      


            //SWATTAKÝ COLLIDERI ALGILIYOR, ALGILAMAMASI LAZIM




        } else
        {
            gunBarrelNearTrigger = false;
            gunController.gunBarrelNearTrigger = false;
        }

        

    }
    /*
    private void OnTriggerEnter(Collider col)
    {

        if(col.gameObject.layer == LayerMask.NameToLayer("Environment") || col.gameObject.CompareTag("Zombie"))
        {
            gunBarrelNearTrigger = true;
            gunController.gunBarrelNearTrigger = true;
            Debug.Log("LAST ENTERED: " + col.name);
        }
        
    }



    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Environment") || col.gameObject.CompareTag("Zombie"))
        {

            //if (!col.bounds.Contains(boxCol.bounds.extents))
           // {
                gunBarrelNearTrigger = false;
                gunController.gunBarrelNearTrigger = false;
            Debug.Log("LAST EXIT: " + col.name);
            // }

        }
           
    }
    */
   
}

