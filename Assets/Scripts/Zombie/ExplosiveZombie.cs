using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameplayManager;

public class ExplosiveZombie : ZombieTemp, IPoolable
{

    public GameObject barrel;
    public ParticleSystem explosionEffect;
    public bool hasExplodedSinceSpawn;

    public override void Start()
    {
        base.Start();
    }



    
    public void BarrelHitByGun()
    {
        Explode();
    }

    public void Explode()
    {
        hasExplodedSinceSpawn = true;
        explosionEffect.Play();
        barrel.SetActive(false);
        AudioManager.instance.PlayOneShot("explosion");
        if(alive)
        Die(BodyPart.Part.Torso, false);

        Collider[] colliders = Physics.OverlapSphere(barrel.transform.position, 3);
        foreach (Collider nearbyObject in colliders) 
        {
            if(nearbyObject.CompareTag("Zombie"))
            {
                nearbyObject.GetComponent<ZombieTemp>().TakeDamage(50, BodyPart.Part.Torso, nearbyObject.transform.position, Quaternion.identity, false); //pos sýkýntýlý, kan efekti çýkmicak normalde   
            } else if (nearbyObject.CompareTag("Player"))
            {
                PlayerHealth.instance.PlayerTakeDamage((int)Mathf.Lerp(0, 90, 1/Vector3.Distance(PlayerHealth.instance.transform.position, transform.position) ));
            }
        }
        CancelInvoke(nameof(Explode));
    }

    public override void Spawn()
    {
        base.Spawn();
        barrel.SetActive(true);
    }

    public override void Die(BodyPart.Part shotFromWhere, bool killedByGame)
    {
        alive = false;

        if (!killedByGame)
            gameplayManager.AddKill();

        agent.enabled = false;
        gameplayManager.aliveEntities--;
        source.Stop();
        Invoke(nameof(ReturnToPool), 6);
        anim.SetBool("alive", alive);
        switch (shotFromWhere)
        {
            case BodyPart.Part.Head:
                anim.SetTrigger("fallingBack");
                break;
            case BodyPart.Part.Torso:
                anim.SetTrigger("fallingBack");
                break;
            case BodyPart.Part.Limb:
                anim.SetTrigger("fallingBack");
                break;
        }

        if(!hasExplodedSinceSpawn)
        Invoke(nameof(Explode), 2);





    }

    public override void Despawn()
    {
        base.Despawn();
        CancelInvoke(nameof(Die));
        hasExplodedSinceSpawn = false;
    }

    public override void ZombieAttackMoment()
    {
        if (!gameplayManager.levelCompleted && !gameplayManager.afterDeath)
            Explode();
    }

}
