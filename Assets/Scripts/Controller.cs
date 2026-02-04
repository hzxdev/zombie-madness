using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour, IDataPersistence
{
    public Slider speedSlider;
    public Text speedText;
    public Camera playerCam;
    public DitzeGames.MobileJoystick.Joystick joystick;
    public FixedButton fireButton;
    public Vector2 moveAxis;
     float horizontalInput, verticalInput;
    public float normalSpeed = 4.25f, jumpPower = 5, stunnedSpeed = 2; //The run animation average velocity is about 2.3f, this doesnt have to be that though

    
    public float currentSpeed;
    Animator anim;
    Rigidbody rigid;
    GunController gunController;
    GameplayManager gameplayManager;
    CapsuleCollider playerCollider;
   // [HideInInspector]
    public bool isMoving;
    [HideInInspector]
    public float moveVectorMagnitude;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float sphereValue;
    [SerializeField]
     float velocityY;
    float distanceToGround;
    [HideInInspector]
    public bool alive;

    bool editor;
     bool playingFromPC;

    void Start()
    {
        playingFromPC = GameplayManager.instance.pc;

        playerCollider = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        gunController = GetComponent<GunController>();
        gameplayManager = GameplayManager.instance;

        if (Application.platform == RuntimePlatform.WindowsEditor)
            editor = true;
    }


    void Update()
    {
        anim.SetBool("isGrounded", IsGrounded());
        anim.SetFloat("distanceToGround", DistanceToGround());
        Debug.DrawRay(groundCheck.position, -groundCheck.up);


        velocityY = rigid.velocity.y;
        anim.SetFloat("velocityY", velocityY);

        if(!playingFromPC)
        {
            horizontalInput = joystick.AxisNormalized.x;
            verticalInput = joystick.AxisNormalized.y;
        } else
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
        }
         
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            JumpButtonPressed();
        }
       



        moveAxis = new Vector2(horizontalInput, verticalInput);
        anim.SetFloat("horizontal", horizontalInput, 1, Time.deltaTime * 10);
        anim.SetFloat("vertical", verticalInput, 1, Time.deltaTime * 10);
        //  gunController.moveVectorMagnitude = new Vector2(horizontalInput, verticalInput).magnitude;
        //gunController.moveVectorMagnitude = (Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput)) / 2;
        moveVectorMagnitude = Mathf.Clamp01(new Vector2(horizontalInput, verticalInput).magnitude);

        //rigid.velocity = transform.InverseTransformDirection(new Vector3(Vector3.right.x * runAnimAverageVelocity * joystick.Horizontal, 0, Vector3.forward.z * runAnimAverageVelocity * joystick.Vertical));
        if(horizontalInput > 0 || verticalInput > 0)
        {
            isMoving = true;
        } else
        {
            if(!joystick.Pressed)
            isMoving = false;
        }
        var locVel 
            = transform.InverseTransformDirection(rigid.velocity);

        if (alive && !gameplayManager.levelCompleted)
        {
           
           

            if(verticalInput > 0) //player should go slower backwards
            {
                locVel.z = currentSpeed * verticalInput;
                locVel.x = currentSpeed * horizontalInput;
            } else
            {
                locVel.z = currentSpeed * verticalInput * 5 / ( 6 + Mathf.Abs(verticalInput) );
                locVel.x = currentSpeed * horizontalInput * 5 / (6 + Mathf.Abs(verticalInput));
            }
            
           
                
            

            rigid.velocity = transform.TransformDirection(locVel);
        } else
        {
            rigid.velocity = transform.TransformDirection(new Vector3(0, locVel.y, 0));
        }
       

       // if (velocityY < 0 && IsGonnaLand())
         //   anim.SetTrigger("landed");


    }

    void LateUpdate()
    {
        if(editor )
        {
           
        }
     
    }

    public void JumpButtonPressed()
    {
        if(IsGrounded())
        {
            Jump();
        }
       
    }

    void Jump()
    {
        // rigid.velocity += new Vector3(0, currentSpeed, 0);
        rigid.AddForce(currentSpeed * 100 * transform.up, ForceMode.Impulse);
        anim.SetTrigger("jump");
        lastFS++;
        if (lastFS > 4)
            lastFS = 1;
        AudioManager.instance.PlayOneShot("footstep" + lastFS.ToString());
        Tutorial.instance.Jumped();
    }

   
    bool IsSphereGrounded() {

        Collider[] hitColliders = Physics.OverlapSphere(groundCheck.position, sphereValue, groundLayer, QueryTriggerInteraction.Ignore);
        foreach (Collider col in hitColliders)
        {
            if (col.transform.root != transform)
            {
                return true;
            }
        }
        return false;
    }
    

        float DistanceToGround()
    {
        RaycastHit hit;
        if( Physics.Raycast(groundCheck.position, -groundCheck.up, out hit, 100))
        {
            return Vector3.Distance(groundCheck.position, hit.point);
        }
        return 0;
    }

    bool IsRaycastGrounded()
    {
        if(DistanceToGround() < 0.05f)
        {
            return true;
        }
        return false;
    }

    bool IsGrounded()
    {
        if (IsSphereGrounded())
        {
            return true;
        } else if(IsRaycastGrounded())
        {
            return true;
        }
        return false;
    }



    int lastFS = 0;
    public void FootstepSound()
    {
        lastFS++;
        if (lastFS > 4)
            lastFS = 1;
        if (Mathf.Abs(horizontalInput) > 0.2f || Mathf.Abs(verticalInput) > 0.2f)
        {
            AudioManager.instance.PlayOneShot("footstep" + lastFS.ToString());
        }
        
            
    }

    public void JumpLanded()
    {
        if(lastFS % 2 == 0)
        {
            AudioManager.instance.PlayOneShot("land1");
        } else
        {
            AudioManager.instance.PlayOneShot("land2");
        }
    }

        private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(groundCheck.position, sphereValue);        
    }

    public void LoadData(GameData data)
    {
        normalSpeed = WeaponsList.instance.GetCharacterByName(data.equippedCharacter).speed;
        stunnedSpeed = normalSpeed / 4;
        
    }

    public void SaveData(GameData data)
    {
        
    }
}

