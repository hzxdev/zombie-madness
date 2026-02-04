using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    public float speed = 50;
    void Start()
    {
        Invoke("Destroy", 0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime);
    }

     void Destroy()
    {
        Destroy(gameObject);    
     }
}
