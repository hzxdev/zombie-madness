using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum ZombieType
{
   RegularZombie,
   ZombieGirl,
   OrcZombie,
   ExplosiveZombie,
   Wolf,
   SuitZombie,
   CommanderZombie,
   ParasiteZombie,
   PistolZombie,
   BomberZombie
}

[CreateAssetMenu(fileName = "UnnamedZombie", menuName = "New Zombie")]
public class ZombieSO : ScriptableObject
{
    public string zombieName;
    public ZombieType zombieType;
    public AnimatorOverrideController animOverride;

    public float wanderingSpeed, chasingSpeed;
    public int health;
    public int damageMin, damageMax;
    public float setTargetPeriod;
    public float chasingStamina; // gets tired after this amount of time

    public int killedCoin;


}