using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameplayManager;

public class BomberZombie : ZombieTemp, IPoolable
{
    public Weapon bombWep;
    public Transform bombsParent;
    GameObject bombGO;
    public float upForce = 300, forwardThrowJumltiplier = 150;
    public Material trailMat;

    public override void Start()
    {
        base.Start();
        if (bombWep.weaponType != WeaponType.Projectile && bombWep != null)
            Debug.LogError("Wep not a bomb!");

        for (int i = 0; i < bombsParent.childCount; i++)
        {
            if (bombsParent.GetChild(i).gameObject.name == bombWep.weaponName)
            {
                bombGO = bombsParent.GetChild(i).gameObject;
               
            }
        }
        bombGO.SetActive(true);


        if(bombGO == null)
        Debug.Log("Couldnt find bomb obj");

    }

   

    public void ZombieThrowMoment()
    {
        if (PlayerHealth.instance.alive && !gameplayManager.afterDeath && !gameplayManager.levelCompleted)
        {
            GameObject go = Instantiate(bombWep.thrownProjectileObj, bombGO.transform.position, bombGO.transform.rotation);
            go.GetComponent<Rigidbody>().AddForce(transform.forward * Vector3.Distance(base.target.position, transform.position) * forwardThrowJumltiplier);
            go.GetComponent<Rigidbody>().AddForce(transform.up * upForce);
            go.GetComponent<Grenade>().thrownByZombie = true;
            go.GetComponent<TrailRenderer>().startColor = trailMat.color;
            PlayZombieAttackSound();
        }

    }

    public override void ZombieAttackMoment()
    {
        
    }

 
    public override void DropRandom()
    {
        int r = Random.Range(0, 3);
        if (r == 2)
        {
            for (int i = 0; i < WeaponsList.instance.pickups.Length; i++)
            {
                if(WeaponsList.instance.pickups[i].pickupName == bombWep.weaponName + "Pickup")
                    Instantiate(WeaponsList.instance.pickups[i].prefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);

            }
            
    
                
       
        }
    }

}
