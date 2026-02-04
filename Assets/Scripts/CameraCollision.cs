using UnityEngine;
using System.Collections;

public class CameraCollision : MonoBehaviour
{

    RaycastHit hit, hit2;
    public Transform playerPoint, defaultTransform;
    public float zPlus;
    public LayerMask blockersMask;

    // Use this for initialization
    private void Start()
    {
 
    }

    void Update()
    {


     
        if (Physics.Linecast(playerPoint.position, defaultTransform.position, out hit, blockersMask))
        {
            //This gets executed if there's any collider in the way
            Vector3 a = transform.InverseTransformPoint(hit.point);
            if(Physics.Linecast(playerPoint.position, new Vector3(transform.localPosition.x, transform.localPosition.y, a.z + zPlus), out hit2, blockersMask))
            {
                transform.position = Vector3.Lerp(transform.position, hit.point, Time.deltaTime * 8);
            } else
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, transform.localPosition.y, a.z + zPlus), Time.deltaTime * 8);
            }
            
            


        } else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, defaultTransform.localPosition, Time.deltaTime * 4);
        }
     //   Debug.DrawLine(playerPoint.position, defaultTransform.position, Color.black);
    }

    

    
    
  
}

