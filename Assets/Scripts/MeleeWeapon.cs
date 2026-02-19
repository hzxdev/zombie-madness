using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    public Weapon wep;
     GunController gunController;
    AudioManager audioManager;

    private void Start()
    {
        gunController = GetComponentInParent<GunController>();
        audioManager = AudioManager.instance;

        if(wep.weaponType != WeaponType.Melee)
        {
            Debug.LogError("Melee weapon yanlýþ assign/ assignlenmemiþ");
        }
    }

    public void Check()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (Collider hitCollider in colliders)
        {
            if (hitCollider.transform.GetComponent<BodyPart>() != null)
            {
                Animator NPCanim = hitCollider.transform.GetComponentInParent<Animator>();
                ZombieTemp zombie = hitCollider.transform.GetComponentInParent<ZombieTemp>();
                Debug.Log("Knife entered trigger with " + hitCollider.transform.name);
                audioManager.PlayOneShotDontIfPlaying("stab" + Random.Range(1, 3).ToString());
                if (!hitCollider.transform.CompareTag("ExplosiveBarrel"))
                {
                    switch (hitCollider.transform.GetComponent<BodyPart>().bodyPart)
                    {
                        case BodyPart.Part.Head:
                            zombie.TakeDamage(wep.headDamage, BodyPart.Part.Head, hitCollider.ClosestPointOnBounds(transform.position), Quaternion.identity, true);
                            break;
                        case BodyPart.Part.Torso:
                            zombie.TakeDamage(wep.bodyDamage, BodyPart.Part.Torso, hitCollider.ClosestPointOnBounds(transform.position), Quaternion.identity, true);
                            break;
                        case BodyPart.Part.Limb:
                            zombie.TakeDamage(wep.limbDamage, BodyPart.Part.Limb, hitCollider.ClosestPointOnBounds(transform.position), Quaternion.identity, true);
                            break;
                    }
                }
            }
            else if (hitCollider.transform.CompareTag("WoodenCrate"))
            {
                WoodenCrate crate = hitCollider.transform.GetComponent<WoodenCrate>();
                Instantiate(crate.particle, hitCollider.ClosestPointOnBounds(transform.position), Quaternion.identity);
                crate.TakeDamage(wep.bodyDamage);
            }
            else if (hitCollider.transform.CompareTag("ExplosiveBarrel"))
            {
                hitCollider.transform.GetComponentInParent<ExplosiveZombie>().BarrelHitByGun();
            }
        }
    }

    void OnTriggerEnter(Collider hitCollider)
    {
        if (!gunController.meleeAttacking)
            return;

        if (hitCollider.transform.GetComponent<BodyPart>() != null)
        {
            Animator NPCanim = hitCollider.transform.GetComponentInParent<Animator>();
            ZombieTemp zombie = hitCollider.transform.GetComponentInParent<ZombieTemp>();



            Debug.Log("Knife entered trigger with " + hitCollider.transform.name);
            audioManager.PlayOneShotDontIfPlaying("stab" + Random.Range(1, 3).ToString());

            if (!hitCollider.transform.CompareTag("ExplosiveBarrel"))
            {
                switch (hitCollider.transform.GetComponent<BodyPart>().bodyPart)
                {
                    case BodyPart.Part.Head:

                        zombie.TakeDamage(wep.headDamage, BodyPart.Part.Head, hitCollider.ClosestPointOnBounds(transform.position), Quaternion.identity, true);
                        break;
                    case BodyPart.Part.Torso:

                        zombie.TakeDamage(wep.bodyDamage, BodyPart.Part.Torso, hitCollider.ClosestPointOnBounds(transform.position), Quaternion.identity, true);
                        break;
                    case BodyPart.Part.Limb:

                        zombie.TakeDamage(wep.limbDamage, BodyPart.Part.Limb, hitCollider.ClosestPointOnBounds(transform.position), Quaternion.identity, true);
                        break;
                }
            }

            


        }
        else if (hitCollider.transform.CompareTag("WoodenCrate"))
        {
            WoodenCrate crate = hitCollider.transform.GetComponent<WoodenCrate>();
            Instantiate(crate.particle, hitCollider.ClosestPointOnBounds(transform.position), Quaternion.identity);
            crate.TakeDamage(wep.bodyDamage);
        } else if(hitCollider.transform.CompareTag("ExplosiveBarrel"))
        {
            hitCollider.transform.GetComponentInParent<ExplosiveZombie>().BarrelHitByGun();
        }

    }
}
