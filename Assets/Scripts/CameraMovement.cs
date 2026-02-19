using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CameraMovement : MonoBehaviour
{
    //default sens = 0.1, 0.1
    public Slider sensSlider;
    public FixedTouchField touchField;
    public Transform player_cam, center_point;
    public float cameraAngleX, cameraAngleHorizontalSpeed = 0.1f , cameraAngleVerticalSpeed = 0.1f, minHeight, maxHeight;
    public static float height;
    [SerializeField]
    int cameraTouchCount;


    public bool is_lookingat;
    float rotX, rotY, deltaX, deltaY;
    Vector3 origRot;

    Touch initTouch = new Touch();

    public Transform aimTarget;
    bool pc;

     void Awake()
    {

    }

    void Start()
    {
        pc = GameplayManager.instance.pc;
        if(pc)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        origRot = transform.eulerAngles;
        rotX = origRot.x;
        rotY = origRot.y;
        cameraAngleX = player_cam.eulerAngles.x;

        sensSlider.value = PlayerPrefs.GetFloat("sensitivity");
        cameraAngleHorizontalSpeed = 0.05f + sensSlider.value / 40;
        cameraAngleVerticalSpeed = 0.05f + sensSlider.value / 40;
    }

    void Update()
    {


      


        if(!pc) //android
        player_cam.eulerAngles -= new Vector3(touchField.TouchDist.y * cameraAngleVerticalSpeed, 0, 0);
        else //pc or browser
        player_cam.eulerAngles -= new Vector3(Input.GetAxis("Mouse Y") * 10 * cameraAngleVerticalSpeed, 0, 0);
        //aimTarget.RotateAround(transform.position, Vector3.up, touchField.TouchDist.y * cameraAngleVerticalSpeed);





        player_cam.eulerAngles = new Vector3(ClampAngle(player_cam.eulerAngles.x, minHeight, maxHeight), player_cam.eulerAngles.y, player_cam.eulerAngles.z);

        if(!pc)
        transform.eulerAngles += new Vector3(0, touchField.TouchDist.x * cameraAngleHorizontalSpeed, 0);
        else
            transform.eulerAngles += new Vector3(0, Input.GetAxis("Mouse X") * 10 * cameraAngleHorizontalSpeed, 0);




        if (IsPointerOverGameObject())
            return;  
    }

     void FixedUpdate()
    {
        
    }

    void LateUpdate()
    {
   

    }


    public static float ClampAngle(float current, float min, float max)
    {
        float dtAngle = Mathf.Abs(((min - max) + 180) % 360 - 180);
        float hdtAngle = dtAngle * 0.5f;
        float midAngle = min + hdtAngle;

        float offset = Mathf.Abs(Mathf.DeltaAngle(current, midAngle)) - hdtAngle;
        if (offset > 0)
            current = Mathf.MoveTowardsAngle(current, midAngle, offset);
        return current;
    }

    public static bool IsPointerOverGameObject()
    {

        if (EventSystem.current.IsPointerOverGameObject(0) || EventSystem.current.IsPointerOverGameObject())
            return true;
        return false;
    }

    public void OnSensitivitySliderChange()
    {
        PlayerPrefs.SetFloat("sensitivity", sensSlider.value);
        cameraAngleHorizontalSpeed = 0.05f + sensSlider.value / 40;
        cameraAngleVerticalSpeed = 0.05f + sensSlider.value / 40;
    }

}
