using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static GameplayManager;

public class PistolZombie : ZombieTemp, IPoolable
{
    public float shootInterval, shootTimer;


    public override void Start()
    {
        alive = true;


        health = maxHealth;

        anim = GetComponent<Animator>();
        anim.SetBool("alive", alive);
        anim.SetFloat("runningAnimSpeedMultp", 1);

        agent = GetComponent<NavMeshAgent>();
        source = GetComponent<AudioSource>();
        zombieSoundRandomTime = Random.Range(zombieRandomTimeMin, zombieRandomTimeMax);
        target = GameObject.FindGameObjectWithTag("Player").transform;
        gameplayManager = GameplayManager.instance;
        objectPooler = GameObject.FindObjectOfType<ObjectPooler>();
        rigid = GetComponent<Rigidbody>();

        StartWandering();
        Initialize();
    }
    public override void Initialize()
    {
        if (currentState == ZombieState.Chasing)
            agent.speed = zombieScriptable.chasingSpeed;
        else if (currentState == ZombieState.Wandering)
            agent.speed = zombieScriptable.wanderingSpeed;
        maxHealth = zombieScriptable.health;
        health = maxHealth;
        damageMin = zombieScriptable.damageMin;
        damageMax = zombieScriptable.damageMax;

        // anim.runtimeAnimatorController = zombieScriptable.animOverride;
        // Different animator than zombies
        setTargetPeriod = zombieScriptable.setTargetPeriod;

    }

    public override void WhenInAttackRange()
    {
        anim.SetTrigger("shoot");
        anim.SetBool("isStopped", true);
        agent.isStopped = true;
        RotateToTarget();
    }


    public override void WhenOutAttackRange()
    {
        anim.SetBool("isStopped", false);
        agent.isStopped = false;

    }

    public override void Spawn()
    {
        base.Spawn();
        shootTimer = 0;
    }

    public override void Update()
    {
        base.Update();

        if(agent.isStopped)
        shootTimer += Time.deltaTime;

        if(shootTimer >= shootInterval && agent.isStopped)
        {
            Shoot();
            shootTimer = 0;
        }
    }

    void Shoot()
    {

    }
}
