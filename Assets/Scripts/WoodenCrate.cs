using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WoodenCrateType
{
    DropRandomWep,
    DropRandomPickup,
    DropDefinedWep,
    DropDefinedPickup,
    Empty
}

public class WoodenCrate : MonoBehaviour
{
    public int maxHealth;
    public GameObject particle;
    public WoodenCrateType crateType;
    public Pickup definedWepPickup;

    int lastPlayerImpactSound;
    public AudioClip[] impactSounds;
    AudioSource source;
    bool broken;

    int health;

    private void Start()
    {
        health = maxHealth;
        source = GetComponent<AudioSource>();
    }

    public void TakeDamage(int damage)
    {
        if (broken)
            return;

        health -= damage;
        if (health <= 0)
        {
            Break();
            broken = true;
        } else
        {
            source.PlayOneShot(impactSounds[lastPlayerImpactSound]);
            lastPlayerImpactSound++;
            if (lastPlayerImpactSound == impactSounds.Length)
                lastPlayerImpactSound = 0;
        }
        //   particle.Play();

    }

    public void Break()
    {
        AudioManager.instance.PlayOneShot("crateBreak"); // gotta play from instance because object gets destroyed
        switch (crateType)
        {
            case WoodenCrateType.DropRandomWep:
                Weapon randomWep = WeaponsList.instance.weapons[Random.Range(0, WeaponsList.instance.weapons.Length)];
                Instantiate(randomWep.droppedPrefab, transform.position, Quaternion.identity);

                break;
            case WoodenCrateType.DropRandomPickup:
                Pickup randomPickup = WeaponsList.instance.pickups[Random.Range(0, WeaponsList.instance.pickups.Length )];
                if (randomPickup.pickupType == PickupType.Weapon) // if weapon pickup should be spawned a bit higher
                    Instantiate(randomPickup.prefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
                else
                    Instantiate(randomPickup.prefab, transform.position, Quaternion.identity);

                break;
            case WoodenCrateType.DropDefinedWep:
                Instantiate(definedWepPickup.prefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
                break;
            case WoodenCrateType.DropDefinedPickup:
                break;
            case WoodenCrateType.Empty:
                break;
        }

        
        Destroy(gameObject);
    }
}
