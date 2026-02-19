using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameplayManager;

public enum Map
{
    UncannyDesert
}

public enum DaySkybox
{
    Night,
    Cloudy,
    Nothingness
}


[CreateAssetMenu(fileName = "UnnamedLevelProperties", menuName = "New Level Properties")]
public class LevelProperties : ScriptableObject
{
    public int level;
    public Map map;
    public DaySkybox weather;
    public int zombiesToKillToComplete;
    public int maxZombiesAtOnce;
    public float timeAfterLastKillToTriggerAZombie;
    [Range(0, 7)]
    public int crates;
    [Range(0, 11)]
    public int beartraps;
    [SerializeField]
    public ZombieSpawner[] spawners;


}


