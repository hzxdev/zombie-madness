using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameplayManager;

public class SuitZombie : ZombieTemp, IPoolable
{
    public GameObject tpMarker, teleportEffect;
    public Transform tpMarkerSp;
    Vector3 lastMarkerPos;
    public AudioClip tpSound;
    public ParticleSystem hitTPEffect;

    public override void Spawn()
    {
        base.Spawn();
        Invoke(nameof(PlaceTPMarker), Random.Range(2, 5));
    }

    private void PlaceTPMarker()
    {
        lastMarkerPos = tpMarker.transform.position;

    }
    public override void Die(BodyPart.Part shotFromWhere, bool killedByGame)
    {
        base.Die(shotFromWhere, killedByGame);
        CancelInvoke(nameof(PlaceTPMarker));
    }

    public override void TakeDamage(int damage, BodyPart.Part part, Vector3 effectPos, Quaternion effectRot, bool alertNearby)
    {
        base.TakeDamage(damage, part, effectPos, effectRot, alertNearby);
         hitTPEffect.Play();
        
            Teleport();
        
    }
    public void Teleport()
    {
        transform.position = instance.spawnpoints[Random.Range(0, instance.spawnpoints.Length)].transform.position;
        
       
        AudioManager.instance.PlayOneShot("suittp");
        
    }
}
