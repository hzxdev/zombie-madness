using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public float speed;

    // Update is called once per frame
    void Update()
    {
        // transform.RotateAround(transform.position, transform.up, speed);
        transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0));
    }
}
