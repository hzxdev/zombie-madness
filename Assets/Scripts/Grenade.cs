using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class Grenade : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject explosionEffect;
    public Weapon grenadeSO;
    public bool thrownByZombie = false;
    Collider[] colliders;
    void Start()
    {
        Invoke(nameof(Explode), 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Explode()
    {
        if (GameplayManager.instance.levelCompleted)
            Destroy(gameObject);
        
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        AudioManager.instance.PlayOneShot("explosion");

       colliders = Physics.OverlapSphere(transform.position, grenadeSO.range);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.CompareTag("Zombie"))
            {
               if(!thrownByZombie)
                {
                    nearbyObject.GetComponent<ZombieTemp>().TakeDamage(grenadeSO.bodyDamage, BodyPart.Part.Torso, nearbyObject.transform.position, Quaternion.identity, false); //pos sýkýntýlý, kan efekti çýkmicak normalde   
                } else
                {
                    nearbyObject.GetComponent<ZombieTemp>().TakeDamage((int)Math.Ceiling((double)(grenadeSO.bodyDamage / 20)), BodyPart.Part.Torso, nearbyObject.transform.position, Quaternion.identity, false); //pos sýkýntýlý, kan efekti çýkmicak normalde   
                }
                
            }
            else if (nearbyObject.CompareTag("Player"))
            {
                PlayerHealth.instance.PlayerTakeDamage((int)Mathf.Lerp(0, 90, 1 / Vector3.Distance(PlayerHealth.instance.transform.position, transform.position)));
            } else if(nearbyObject.CompareTag("WoodenCrate"))
            {
                nearbyObject.GetComponent<WoodenCrate>().Break();
            } else if(nearbyObject.name == "AlertArea")
            {
                nearbyObject.GetComponent<AlertArea>().BombExploded();
            }
        }
        Destroy(gameObject);
    }
}
