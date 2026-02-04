using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Bot : MonoBehaviour
{
    public enum BotPattern
    {
        Idle,
        PingPongHorizontal,
        PingPongVertical,
        TowardsPlayerVertical
    }

    GameplayManager gameplayManager;
    public Weapon weapon;
    public Transform weaponsParent;
    public GameObject[] weaponGOs;
    GameObject currentWeaponObject;
    public BotPattern botPattern;
    public float pingPongValue, pingPongDuration, speed;
    public float maxHealth = 100;
     Animator killEffectAnim;
    public TwoBoneIKConstraint leftArmRig, rightArmRig;
    public GameObject fleshSmallImpactGO, fleshBigImpactGO;
    float health;
    Animator anim;
    Rigidbody rigid;
    bool goingLeft = true, goingRight = false, alive;
    bool goingBack = true, goingForward = false;

    void Start()
    {
        alive = true;
        gameplayManager = GameplayManager.instance;
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        killEffectAnim = GameObject.Find("KillEffect").GetComponent<Animator>();
        anim.SetBool("alive", alive);
        health = maxHealth;
        
        for (int i = 0; i < weaponGOs.Length; i++)
        {
            weaponGOs[i].SetActive(false);
        }
        currentWeaponObject = weaponsParent.GetChild(weapon.weaponId).gameObject;
        currentWeaponObject.SetActive(true);


        if(botPattern == BotPattern.PingPongHorizontal)
        StartHorizontalPingPong();
        if (botPattern == BotPattern.PingPongVertical)
            StartVerticalPingPong();
    }

    public void StartHorizontalPingPong()
    {
        if (!alive)
            return;
        if (goingRight)
        {
            goingRight = false;
            goingLeft = true;
            anim.SetFloat("horizontal", -1);
            Invoke(nameof(StartHorizontalPingPong), pingPongDuration);
            return;
            
        }
        if (goingLeft)
        {
            goingRight = true;
            goingLeft = false;
            anim.SetFloat("horizontal", 1);
            
            Invoke(nameof(StartHorizontalPingPong), pingPongDuration);
            return;

        }
    }

    public void StartVerticalPingPong()
    {      
        if (!alive)
            return;
        if (goingForward)
        {
            goingForward = false;
            goingBack = true;
            anim.SetFloat("vertical", -1);
            
            Invoke(nameof(StartVerticalPingPong), pingPongDuration);
            return;

        }
        if (goingBack)
        {
            goingForward = true;
            goingBack = false;
            anim.SetFloat("vertical", 1);

            Invoke(nameof(StartVerticalPingPong), pingPongDuration);
            return;

        }
    }


    void Update()
    {
        if (!alive)
            return;

        if(botPattern == BotPattern.PingPongHorizontal)
        {
            if (goingLeft)
                rigid.velocity = transform.TransformDirection(new Vector3(-speed, 0, 0));
            if (goingRight)
                rigid.velocity = transform.TransformDirection(new Vector3(speed, 0, 0));
        }

        if (botPattern == BotPattern.PingPongVertical)
        {
            if (goingBack)
                rigid.velocity = transform.TransformDirection(new Vector3(0, 0, -speed));
            if (goingForward)
                rigid.velocity = transform.TransformDirection(new Vector3(0, 0, speed));
        }

           
    }


    public void TakeDamage(int damage, BodyPart.Part part, Vector3 effectPos, Quaternion effectRot)
    {

        if(part == BodyPart.Part.Head)
        {
            GameObject impactGO = Instantiate(fleshBigImpactGO, effectPos, effectRot);
            Destroy(impactGO, 3);
        } else
        {
            GameObject impactGO = Instantiate(fleshSmallImpactGO, effectPos, effectRot);
            Destroy(impactGO, 3);
        }
            
        
        if (alive)
        {
            health -= damage;
            anim.SetTrigger("hit");
            if (health <= 0)
            {
                Die(part);
                health = 0;
            }
        }
       
    }

    void Die(BodyPart.Part shotFromWhere)
    {
        alive = false;
        gameplayManager.AddKill();


        anim.SetBool("alive", alive);
        anim.SetLayerWeight(1, 0);
        switch (shotFromWhere)
        {
            case BodyPart.Part.Head:
                anim.SetTrigger("fallingBack");
                break;
            case BodyPart.Part.Torso:
                anim.SetTrigger("fallingBack");
                break;
            case BodyPart.Part.Limb:
                anim.SetTrigger("fallingBack");
                break;
        }

        switch (botPattern)
        {
            case BotPattern.Idle:
                break;
            case BotPattern.PingPongHorizontal:
                CancelInvoke(nameof(StartHorizontalPingPong));
                break;
            case BotPattern.PingPongVertical:
                CancelInvoke(nameof(StartVerticalPingPong));
                break;
            case BotPattern.TowardsPlayerVertical:
                break;
           
        }
        DropWeapon(weapon);
        currentWeaponObject.SetActive(false);
        
        rightArmRig.weight = 0;
        leftArmRig.weight = 0;
    }

    void DropWeapon(Weapon weaponToBeDropped)
    {
        Instantiate(weaponToBeDropped.droppedPrefab, currentWeaponObject.transform.position, Quaternion.identity);
    }

}