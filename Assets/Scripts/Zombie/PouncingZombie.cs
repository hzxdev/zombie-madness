using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouncingZombie : ZombieTemp
{
    bool isPouncing;
    public override void WhenInAttackRange()
    {
        anim.SetBool("isStopped", true);
       
        
        if(!isPouncing)
        {
            RotateToTarget();
            agent.isStopped = true;
            isPouncing = true;
        }
    }

    public  override void Initialize()
    {
        if (currentState == ZombieState.Chasing)
            agent.speed = zombieScriptable.chasingSpeed;
        else if (currentState == ZombieState.Wandering)
            agent.speed = zombieScriptable.wanderingSpeed;
        maxHealth = zombieScriptable.health;
        health = maxHealth;
        damageMin = zombieScriptable.damageMin;
        damageMax = zombieScriptable.damageMax;
        setTargetPeriod = zombieScriptable.setTargetPeriod;
    }
}
