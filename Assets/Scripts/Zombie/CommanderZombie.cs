using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameplayManager;
using Random = UnityEngine.Random;

public enum CommanderZombieMode
{
    Mode1,
    Mode2,
}

public class CommanderZombie : ZombieTemp, IPoolable
{
    public CommanderZombieMode mode;
    public float resurrectingPeriod = 5, extraTime = 0.25f;
    float resurrectTime;
    public GameObject regularZombiePrefab, zombieGirlPrefab, orcZombiePrefab, parasiteZombie, wolfPrefab, bomberPrefab, resurrectionEffect;
    public Transform spawnPoint1, spawnPoint2;
    ParticleSystem rs1, rs2;
    Vector3 pos1, pos2;
    bool hasSpawned;
    public LayerMask layerMask;

    // Start is called before the first frame update
  

    // Update is called once per frame
    public override void Start()
    {
        base.Start();
        if(gameplayManager.currentLevelId > 24)
        {
            mode = CommanderZombieMode.Mode2;
        } else
        {
            mode = CommanderZombieMode.Mode1;
        }
    }

    public override void Update()
    {
        base.Update();

        if (hasSpawned)
            return;

        resurrectTime += Time.deltaTime;
        if(resurrectTime >= resurrectingPeriod)
        {
            SpawnParticles();
            resurrectTime = 0;
            hasSpawned = true;
        }
    }

    void SpawnParticles()
    {
       GameObject go1 =  Instantiate(resurrectionEffect, spawnPoint1.position, Quaternion.identity);
        GameObject go2 = Instantiate(resurrectionEffect, spawnPoint2.position, Quaternion.identity);
        Invoke(nameof(ResurrectZombies), extraTime);
        pos1 = go1.transform.position;
        pos2 = go2.transform.position;
    }

    void ResurrectZombies()
    {
        int r = Random.Range(0, 3);
        if (mode == CommanderZombieMode.Mode1) //light
        {
            if (r == 0)
            {
                if (!Physics.CheckSphere(pos1, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(regularZombiePrefab, pos1, Quaternion.identity);
                if (!Physics.CheckSphere(pos2, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(regularZombiePrefab, pos2, Quaternion.identity);
            }

            if (r == 1)
            {
                if (!Physics.CheckSphere(pos1, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(zombieGirlPrefab, pos1, Quaternion.identity);
                if (!Physics.CheckSphere(pos2, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(zombieGirlPrefab, pos2, Quaternion.identity);
            }
            if (r == 2)
            {
                if (!Physics.CheckSphere(pos1, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(orcZombiePrefab, pos1, Quaternion.identity);
                if (!Physics.CheckSphere(pos2, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(orcZombiePrefab, pos2, Quaternion.identity);
            }
        }
        if (mode == CommanderZombieMode.Mode2) //hard
        {
            if (r == 0)
            {
                if (!Physics.CheckSphere(pos1, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(parasiteZombie, pos1, Quaternion.identity);
                if (!Physics.CheckSphere(pos2, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(parasiteZombie, pos2, Quaternion.identity);
            }

            if (r == 1)
            {
                if (!Physics.CheckSphere(pos1, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(wolfPrefab, pos1, Quaternion.identity);
                if (!Physics.CheckSphere(pos2, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(wolfPrefab, pos2, Quaternion.identity);
            }
            if (r == 2)
            {
                if (!Physics.CheckSphere(pos1, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(bomberPrefab, pos1, Quaternion.identity);
                if (!Physics.CheckSphere(pos2, 0.1f, layerMask, QueryTriggerInteraction.Ignore))
                    gameplayManager.Spawn(bomberPrefab, pos2, Quaternion.identity);
            }
        }


        /*if (r == 3)
        {
            gameplayManager.Spawn(parasiteZombie, pos1, Quaternion.identity);
            gameplayManager.Spawn(parasiteZombie, pos2, Quaternion.identity);
        }*/
    }

    public override void Die(BodyPart.Part shotFromWhere, bool killedByGame)
    {
        base.Die(shotFromWhere, killedByGame);
        SpawnParticles();
    }

    public override void Spawn()
    {
        base.Spawn();
        hasSpawned = false;
    }
}
