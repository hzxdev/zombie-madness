using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static GameplayManager;

public class Wolf : ZombieTemp, IPoolable
{

    public bool isAttacking;
    public bool playerColliding;
    public float jumpForwardForce, jumpUpForce;

    public override void Start()
    {
        alive = true;


        health = maxHealth;

        anim = GetComponent<Animator>();
        anim.SetBool("alive", alive);

        agent = GetComponent<NavMeshAgent>();
        source = GetComponent<AudioSource>();
        zombieSoundRandomTime = Random.Range(zombieRandomTimeMin, zombieRandomTimeMax);
        target = GameObject.FindGameObjectWithTag("Player").transform;
        gameplayManager = GameplayManager.instance;
        objectPooler = GameObject.FindObjectOfType<ObjectPooler>();
        rigid = GetComponent<Rigidbody>();


        Initialize();
    }

    public override void Initialize()
    {
            agent.speed = zombieScriptable.chasingSpeed;
        maxHealth = zombieScriptable.health;
        health = maxHealth;
        damageMin = zombieScriptable.damageMin;
        damageMax = zombieScriptable.damageMax;

        // anim.runtimeAnimatorController = zombieScriptable.animOverride;
        // Different animator than zombies
        setTargetPeriod = zombieScriptable.setTargetPeriod;

    }

    public override void Update()
    { 
        if (!alive)
            return;

        DoRandomSounds();     

        if(!isAttacking) //perfectly follow player until its time to attack 
            SetPlayerAsTarget();

            if (Vector3.Distance(transform.position, target.position) <= agent.stoppingDistance)
            {
                WhenInAttackRange();
            } else
        {
            WhenOutAttackRange();
        }

    }

    public override void HurtAnim()
    {
        if(!anim.GetCurrentAnimatorStateInfo(0).IsName("attack2"))
        anim.SetTrigger("hit");
    }

    public override void WhenInAttackRange()
    {
        anim.SetTrigger("attack");
        agent.isStopped = true;
        RotateToTarget();
    }


    public override void WhenOutAttackRange()
    {
        
        agent.isStopped = false;
       
    }

    public override void SlowDown(float duration)
    {
        agent.speed = 0;
        CancelInvoke(nameof(base.StopSlowDown));
        Invoke(nameof(base.StopSlowDown), 1);
    }


    public void JumpStart()
    {
        if(alive)
        {
            if(!playerColliding)
            {
                rigid.AddForce(transform.forward * jumpForwardForce);
                rigid.AddForce(transform.up * jumpUpForce);
                AudioManager.instance.PlayOneShot("wolf_attack");
                isAttacking = true;
            } else
            {
                base.ZombieAttackMoment(); //deal damage

                if (!gameplayManager.levelCompleted)
                {
                    AudioManager.instance.PlayOneShot("wolf_attack");
                    GunController.instance.Stun(3);
                    Die(BodyPart.Part.Torso, true);
                    CancelInvoke(nameof(ReturnToPool));
                    ReturnToPool();
                }
            }
            
        }
       


        // base.ZombieAttackMoment();

        /*
        Die(BodyPart.Part.Torso, true);
        CancelInvoke(nameof(ReturnToPool));
        ReturnToPool();
        */

        
    }

    public override void DoRandomSounds()
    {
        
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.transform.CompareTag("Player"))
        {
            playerColliding = true;
            if (isAttacking && playerColliding && alive)
            {
                base.ZombieAttackMoment(); //deal damage

                if (!gameplayManager.levelCompleted)
                {
                    GunController.instance.Stun(3);
                    Die(BodyPart.Part.Torso, true);
                    CancelInvoke(nameof(ReturnToPool));
                    ReturnToPool();
                }
                   
            }

        }


        if (col.transform.CompareTag("Beartrap"))
        {
            if (!col.transform.GetComponent<Beartrap>().isClosed)
            {
                SlowDown(4);
                TakeDamage(35, BodyPart.Part.Limb, col.transform.position, Quaternion.identity, false);
            }

            col.transform.GetComponent<Beartrap>().Close();
        }

    }

    void OnCollisionExit(Collision col)
    {
        if (col.transform.CompareTag("Player"))
        {
            playerColliding = false;
        }

    }


    public void AttackEnd()
    {
        isAttacking = false;
    }

    public override void Despawn()
    {
        CancelInvoke(nameof(StartChasing));
        playerColliding = false;
        isAttacking = false;

        Debug.Log("Zombie Despawn()");
    }

    public override void TakeDamage(int damage, BodyPart.Part part, Vector3 effectPos, Quaternion effectRot, bool alertNearby)
    {
        base.TakeDamage(damage, part, effectPos, effectRot, alertNearby);
        isAttacking = false;
    }

}
