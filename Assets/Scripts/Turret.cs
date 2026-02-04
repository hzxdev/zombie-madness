using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour, IDataPersistence
{
    public int id, price;
    public int damage;
    public bool isActivated, targeted;
    public float radius, checkInterval = 2, shootInterval;
    public Transform currentTarget, turretBarrel;
    ZombieTemp currentZombie;
    public ParticleSystem muzzleFlash;
    AudioSource source;
    public AudioClip[] sounds;
    public GameObject turretBullet;
    public Material activatedMat, inactiveMat;
     MeshRenderer meshRenderer;
    GameplayManager gameplayManager;


    public void LoadData(GameData data)
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        gameplayManager = GameplayManager.instance;
        if (data.lastDay < 25 || DataPersistenceManager.instance.currentGamemode == GameMode.Survival)
        {
            Destroy(gameObject);
        }

        if (data.turrets[id])
        {
            isActivated = true;
            InvokeRepeating(nameof(CheckForTargets), checkInterval, checkInterval);
            meshRenderer.material = activatedMat;
        } else
        {
            meshRenderer.material = inactiveMat;
        }

        Physics.IgnoreCollision(GetComponentInChildren<BoxCollider>(), GameObject.FindObjectOfType<AlertArea>().GetComponent<BoxCollider>());

        source = GetComponent<AudioSource>();
    }

    public void SaveData(GameData data)
    {

    }
   
    public void Activate()
    {
        InvokeRepeating(nameof(CheckForTargets), checkInterval, checkInterval);
        meshRenderer.material = activatedMat;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActivated)
            return;

        

        if(targeted)
        {
            var lookPos = currentTarget.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 4);
          
        } else
        {
            transform.Rotate(Vector3.up * Time.deltaTime * 15);
        }

    }

    void Shoot()
    {
        if (!targeted)
            return;

        if (gameplayManager.afterDeath)
            return;
        if (gameplayManager.levelCompleted)
            return;

        if (DataPersistenceManager.instance.gameData.lastDay < 25 || DataPersistenceManager.instance.currentGamemode == GameMode.Survival)
        {
            return;
        }

        source.PlayOneShot(sounds[0]);
        muzzleFlash.Play();
        RaycastHit hit;
      // GameObject bullet = Instantiate(turretBullet, turretBarrel.position, Quaternion.Euler(turretBarrel.forward));


       if(Physics.Raycast(turretBarrel.position, turretBarrel.forward, out hit, radius * 2))
        {
            if(hit.transform.root.name == GameplayManager.instance.RootPoolName)
            {
                ZombieTemp zombie = hit.collider.transform.GetComponentInParent<ZombieTemp>();
                if (!hit.collider.transform.CompareTag("ExplosiveBarrel"))
                {
                    switch (hit.collider.transform.GetComponent<BodyPart>().bodyPart)
                    {
                        case BodyPart.Part.Head:


                            zombie.TakeDamage(damage, BodyPart.Part.Head, hit.point, Quaternion.LookRotation(hit.normal), true);
                            break;
                        case BodyPart.Part.Torso:

                            zombie.TakeDamage(damage, BodyPart.Part.Torso, hit.point, Quaternion.LookRotation(hit.normal), true);
                            break;
                        case BodyPart.Part.Limb:

                            zombie.TakeDamage(damage, BodyPart.Part.Limb, hit.point, Quaternion.LookRotation(hit.normal), true);
                            break;
                    }
                }
                else
                {
                    hit.collider.transform.GetComponentInParent<ExplosiveZombie>().BarrelHitByGun();
                }
               // Destroy(bullet);
            } else if (hit.collider.transform.CompareTag("WoodenCrate"))
            {
                WoodenCrate crate = hit.collider.transform.GetComponent<WoodenCrate>();
                Instantiate(crate.particle, hit.point, Quaternion.LookRotation(hit.normal));
                crate.TakeDamage(150);
            }
        }
    }

    void CheckForTargets()
    {
        Debug.Log("turret check for targets");

        if(!targeted)
        source.PlayOneShot(sounds[1]);

        RaycastHit[] targets = Physics.SphereCastAll(transform.position, radius, transform.forward);
        foreach (var target in targets)
        {
            if(target.collider.transform.root.name == GameplayManager.instance.RootPoolName)
            {
                //it's a zombie

               
                ZombieTemp zombie = target.collider.transform.GetComponentInParent<ZombieTemp>();
                currentTarget = zombie.transform;

                if (currentTarget.gameObject.activeSelf)
                {
                    targeted = true;
                    Shoot();
                    break;
                }
               
            }
            currentTarget = null;
            targeted = false;
        }

        
    }


    private void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Player"))
        {
            //turret ui prompt
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            //turret ui prompt close
        }
    }

  
}
