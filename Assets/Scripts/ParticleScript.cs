using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    public float destroyAfter;
    public bool followPlayer;
     Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        Destroy(gameObject, destroyAfter);
    }

    void Update()
    {
        if (!followPlayer)
            return;

        transform.position = player.position;
    }
}
