using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieFOV : MonoBehaviour
{
    ZombieTemp zombie;
    LayerMask obstacleMask;
    public Vector3 playerOffset, zombieOffset;

     void Start()
    {
        zombie = transform.parent.GetComponent<ZombieTemp>();
    }

    

    private void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            //FOR PERFORMANCE, DISABLED NOT BEING ABLE TO SEE THROUGH WALLS
          //  if(!Physics.Linecast(transform.position + zombieOffset, col.transform.position + playerOffset, obstacleMask)) //Önünde bir þey yoksa
            zombie.PlayerEnteredFieldOfVision();
        }
    }

 
    //SAME HERE
    /*
    private void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            Debug.DrawLine(transform.position + zombieOffset, col.transform.position + playerOffset);
        }
    }
    */
}
