using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public Pickup pickup;

    private void Start()
    {
        Collider alertArea = GameObject.Find("AlertArea").GetComponent<Collider>();

        for (int i = 0; i < GetComponents<Collider>().Length; i++)
        {
            Physics.IgnoreCollision(GetComponents<Collider>()[i], alertArea);
        }
    }
}
