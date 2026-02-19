using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootTrigger : MonoBehaviour
{
    
     Controller controller;
     GameObject player;
    public FootTrigger otherTrigger;
    public bool isThisFootGrounded;

    void Start()
    {
        controller = GetComponentInParent<Controller>();
        player = controller.gameObject;
    }

    
    void Update()
    {
        
    }

     void OnTriggerEnter(Collider collision)
    {
        isThisFootGrounded = true;
        //controller.isGrounded = true;
    }

    void OnTriggerStay(Collider collision)
    {
        isThisFootGrounded = true;

        //controller.isGrounded = true;
    }

    void OnTriggerExit(Collider collision)
    {
        isThisFootGrounded = false;
      //  if(!otherTrigger.isThisFootGrounded)
        //    controller.isGrounded = false;
    }
}
