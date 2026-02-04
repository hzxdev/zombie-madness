using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;
using System.Linq;
using TMPro;

public class GunController : MonoBehaviour, IDataPersistence
{
    bool pc;

    [HideInInspector]
    public static GunController instance;

    
  //  WeaponSlot currentActiveSlot;
    public Transform weaponsParent, nonIKWeaponsParent;
     GameObject[] weaponGOs, nonIKWeaponGOs;
    public string desiredWeaponName;
    public Weapon defaultMeleeWeapon;
    GameObject currentWeaponObject;
   
    public Transform playerCam, weaponDropPoint, aimHitPoint, currentGunShootRayPoint, redCrossIcon;
    public GameObject bulletImpactGO, weaponPickupBulletParticle, sandImpactParticle;
    public ParticleSystem muzzleFlash, cartridgeEject;
    Camera playerCamComponent;
    public AudioSource wepAudioSource;
    public Animator gunAnim;

    public LayerMask gunHitScanDetectLayer;
    PlayerHealth playerHealth;
    Animator playerAnim;
    CameraMovement cameraMovement;
    public float gunCoolDown, gunRange, standingSpread, movingSpread, standingSpreadTimeMultiplier, movingSpreadTimeMultiplier;
    float currentSpread;
    float timeStamp;
    public bool shootPressed, isShooting;
    public DitzeGames.MobileJoystick.FireJoystick fireJoystick;
    public RectTransform reticle, weaponIconRect;
    public Image currentWeaponIcon;
    public float reticleSize;
    float moveVectorMagnitude;
    bool isMoving, reloadingAnimationStarted, reloadRigIsLerping, gunHitscanBlocked;
    public bool gunBarrelNearTrigger;
    public bool meleeAttacking;
    [HideInInspector]
    public bool isReloading, isStunned;

    public GameObject turretButton, turretPurchasePanel;
    public TextMeshProUGUI turretPriceText, turretName;
    Turret lastInteractedTurret;

    //HELPER TEXTS
    public GameObject outOfAmmoHelperText;

    Controller controller;
    //public Rig leftArmRig, rightArmRig;
    //  public WeaponIKTarget leftArmIKTarget rightArmIKTarget;
    public WeaponIKTarget leftArmTarget, leftArmHint;
    public TwoBoneIKConstraint leftHandRig;
    public MultiAimConstraint rightHandRig, upperBodyRig;
    public Text ammoText, weaponNameDisplay;
    //  [HideInInspector]
    bool isThrowingProjectile;

    Transform lastBombTransform;
    bool isBombIndicatorActive;
    public GameObject dotCrosshair, stunnedPanel, exchangeWeaponButton;
    public RectTransform bombIndicator, hazardSign;
    public GameObject stunnedParticle;
    public GameObject thrownGrenadeObj;
    public Material stunnedCamMat;
    public Color stunnedCamMatColor;
    public Animator stunnedAnim;
    float stunTimer;

    public RectTransform chLeftLine, chRightLine, chTopLine, chBottomLine;
    public float verticalRecoil, horizontalRecoil, duration, crossHairMinWidth, crossHairMaxWidth;
    float time, standShootReturnTimerCh, movingShootReturnTimerCh, shootingTime, crossHairLineLength;

    bool  weaponSlotTouchStillHolding, isTouchOnWeaponSlot, weaponSlotTouchDown;

    bool throwNear;
    public float grenadeFarForwardForce, grenadeFarUpForce, grenadeNearForwardForce, grenadeNearUpForce;
    public GameObject throwNearButton, reloadButton;

    public WeaponSlot[] slots;
    public WeaponPickupTrigger wpt;

    //the slot we start with (melee in this case)
    [HideInInspector]
    public int currentSlotId = 0;
    RaycastHit rayHitFromGunsBarrel;
    GameplayManager gameplayManager;
    bool isAndroid;

    public Sprite shootImg, stabImg, grenadeImg;
    public Image fireJoystickHandle;

    float touchingWeaponSlotTime;
    Weapon exchangeWep;
    GameObject lastEnteredWepPickup;
    public LayerMask shootingLayerMask;

    void Awake()
    {
        if (instance == null)
            instance = this;

        pc = GameplayManager.instance.pc;
    }

    
    public void LoadData(GameData data)
    {

        for (int i = 0; i < WeaponsList.instance.weapons.Length; i++)
        {
            if (!DataPersistenceManager.instance.gameData.weaponsAmmo.ContainsKey(WeaponsList.instance.weapons[i].weaponName))
                DataPersistenceManager.instance.gameData.weaponsAmmo.Add(WeaponsList.instance.weapons[i].weaponName, 0);
        }

        for (int i = 0; i < WeaponsList.instance.weapons.Length; i++)
        {

           
                if (data.weaponsAmmo.Values.ElementAt(i) > 0)
                {

                WeaponSlot slot = GetFirstSlotWithTheType(WeaponTypeToSlotType(WeaponsList.instance.GetWeaponByName(data.weaponsAmmo.Keys.ElementAt(i)).weaponType));
                    slot.unloadedAmmo = data.weaponsAmmo.Values.ElementAt(i);
                slot.weapon = WeaponsList.instance.GetWeaponByName(data.weaponsAmmo.Keys.ElementAt(i));

                int ammoDifference = slot.weapon.maxMagAmmo - slot.magAmmo;
                if (ammoDifference > slot.unloadedAmmo)
                {
                    slot.magAmmo += slot.unloadedAmmo;
                    slot.unloadedAmmo = 0;
                }
                else
                {
                    slot.unloadedAmmo -= ammoDifference;
                    slot.magAmmo = slot.weapon.maxMagAmmo;
                }


            }


            

        }

        

    }

    public void SaveData(GameData data)
    {
        for (int i = 0; i < WeaponsList.instance.weapons.Length; i++) // reset
        {
            data.weaponsAmmo[WeaponsList.instance.weapons[i].weaponName] = 0;
        }

        for (int i = 0; i < WeaponsList.instance.weapons.Length; i++)
        {
           
                for (int k = 0; k < slots.Length; k++)
                {
                if(slots[k].weapon != null)
                {
                    if (data.weaponsAmmo.Keys.ElementAt(i) == slots[k].weapon.weaponName && slots[k].magAmmo + slots[k].unloadedAmmo > 0)
                    {
                        
                        data.weaponsAmmo[slots[k].weapon.weaponName] = slots[k].unloadedAmmo + slots[k].magAmmo;

                    }
                }

               
                }
            
        }

        Debug.Log("gun controller saved");
    }

    void Start()
    {

        //Get weapon gameobjects in the right hand
        weaponsParent = GetComponent<CharacterManager>().currentCharInfo. weaponsParent;
        nonIKWeaponsParent = GetComponent<CharacterManager>().currentCharInfo.nonIKWeaponsParent;
        weaponGOs = new GameObject[weaponsParent.childCount];
        for (int i = 0; i < weaponsParent.childCount; i++)
        {
            weaponGOs[i] = weaponsParent.GetChild(i).gameObject;
        }

        nonIKWeaponGOs = new GameObject[nonIKWeaponsParent.childCount];
        for (int i = 0; i < nonIKWeaponsParent.childCount; i++)
        {
            nonIKWeaponGOs[i] = nonIKWeaponsParent.GetChild(i).gameObject;
        }

        playerHealth = GetComponent<PlayerHealth>();
        playerAnim = GetComponent<Animator>();
        playerCamComponent = playerCam.GetComponent<Camera>();
        controller = GetComponent<Controller>();
        currentSpread = standingSpread;
        cameraMovement = GetComponent<CameraMovement>();
        gameplayManager = GameplayManager.instance;
        controller.currentSpeed = controller.normalSpeed;

       



        if (gameplayManager.currentLevelId == 0)
        {
            AddWeaponWithDefaultValues(defaultMeleeWeapon.weaponName);
            currentSlotId = 1;
            AddWeapon(WeaponsList.instance.GetWeaponByName(desiredWeaponName), 12, 72); // colt 45
            ChangeToSlotAndUpdateWeapon(slots[currentSlotId]);
        }
        else
        {
            //CHANGETOSLOTANDUPDATEWEAPONUN MELEE KISMI, İLK BIÇAKLA BAŞLAYACAĞI İÇİN
            AddWeaponWithDefaultValues(defaultMeleeWeapon.weaponName);
            currentSlotId = 2;

            ChangeToSlotAndUpdateWeapon(slots[currentSlotId]);
        }

        //figure out which weapon to start with

        if (GetFirstSlotWithTheType(WeaponSlotType.Projectile).weapon != null)
        {
            currentSlotId = 3;
        }

        if (GetFirstSlotWithTheType(WeaponSlotType.Secondary).weapon != null)
        {
            currentSlotId = 1;
        }

        if (GetFirstSlotWithTheType(WeaponSlotType.Primary).weapon != null)
        {
            currentSlotId = 0;
        }

        if (gameplayManager.currentLevelId == 0) {
            currentSlotId = 2;
        }

        ChangeToSlotAndUpdateWeapon(slots[currentSlotId]);

       


        // AddWeaponWithDefaultValues(defaultMeleeWeapon.weaponName);
        //currentSlotId = 2;
        //ChangeToSlotAndUpdateWeapon(slots[currentSlotId]);

    }

   

    void GenerateRecoil()
    {
        time = duration;
    }
    

    // Update is called once per frame
    void Update()
    {

        /*if(!playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Reload") && reloadingAnimationStarted && !isReloading )      
       {
              rightArmRig.weight = 1; 


           reloadingAnimationStarted = false;
       }

       */



        if (gunHitscanBlocked || gunBarrelNearTrigger)
        {
            if (Physics.Raycast(currentGunShootRayPoint.position, currentGunShootRayPoint.forward, out rayHitFromGunsBarrel, GetCurrentWeapon().range))
            {
                if (!(GetCurrentWeapon().weaponType == WeaponType.Melee)) //bu oldu galiba
                {
                    redCrossIcon.position = rayHitFromGunsBarrel.point;
                    redCrossIcon.rotation = Quaternion.LookRotation(rayHitFromGunsBarrel.normal);
                    redCrossIcon.gameObject.SetActive(true);

                    reticle.gameObject.SetActive(false);
                } else
                {
                    redCrossIcon.gameObject.SetActive(false);
                }
                  
            }
        }
            
        else
        {
            if(!(GetCurrentWeapon().weaponType == WeaponType.Melee) && !(GetCurrentWeapon().weaponType == WeaponType.Projectile))
            reticle.gameObject.SetActive(true);


            redCrossIcon.gameObject.SetActive(false);
        }
           

        HandleSpineAimMovements();
        HandleWeaponAnimations();
     

        if (isShooting)
            shootingTime += Time.deltaTime;
        else
            shootingTime = 0;

            moveVectorMagnitude = controller.moveVectorMagnitude;
        isMoving = controller.isMoving;

        if(reticle.gameObject.activeSelf)
        {
            reticle.sizeDelta = new Vector2(reticleSize, reticleSize);
            //reticleSize = currentSpread * Mathf.PI *  Mathf.PI / 2 * Mathf.Sqrt(2);
            reticleSize = (currentSpread + crossHairLineLength) * 2 + crossHairLineLength /* * 2  */ ;

            float lineLengthLerpValue = Map(currentSpread, standingSpread, movingSpread * movingSpreadTimeMultiplier, 0, 1);
            crossHairLineLength = Mathf.Lerp(crossHairMinWidth, crossHairMaxWidth, lineLengthLerpValue);
            chLeftLine.sizeDelta = new Vector2(crossHairLineLength, 3);
            chRightLine.sizeDelta = new Vector2(crossHairLineLength, 3);
            chTopLine.sizeDelta = new Vector2(3, crossHairLineLength);
            chBottomLine.sizeDelta = new Vector2(3, crossHairLineLength);
        }
       

        

        //problem: jjoystick input values
        if (isMoving)
        {
          
            if(isShooting)
            {
                currentSpread = Mathf.Lerp(currentSpread,Mathf.Clamp(  movingSpread * movingSpreadTimeMultiplier * moveVectorMagnitude, standingSpread * standingSpreadTimeMultiplier, Mathf.Infinity), shootingTime * 2 );
                movingShootReturnTimerCh = 0;
            } else
            {
                //currentSpread = Mathf.Lerp(currentSpread, Mathf.Lerp(currentSpread, movingSpread, moveVectorMagnitude), movingShootReturnTimerCh);
                currentSpread = Mathf.Lerp(standingSpread, Mathf.Clamp( movingSpread * moveVectorMagnitude, standingSpread, Mathf.Infinity), moveVectorMagnitude);
                movingShootReturnTimerCh += Time.deltaTime;
                standShootReturnTimerCh = 0;
            }
        }
        
         else
        {
            if(isShooting)
            {
                currentSpread = Mathf.SmoothStep(currentSpread, standingSpread * standingSpreadTimeMultiplier, shootingTime );
                standShootReturnTimerCh = 0;
            } else
            {
                currentSpread = Mathf.SmoothStep(currentSpread, standingSpread, standShootReturnTimerCh);
                standShootReturnTimerCh += Time.deltaTime;
            }
            
            
        }
            
        
        
        if(time > 0)
        {
            playerCam.localEulerAngles -= new Vector3(verticalRecoil * Time.deltaTime / duration, UnityEngine.Random.Range( -horizontalRecoil * Time.deltaTime, horizontalRecoil * Time.deltaTime), 0);
            time -= Time.deltaTime;
        }


        if (!pc) //android
            shootPressed = fireJoystick.Pressed;
        else //pc or browser
            shootPressed = Input.GetMouseButton(0);

        timeStamp += Time.deltaTime;
        if(shootPressed && !gameplayManager.afterDeath && !gameplayManager.levelCompleted)
        {
            WeaponSlotType currentSlotType = slots[currentSlotId].slotType;
            if (timeStamp > gunCoolDown )
            {
                for (int i = 0; i < GetCurrentWeapon().bulletCount; i++)
                {
                    if(currentSlotType != WeaponSlotType.Projectile) //if projectile, look at unloaded ammo
                    {
                        if (slots[currentSlotId].magAmmo > 0)
                        {
                            if ((currentSlotType == WeaponSlotType.Primary || currentSlotType == WeaponSlotType.Secondary) && !isReloading)
                                Shoot();
                            if (currentSlotType == WeaponSlotType.Melee)
                                MeleeAttack();
                            
                                
                        }
                        else if (slots[currentSlotId].magAmmo <= 0)
                        {
                            // if(!isReloading)
                            // AudioManager.instance.PlayOneShot("noammo");
                        }
                    } else
                    {
                        if (slots[currentSlotId].unloadedAmmo > 0)
                            ThrowProjectile();
                        throwNear = false;
                    }
                  
                    
                }
                
                timeStamp = 0;
            } else
            {
              
            }
        } else
        {
           
            isShooting = false;
        }

        if (GetCurrentWeapon().weaponType != WeaponType.Melee && GetCurrentWeapon().weaponType != WeaponType.Projectile)
        {
            Vector3 pos = new Vector2(Screen.width / 2, Screen.height / 2);
            Ray ray = playerCamComponent.ScreenPointToRay(pos);
            RaycastHit hit;
            if (Physics.Raycast(playerCam.position, ray.direction, out hit, gunRange))
            {



                Debug.DrawLine(playerCam.position, hit.point, Color.green);
                RaycastHit hit2;
                float dist = Vector3.Distance(currentGunShootRayPoint.position, hit.point);
                if (Physics.Raycast(currentGunShootRayPoint.position, (hit.point - currentGunShootRayPoint.position).normalized, out hit2, dist - 0.5f, gunHitScanDetectLayer, QueryTriggerInteraction.Ignore) && Vector3.Distance(currentGunShootRayPoint.position, hit2.point) < 0.5f/* || transform.InverseTransformPoint(hit2.point).z > transform.InverseTransformPoint(currentGunBarrelPoint.position).z*/)
                {
                    Debug.Log("Gun barrel is being blocked");
                    Debug.DrawLine(currentGunShootRayPoint.position, hit2.point, Color.red);
                    if (GetCurrentWeapon().weaponType != WeaponType.Melee && GetCurrentWeapon().weaponType != WeaponType.Projectile)
                        gunHitscanBlocked = true;

                    redCrossIcon.position = hit2.point;
                    redCrossIcon.rotation = Quaternion.LookRotation(hit2.normal);
                    //  redCrossIcon.gameObject.SetActive(true);


                }
                else
                {
                    gunHitscanBlocked = false;


                }




            }
        }


        if (isTouchOnWeaponSlot && weaponSlotTouchStillHolding && weaponSlotTouchDown)
            touchingWeaponSlotTime += Time.deltaTime;
        else
            touchingWeaponSlotTime = 0;
        
       if(touchingWeaponSlotTime > 1)
        {
            Debug.Log("Drop weapon");

            if (slots[currentSlotId].weapon != defaultMeleeWeapon)
                DropWeapon(slots[currentSlotId]);

            SwitchToNextWeapon();
            weaponSlotTouchStillHolding = false;
            touchingWeaponSlotTime = 0;
        }

       if(isBombIndicatorActive && lastBombTransform != null)
        {
            
            Vector3 direction = transform.position - lastBombTransform.position;
            Quaternion sourceRot = Quaternion.LookRotation(direction);
            sourceRot.z = -sourceRot.y;
            sourceRot.x = sourceRot.y = 0;
            Vector3 northDirection = new Vector3(0, 0, transform.eulerAngles.y);
            bombIndicator.localRotation = sourceRot * Quaternion.Euler(northDirection);
            hazardSign.localEulerAngles = -bombIndicator.localEulerAngles;
        }

       if(Input.GetKeyDown(KeyCode.R))
        {
            ReloadButton();
        }

    }

    void HandleWeaponAnimations()
    {
        playerAnim.SetBool("isShooting", isShooting);
        
        if(slots[currentSlotId].weapon.weaponType != WeaponType.Melee && GetCurrentWeapon().weaponType != WeaponType.Projectile)
         gunAnim.SetBool("shooting", isShooting);
    }

    

    void Shoot()
    {
        slots[currentSlotId].magAmmo--;
        ammoText.text = slots[currentSlotId].magAmmo.ToString() + "/" + slots[currentSlotId].unloadedAmmo.ToString();


        //Handheld.Vibrate();

        GenerateRecoil();
        muzzleFlash.Play();
        cartridgeEject.Play();
        wepAudioSource.PlayOneShot(wepAudioSource.clip);
        gunAnim.SetTrigger("shoot");
        isShooting = true;

        Vector3 pos = new Vector2(Screen.width / 2, Screen.height / 2) +  UnityEngine.Random.insideUnitCircle * currentSpread;
           Ray ray = playerCamComponent.ScreenPointToRay(pos);
        
        RaycastHit hit;

        if(!gunBarrelNearTrigger && !gunHitscanBlocked)
        {
           if (Physics.Raycast(playerCam.position, ray.direction, out hit, gunRange, shootingLayerMask, QueryTriggerInteraction.Collide))
            {
                GunRaycasting(hit);
            }
        } else
        {
            if(Physics.Raycast(currentGunShootRayPoint.position, currentGunShootRayPoint.forward, out hit, gunRange))
                GunRaycasting(hit);
        }  

        if (slots[currentSlotId].magAmmo == 0 && slots[currentSlotId].unloadedAmmo != 0) //Auto reload
            ReloadStart();

        if (slots[currentSlotId].magAmmo == 0 && slots[currentSlotId].unloadedAmmo == 0)//Destroy gun and switch to next
        {
            slots[currentSlotId].weapon = null;
            SwitchToNextWeapon();
        }
          

        if (gameplayManager.currentLevelId == 0)
        {
            if(slots[currentSlotId].magAmmo == 0 && slots[currentSlotId].unloadedAmmo == 0 && GetFirstSlotWithTheType(WeaponSlotType.Primary).weapon == null )
            {
                
                outOfAmmoHelperText.SetActive(true);
            }

        }
        Tutorial.instance.Shooted();
    }

    void GunRaycasting(RaycastHit hit)
    {
        Debug.DrawLine(playerCam.position, hit.point, Color.yellow);

        //   if(gunBeingBlocked)
        //  {
        //     if(
        // }

        Collider hitCollider = hit.collider;




        if (hitCollider.transform.root.CompareTag("NPC"))
        {
            Animator NPCanim = hitCollider.transform.root.GetComponent<Animator>();
            Bot bot = hitCollider.transform.root.GetComponent<Bot>();
            switch (hitCollider.transform.GetComponent<BodyPart>().bodyPart)
            {
                case BodyPart.Part.Head:


                    bot.TakeDamage(GetCurrentWeapon().headDamage, BodyPart.Part.Head, hit.point, Quaternion.LookRotation(hit.normal));
                    break;
                case BodyPart.Part.Torso:

                    bot.TakeDamage(GetCurrentWeapon().bodyDamage, BodyPart.Part.Torso, hit.point, Quaternion.LookRotation(hit.normal));
                    break;
                case BodyPart.Part.Limb:

                    bot.TakeDamage(GetCurrentWeapon().limbDamage, BodyPart.Part.Limb, hit.point, Quaternion.LookRotation(hit.normal));
                    break;
            }



        }
        else if (hitCollider.transform.root.name == GameplayManager.instance.RootPoolName)
        {

            Animator NPCanim = hitCollider.transform.GetComponentInParent<Animator>();
            //Zombie zombie = hitCollider.transform.root.GetComponent<Zombie>();
            ZombieTemp zombie = hitCollider.transform.GetComponentInParent<ZombieTemp>();


            if(!hitCollider.transform.CompareTag("ExplosiveBarrel"))
            {
                switch (hitCollider.transform.GetComponent<BodyPart>().bodyPart)
                {
                    case BodyPart.Part.Head:


                        zombie.TakeDamage(GetCurrentWeapon().headDamage, BodyPart.Part.Head, hit.point, Quaternion.LookRotation(hit.normal), true);
                        break;
                    case BodyPart.Part.Torso:

                        zombie.TakeDamage(GetCurrentWeapon().bodyDamage, BodyPart.Part.Torso, hit.point, Quaternion.LookRotation(hit.normal), true);
                        break;
                    case BodyPart.Part.Limb:

                        zombie.TakeDamage(GetCurrentWeapon().limbDamage, BodyPart.Part.Limb, hit.point, Quaternion.LookRotation(hit.normal), true);
                        break;
                }
            } else
            {
                hitCollider.transform.GetComponentInParent<ExplosiveZombie>().BarrelHitByGun();
            }

           



        }
        else if (hitCollider.transform.CompareTag("WoodenCrate"))
        {
            WoodenCrate crate = hitCollider.transform.GetComponent<WoodenCrate>();
            Instantiate(crate.particle, hit.point, Quaternion.LookRotation(hit.normal));
            crate.TakeDamage(GetCurrentWeapon().bodyDamage);
        }

        else
        {
      //      GameObject impactGO = Instantiate(bulletImpactGO, hit.point, Quaternion.LookRotation(hit.normal));
        //    Destroy(impactGO, 3);
            Instantiate(sandImpactParticle, hit.point, Quaternion.LookRotation(hit.normal));
        }






        
    }

   void MeleeAttack()
    {
      //  wepAudioSource.PlayOneShot(wepAudioSource.clip);
      if(!meleeAttacking)
        playerAnim.SetTrigger("stab");
    }

    public float Map( float value, float fromSource, float toSource, float fromTarget, float toTarget)
    {
        return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }

 

   

    void CheckDroppedWeapons()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, 8))
        {      
            if (hit.transform.CompareTag("DroppedWeapon"))
            {
                DroppedWeapon dw = hit.transform.GetComponent<DroppedWeapon>();
                Weapon weaponInSight = dw.weapon;
                Destroy(hit.transform.gameObject);

                //if there's a same slot type of weapon, drop it
                if (GetFirstSlotWithTheType(WeaponTypeToSlotType(dw.weapon.weaponType)).weapon != null)
                    DropWeapon(GetFirstSlotWithTheType(WeaponTypeToSlotType(dw.weapon.weaponType)));

                AddWeapon(weaponInSight, dw.ammoLeftInMag, dw.unreloadedAmmo);

                //if we're changing the weapon in the slot we're currently in, update
                if (GetFirstSlotWithTheType(WeaponTypeToSlotType(dw.weapon.weaponType)) == slots[currentSlotId])
                    ChangeToSlotAndUpdateWeapon(slots[currentSlotId]);           

            }

        }
    }

    void AddWeapon(Weapon weapon, int magAmmo, int unloadedAmmo)
    {
        //find which slot type the weapon belongs to    
        WeaponSlot slot = GetFirstSlotWithTheType(WeaponTypeToSlotType(weapon.weaponType));

        if(weapon.weaponType != WeaponType.Projectile)
        {
            slot.weapon = weapon;
            slot.magAmmo = magAmmo;
            slot.unloadedAmmo = unloadedAmmo;
        } else
        {
            slot.weapon = weapon;
            slot.magAmmo = 0;
            slot.unloadedAmmo = unloadedAmmo + magAmmo;
        }
       
        
    }

    void DropWeapon(WeaponSlot inWhichSlot)
    {

        GameObject go = Instantiate(inWhichSlot.weapon.droppedPrefab, weaponDropPoint.position, Quaternion.identity);
        DroppedWeapon dw = go.GetComponent<DroppedWeapon>();
        dw.ammoLeftInMag = inWhichSlot.magAmmo;
        dw.unreloadedAmmo = inWhichSlot.unloadedAmmo;
        dw.Throw(weaponDropPoint.forward);
        inWhichSlot.weapon = null;
        inWhichSlot.magAmmo = 0;
        inWhichSlot.unloadedAmmo = 0;
    }

    //change the slot and update to the weapon values, model in that slot
   public void ChangeToSlotAndUpdateWeapon(WeaponSlot slot)
    {
        Weapon theWep = slot.weapon;
        for (int i = 0; i < weaponGOs.Length; i++)
        {
            weaponGOs[i].SetActive(false);
        }
        for (int i = 0; i < nonIKWeaponGOs.Length; i++)
        {
            nonIKWeaponGOs[i].SetActive(false);
        }

        gunCoolDown = theWep.cooldown;
        gunRange = theWep.range;
        standingSpread = theWep.standingSpread;
        movingSpread = theWep.movingSpread;
        
      
          
        
        verticalRecoil = theWep.verticalRecoil;
        horizontalRecoil = theWep.horizontalRecoil;
        isThrowingProjectile = false;
        playerAnim.SetBool("throwing", false);

        weaponIconRect.sizeDelta = new Vector2(theWep.weaponIcon.rect.size.x / 1.5f, theWep.weaponIcon.rect.size.y / 1.5f);
        if (slot.slotType == WeaponSlotType.Primary || slot.slotType == WeaponSlotType.Secondary)
        {
            ammoText.text = slots[currentSlotId].magAmmo.ToString() + "/" + slots[currentSlotId].unloadedAmmo.ToString();
            currentWeaponObject = weaponsParent.GetChild(theWep.weaponId).gameObject;        
            gunAnim = currentWeaponObject.GetComponent<Animator>();
            playerAnim.runtimeAnimatorController = theWep.animatorOverride;
            HandleAnimatorOverrideChange();
            leftArmTarget.target = currentWeaponObject.transform.Find("LeftHandTarget");
            leftArmHint.target = currentWeaponObject.transform.Find("LeftHandHint");
            currentGunShootRayPoint = currentWeaponObject.transform.Find("GunRayPoint");

            //  leftArmIKTarget.target = currentWeaponObject.transform.Find("LeftArmHoldpoint");
            //  rightArmIKTarget.target = currentWeaponObject.transform.Find("RightArmHoldpoint");
            cartridgeEject = currentWeaponObject.transform.Find("CartridgeEjectParticle").GetComponent<ParticleSystem>();
            muzzleFlash = currentWeaponObject.transform.Find("MFpoint").GetChild(0).GetComponent<ParticleSystem>();
           // leftArmRig.weight = 1;
          //  rightArmRig.weight = 1;
            leftHandRig.weight = 1;
            rightHandRig.weight = 1;
            rightHandRig.data.offset = new Vector3(theWep.rightHandXOffset, rightHandRig.data.offset.y, theWep.rightHandZOffset);
            playerAnim.SetBool("aimMode" , true);
            playerAnim.SetBool("knifeMode", false);
            fireJoystickHandle.sprite = shootImg;
            fireJoystickHandle.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 250);



            dotCrosshair.SetActive(false);
            reticle.gameObject.SetActive(true);
        }
        if (slot.slotType == WeaponSlotType.Melee)
        {
            currentWeaponObject = nonIKWeaponsParent.Find(theWep.weaponName).gameObject;
            
            ammoText.text = "";
           // leftArmRig.weight = 0;
            //rightArmRig.weight = 0;
            leftHandRig.weight = 0;
            rightHandRig.weight = 0;
            playerAnim.SetBool("knifeMode", true);
            playerAnim.SetBool("aimMode", false);
           // dotCrosshair.SetActive(true);
            reticle.gameObject.SetActive(false);
            fireJoystickHandle.sprite = stabImg;
            fireJoystickHandle.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
        }
        if (slot.slotType == WeaponSlotType.Projectile)
        {
            currentWeaponObject = nonIKWeaponsParent.Find(theWep.weaponName).gameObject;
            
            ammoText.text = slots[currentSlotId].unloadedAmmo.ToString();

            // leftArmRig.weight = 0;
            //rightArmRig.weight = 0;
            leftHandRig.weight = 0;
            rightHandRig.weight = 0;
            playerAnim.SetBool("knifeMode", true);
            playerAnim.SetBool("aimMode", false);
            // dotCrosshair.SetActive(true);
            reticle.gameObject.SetActive(false);
            fireJoystickHandle.sprite = grenadeImg;
            fireJoystickHandle.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
        }
        if (isReloading)
        {
            isReloading = false;
           // reloadingAnimationStarted = false;
            playerAnim.SetBool("isReloading", isReloading);
            playerAnim.SetTrigger("forceCancelReload");
            
        }
        currentWeaponObject.SetActive(true);
        currentWeaponIcon.sprite = theWep.weaponIcon;
        wepAudioSource = currentWeaponObject.GetComponent<AudioSource>();
        if (slots[currentSlotId].magAmmo == 0 && slots[currentSlotId].unloadedAmmo != 0) //Auto reload
            ReloadStart();
        if (slot.slotType == WeaponSlotType.Projectile)
        {
            if (!pc)
                throwNearButton.SetActive(true);
            if(!pc)
            reloadButton.SetActive(false);
        }
            
        else
        {
            if (!pc)
                throwNearButton.SetActive(false);
            if (!pc)
                reloadButton.SetActive(true);
        }
            
    }

    public void SwitchToNextWeapon()
    {
        

        int nextSlot = currentSlotId + 1;
        if (nextSlot == slots.Length   )
        {
            nextSlot = 0;
            if(slots[currentSlotId].weapon != null)
            weaponNameDisplay.text = slots[currentSlotId].weapon.weaponName;
            weaponNameDisplay.GetComponent<Animator>().SetTrigger("a");
        }
           

        if(slots[nextSlot].weapon != null)
        {
            Debug.Log("the next slot is " + (nextSlot).ToString() + "th");
            currentSlotId = nextSlot;
            ChangeToSlotAndUpdateWeapon(slots[currentSlotId]);
            weaponNameDisplay.text = slots[currentSlotId].weapon.weaponName;
            weaponNameDisplay.GetComponent<Animator>().SetTrigger("a");
            return;
        }

        for (int i = currentSlotId; slots[nextSlot].weapon == null; i++)
        {
            
            if (i == slots.Length)
                i = 0;
            
            nextSlot = i + 1;
            if (nextSlot == slots.Length)
                nextSlot = 0;
            
            if (slots[nextSlot].weapon != null)
            {
                Debug.Log("the next slot is " + (nextSlot).ToString() + "th");
                if (nextSlot == currentSlotId)
                {
                    Debug.Log("next slot is same, doin nothing");
                    break;
                } 
                   
                
                    

                currentSlotId = nextSlot;
                if (slots[currentSlotId].unloadedAmmo == 0 && slots[currentSlotId].weapon.weaponType == WeaponType.Projectile)
                    SwitchToNextWeapon();

                weaponNameDisplay.text = slots[currentSlotId].weapon.weaponName;
                weaponNameDisplay.GetComponent<Animator>().SetTrigger("a");
                ChangeToSlotAndUpdateWeapon(slots[currentSlotId]);
                break; 
            }
                
             
        }


        Tutorial.instance.WeaponSwitched();

       
           
    }

    public void WeaponSlotTouchIsInside()
    {
        isTouchOnWeaponSlot = true;
    }

    public void WeaponSlotTouchIsOutside()
    {
        isTouchOnWeaponSlot = false;
        weaponSlotTouchDown = false;
    }

    public void WeaponSlotTouchDown()
    {
        weaponSlotTouchDown = true;
        weaponSlotTouchStillHolding = true;
    }

    public void WeaponSlotTouchUp()
    {
        if(weaponSlotTouchDown && weaponSlotTouchStillHolding)
        {
            SwitchToNextWeapon();
            weaponSlotTouchDown = false;
        }

        
        if(weaponSlotTouchStillHolding && !isTouchOnWeaponSlot) //sliding to outside 
        {
            SwitchToNextWeapon();
            weaponSlotTouchDown = false;
        }
       
        
    }


    public void CheckDroppedWeaponButton()
    {
        CheckDroppedWeapons();
    }

    public void ReloadButton()
    {
        ReloadStart();
    }

    public void ReloadStart()
    {

        //rightArmRig.weight = 0;
        //   playerAnim.SetBool("isReloading", true);
        if (gunAnim != null)
            gunAnim.SetBool("reload", true);
        isReloading = true;


    }

    //reload animation event
    public void ReloadComplete()
    {
        
        WeaponSlot slot = slots[currentSlotId];
        int ammoDifference = slot.weapon.maxMagAmmo - slot.magAmmo;
        if(ammoDifference > slot.unloadedAmmo)
        {
            slot.magAmmo += slot.unloadedAmmo;
            slot.unloadedAmmo = 0;
        } else
        {
            slot.unloadedAmmo -= ammoDifference;
            slot.magAmmo = slot.weapon.maxMagAmmo;
        }
        
        ammoText.text = slot.magAmmo.ToString() + "/" + slot.unloadedAmmo.ToString();


          isReloading = false;
       
        
        gunAnim.SetBool("reload", false);
        //playerAnim.SetBool("isReloading", isReloading);

    }

   

    

    public void MeleeAttackPhaseBegin()
    {
        Debug.Log("melee attack begins!");
        meleeAttacking = true;
    }

    public void MeleeAttackPhaseEnd()
    {
        Debug.Log("melee attack ends!");
        if (GetCurrentWeapon().weaponType == WeaponType.Melee)
        {
            currentWeaponObject.GetComponent<MeleeWeapon>().Check();
        }
        meleeAttacking = false;
    }

    public void DroppedWeaponEnteredPickupRange(DroppedWeapon dw)
    {
        WeaponSlot slot = GetFirstSlotWithTheType(WeaponTypeToSlotType(dw.weapon.weaponType));
        
        if(slot.weapon == null) //if that slot is empty, automatically pick up the weapon
        {
            AddWeapon(dw.weapon, dw.ammoLeftInMag, dw.unreloadedAmmo);
            Destroy(dw.gameObject);
            if (ReturnWeaponWithPriority(dw.weapon, slots[currentSlotId].weapon) == dw.weapon)
            {
                currentSlotId = Array.IndexOf(slots, slot);
                ChangeToSlotAndUpdateWeapon(slot);

            }
               
        } 

    }

    void HandleSpineAimMovements()
    {
        /* RaycastHit hit;
         if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, 50))
         {
            aimHitPoint.position = hit.point;
         } 
         */
        
        float mappedValue = Map(playerCam.eulerAngles.x, cameraMovement.minHeight, cameraMovement.maxHeight, -30, 50) -5 ;
        upperBodyRig.data.offset = new Vector3(mappedValue, upperBodyRig.data.offset.y, upperBodyRig.data.offset.z);
        rightHandRig.data.offset = new Vector3(rightHandRig.data.offset.x, mappedValue , rightHandRig.data.offset.z);
    }
   

    /* Weapon SlotToWeapon(WeaponSlotType slot)
     {
         switch (slot)
         {
             case WeaponSlotType.Primary:
                 return primaryWeapon;
             case WeaponSlotType.Secondary:
                 return secondaryWeapon;
             case WeaponSlotType.Melee:
                 return meleeWeapon;
             case WeaponSlotType.Projectile:
                 return projectileWeapon;

         }
         return null;
     }
     */
     Weapon ReturnWeaponWithPriority(Weapon wep1, Weapon wep2)
    {
        WeaponSlotType wep1slot = WeaponTypeToSlotType(wep1.weaponType);
        WeaponSlotType wep2slot = WeaponTypeToSlotType(wep2.weaponType);
        if (wep1slot == wep2slot)
            return wep1;
        var a = Mathf.Min((int)wep1slot, (int)wep2slot);
        if(a == (int)wep1slot)
        {
            return wep1;
        }
        else if (a == (int)wep2slot)
        {
            return wep2;
        }

        Debug.LogError("yanlışlık var");
        return null;
        //  return  (WeaponSlotType) Enum.ToObject(typeof(WeaponSlotType), a);

    }
    WeaponSlotType WeaponTypeToSlotType(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Rifle:
                return WeaponSlotType.Primary;              
            case WeaponType.Sniper:
                return WeaponSlotType.Primary;
               
            case WeaponType.Shotgun:
                return WeaponSlotType.Primary;
                
            case WeaponType.Submachine:
                return WeaponSlotType.Primary;
                
            case WeaponType.Pistol:
                return WeaponSlotType.Secondary;
                
            case WeaponType.Projectile:
                return WeaponSlotType.Projectile;
            case WeaponType.Melee:
                return WeaponSlotType.Melee;
        }

        return WeaponSlotType.Primary;
    }

   public Weapon GetCurrentWeapon()
    {
        return slots[currentSlotId].weapon;
    }

    WeaponSlot GetFirstSlotWithTheType(WeaponSlotType slotType)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].slotType == slotType)
            {
                return slots[i];
               
            }
        }
        return null;
    }
    
    void ThrowProjectile()
    {
        if(!isThrowingProjectile)
        {
            
            
            isThrowingProjectile = true;
            playerAnim.SetBool("throwing", true);
           
        }
        

    }

   

    public void ThrowAnimEnd()
    {
        if (slots[currentSlotId].unloadedAmmo == 0 && GetCurrentWeapon().weaponType == WeaponType.Projectile)
        {
            slots[currentSlotId].weapon = null;
            SwitchToNextWeapon();
        }
        isThrowingProjectile = false;
        playerAnim.SetBool("throwing", false);
       
    }

    public void ThrowNearButton()
    {
        throwNear = true;
        if (timeStamp > gunCoolDown)
        {
             if(!gameplayManager.afterDeath && !gameplayManager.levelCompleted && slots[currentSlotId].slotType == WeaponSlotType.Projectile && slots[currentSlotId].unloadedAmmo > 0)
            {
                ThrowProjectile();
            }
        }
    }

    public void ThrowMoment()
    {
        if(GetCurrentWeapon().weaponType == WeaponType.Projectile)
        {
            slots[currentSlotId].unloadedAmmo--;
            ammoText.text = slots[currentSlotId].unloadedAmmo.ToString();
            GameObject go = Instantiate(GetCurrentWeapon().thrownProjectileObj, currentWeaponObject.transform.position, currentWeaponObject.transform.rotation);
            if(!throwNear)
            {
                go.GetComponent<Rigidbody>().AddForce(transform.forward * grenadeFarForwardForce);
                go.GetComponent<Rigidbody>().AddForce(transform.up * grenadeFarUpForce);
            } else
            {
                go.GetComponent<Rigidbody>().AddForce(transform.forward * grenadeNearForwardForce);
                go.GetComponent<Rigidbody>().AddForce(transform.up * grenadeNearUpForce);
            }
           
        }
      
    }



    void AddWeaponWithDefaultValues(string weaponName)
    {
        AddWeapon(WeaponsList.instance.GetWeaponByName(weaponName), WeaponsList.instance.GetWeaponByName(weaponName).maxMagAmmo, WeaponsList.instance.GetWeaponByName(weaponName).maxTotalAmmo - WeaponsList.instance.GetWeaponByName(weaponName).maxMagAmmo);
        
    }
    
    void HandleAnimatorOverrideChange()
    {
        

        AnimatorStateInfo[] layerInfo = new AnimatorStateInfo[playerAnim.layerCount];
        for (int i = 0; i < playerAnim.layerCount; i++)
        {
            layerInfo[i] = playerAnim.GetCurrentAnimatorStateInfo(i);
        }

        // Do swap clip in override controller here

        // Push back state
        for (int i = 0; i < playerAnim.layerCount; i++)
        {
            playerAnim.Play(layerInfo[i].fullPathHash, i, layerInfo[i].normalizedTime);
        }

        // Force an update
        playerAnim.Update(0.0f);
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.transform.CompareTag("Pickup"))
        {
            PickupObject pickupobj = col.transform.GetComponent<PickupObject>();

            switch (pickupobj.pickup.pickupType)
            {
                case PickupType.AddHealth:
                    playerHealth.PlayerAddHealth(pickupobj.pickup.value);
                    Instantiate(pickupobj.pickup.particlePrefab, transform.position, Quaternion.identity);
                    AudioManager.instance.PlayOneShot("healthPickup");
                    break;
                case PickupType.AddArmor:
                    playerHealth.PlayerAddArmor(pickupobj.pickup.value);
                    Instantiate(pickupobj.pickup.particlePrefab, transform.position, Quaternion.identity);
                    AudioManager.instance.PlayOneShot("healthPickup");
                    break;
            }
            Destroy(col.gameObject);
        }

        if (col.transform.CompareTag("WeaponPickup"))
        {
            Weapon wep = col.transform.GetComponent<WeaponPickup>().wep;
            WeaponSlot wpslot = GetFirstSlotWithTheType(WeaponTypeToSlotType(wep.weaponType));
            

            if (wpslot.weapon == null)  //auto reload when pickup
            {
                if(wpslot.slotType != WeaponSlotType.Projectile)
                AddWeapon(wep, wep.maxMagAmmo,wep.pickupAmmo - wep.maxMagAmmo); //pickup are 2 mags
                if (wpslot.slotType == WeaponSlotType.Projectile)
                    AddWeapon(wep, 0, wep.pickupAmmo);

                if (ReturnWeaponWithPriority(wep, slots[currentSlotId].weapon) == wep)
                {
                    currentSlotId = Array.IndexOf(slots, wpslot);
                    ChangeToSlotAndUpdateWeapon(wpslot);

                }

                WeaponSlot slot = slots[currentSlotId];


                int ammoDifference = slot.weapon.maxMagAmmo - slot.magAmmo;
                if (ammoDifference > slot.unloadedAmmo)
                {
                    slot.magAmmo += slot.unloadedAmmo;
                    slot.unloadedAmmo = 0;
                }
                else
                {
                    slot.unloadedAmmo -= ammoDifference;
                    slot.magAmmo = slot.weapon.maxMagAmmo;
                }

                if(slot.slotType != WeaponSlotType.Projectile && slot.slotType != WeaponSlotType.Melee)
                ammoText.text = slot.magAmmo.ToString() + "/" + slot.unloadedAmmo.ToString();
                if(slot.slotType == WeaponSlotType.Projectile)
                    ammoText.text = slot.unloadedAmmo.ToString();

                Instantiate(weaponPickupBulletParticle, transform.position, Quaternion.identity);
                AudioManager.instance.PlayOneShot("weaponpickup");
                Destroy(col.gameObject);
            } else
            {
                if(wpslot.weapon == wep) //picking up ammo of a weapon you already have
                {
                    wpslot.unloadedAmmo += wep.pickupAmmo;

                    int ammoDifference = wpslot.weapon.maxMagAmmo - wpslot.magAmmo;
                    if (ammoDifference > wpslot.unloadedAmmo)
                    {
                        wpslot.magAmmo += wpslot.unloadedAmmo;
                        wpslot.unloadedAmmo = 0;
                    }
                    else
                    {
                        wpslot.unloadedAmmo -= ammoDifference;
                        wpslot.magAmmo = wpslot.weapon.maxMagAmmo;
                    }

                    if (wpslot == slots[currentSlotId])
                    {
                        if (wpslot.slotType != WeaponSlotType.Projectile && wpslot.slotType != WeaponSlotType.Melee)
                            ammoText.text = wpslot.magAmmo.ToString() + "/" + wpslot.unloadedAmmo.ToString();
                        if (wpslot.slotType == WeaponSlotType.Projectile)
                            ammoText.text = wpslot.unloadedAmmo.ToString();
                    }
                        

                    Instantiate(weaponPickupBulletParticle, transform.position, Quaternion.identity);
                    AudioManager.instance.PlayOneShot("weaponpickup");
                    Destroy(col.gameObject);
                } else
                {
                    exchangeWeaponButton.SetActive(true);
                    lastEnteredWepPickup = col.transform.gameObject;
                    exchangeWep = wep;
                }

               
            }

            ChangeToSlotAndUpdateWeapon(slots[currentSlotId]);



        }

        if(col.transform.CompareTag("TurretInteractionArea"))
        {
            
            lastInteractedTurret = col.transform.GetComponentInParent<Turret>();
            if(!lastInteractedTurret.isActivated)
                turretPurchasePanel.SetActive(true);

            turretName.text = "TURRET " +  (lastInteractedTurret.id + 1).ToString();
            turretPriceText.text = lastInteractedTurret.price.ToString();
        }

    }

    public void OnTriggerExit(Collider col)
    {
        if (col.transform.CompareTag("WeaponPickup"))
        {
            exchangeWeaponButton.SetActive(false);
        }

        if (col.transform.CompareTag("TurretInteractionArea"))
        {
            turretPurchasePanel.SetActive(false);
        }
    }

    public void Revived()
    {
        upperBodyRig.weight = 1;
    }

    public void UpdateThrowNearButtonStatus()
    {
        if (slots[currentSlotId].slotType == WeaponSlotType.Projectile)
        {
            if (!pc)
                throwNearButton.SetActive(true);
            if (!pc)
                reloadButton.SetActive(false);
        }

        else
        {
            if (!pc)
                throwNearButton.SetActive(false);
            if (!pc)
                reloadButton.SetActive(true);
        }
    }

    public void TurretPurchaseClicked()
    {
        if(DataPersistenceManager.instance.gameData.coin >= lastInteractedTurret.price)
        {
            DataPersistenceManager.instance.gameData.coin = DataPersistenceManager.instance.gameData.coin - lastInteractedTurret.price;
            DataPersistenceManager.instance.gameData.turrets[lastInteractedTurret.id] = true;
            lastInteractedTurret.isActivated = true;
            turretPurchasePanel.SetActive(false);
            AudioManager.instance.PlayOneShot("purchase");
            lastInteractedTurret.Activate();
            Debug.Log("Bought turret");
        } else
        {
            Debug.Log("Cant buy turret");
        }
    }

    public void ExchangeWeapon()
    {
        WeaponSlot wpslot = GetFirstSlotWithTheType(WeaponTypeToSlotType(exchangeWep.weaponType));
        
        for (int i = 0; i < slots.Length; i++)
        {
            if(wpslot == slots[i])
            {
                currentSlotId = i;
                break;
            }
        }
        ChangeToSlotAndUpdateWeapon(wpslot);

        if (wpslot.slotType != WeaponSlotType.Projectile)
            AddWeapon(exchangeWep, exchangeWep.maxMagAmmo, exchangeWep.pickupAmmo - exchangeWep.maxMagAmmo); //pickup are 2 mags
        if (wpslot.slotType == WeaponSlotType.Projectile)
            AddWeapon(exchangeWep, 0, exchangeWep.pickupAmmo);

        if (wpslot == slots[currentSlotId])
        {
            if (wpslot.slotType != WeaponSlotType.Projectile && wpslot.slotType != WeaponSlotType.Melee)
                ammoText.text = wpslot.magAmmo.ToString() + "/" + wpslot.unloadedAmmo.ToString();
            if (wpslot.slotType == WeaponSlotType.Projectile)
                ammoText.text = wpslot.unloadedAmmo.ToString();
        }

       
        ChangeToSlotAndUpdateWeapon(wpslot);

        Instantiate(weaponPickupBulletParticle, transform.position, Quaternion.identity);
        AudioManager.instance.PlayOneShot("weaponpickup");
        exchangeWeaponButton.SetActive(false);
        Destroy(lastEnteredWepPickup);
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.transform.CompareTag("Beartrap"))
        {
           if(!col.transform.GetComponent<Beartrap>().isClosed)
            {
                Stun(3);
                playerHealth.PlayerTakeDamage(UnityEngine.Random.Range(10, 15));
            }
            
            col.transform.GetComponent<Beartrap>().Close();
        }
    }

    public void Stun(float duration)
    {

        stunnedAnim.Play("StunnedFlash");
        AudioManager.instance.PlayOneShot("stunned");
        stunnedPanel.SetActive(false);
        stunnedPanel.SetActive(true);
       

        if (!isStunned)
        {

            isStunned = true;
            
            controller.currentSpeed = controller.stunnedSpeed;
            playerAnim.SetFloat("blendSpeedMultiplier", controller.currentSpeed / 4);
            stunnedPanel.SetActive(true);
        //    stunnedParticle.GetComponent<ParticleSystem>().Stop();
            stunnedParticle.GetComponent<ParticleSystem>().Play();
            Invoke(nameof(StunWearoff), duration);
            Invoke(nameof(StunLerpWearoff), duration * (93 / 100));
        } else
        {
            CancelInvoke(nameof(StunWearoff));
            Invoke(nameof(StunWearoff), duration);
            CancelInvoke(nameof(StunLerpWearoff));
            Invoke(nameof(StunLerpWearoff), duration * (93 / 100));
        }
        
    }

    void StunWearoff()
    {
        isStunned = false;
        controller.currentSpeed = controller.normalSpeed;
        playerAnim.SetFloat("blendSpeedMultiplier", controller.currentSpeed / 4);
        stunnedPanel.SetActive(false);
       // stunnedParticle.SetActive(false);
    }

    void StunLerpWearoff()
    {
        
        stunTimer = 0;
    }

 
    public void AlertAreaBombEntered(Transform bombTransform)
    {
        bombIndicator.gameObject.SetActive(true);
        isBombIndicatorActive = true;
        lastBombTransform = bombTransform;
        CancelInvoke(nameof(DisableBombIndicator));
        Invoke(nameof(DisableBombIndicator), 2);
    }

    public void AlertAreaBombExploded()
    {
         DisableBombIndicator();

    }

    void DisableBombIndicator()
    {
        bombIndicator.gameObject.SetActive(false);
        isBombIndicatorActive = false;
    }

    public void AlertAreaBombExit()
    {

    }

}