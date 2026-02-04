using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingZombie : ZombieTemp
{
    public override void WhenInAttackRange()
    {
        RotateToTarget();
    }
}
