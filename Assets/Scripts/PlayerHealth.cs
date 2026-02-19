using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameplayManager;

public class PlayerHealth : MonoBehaviour
{
    [HideInInspector]
    public static PlayerHealth instance;
    public Text healthText, armorText;
    public GameObject armorBar;
    public int maxHealth = 100, maxArmor = 100;
    int health, armor;
    public bool alive;
    Animator bloodVignetteEffect, anim;
    Controller controller;
    public GameObject reviveParticleEffect;
    
    public bool isReviveInvincible;
    Character character;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("birden fazla playerhealth var !");
    }

    void Start()
    {
        alive = true;

        maxHealth = WeaponsList.instance.GetCharacterByName(DataPersistenceManager.instance.gameData.equippedCharacter).maxHealth;
        health = maxHealth;
        healthText.text = health.ToString();
        // armor = maxArmor;
        bloodVignetteEffect = GameObject.Find("BloodVignette").GetComponent<Animator>();
        controller = GetComponent<Controller>();
        controller.alive = true;
        anim = GetComponent<Animator>();
        anim.SetBool("alive", true);
        GunController.instance.stunnedParticle.SetActive(true);
        character = GetComponent<CharacterManager>().currentCharInfo.character;
    }
   
    void Update()
    {
        if(!alive)
            GunController.instance.upperBodyRig.weight = 0; //updatede olmayýnca olmyuor
    }

    public void KillPlayer()
    {
        health = 0;
        armor = 0;
        PlayerTakeDamage(0);
    }

    public void PlayerTakeDamage(int damage)
    {
        if (!alive)
            return;

        if (isReviveInvincible)
            return;

        if(armor > 0)
        {
            if(damage > armor)
            {
                int difference = damage - armor;
                armor = 0;
                health -= difference;
            } else
            {
                armor -= damage;
            }


        } else
        {
            health -= damage;
        }

       



        if(health <= 0)
        {
            health = 0;
            controller.alive = false;
            alive = false;
            anim.SetBool("alive", false);
            GunController.instance.leftHandRig.weight = 0;
            GunController.instance.rightHandRig.weight = 0;
            GunController.instance.meleeAttacking = false;
            AudioManager.instance.PlayOneShot("deathcinematic");
            GunController.instance.Stun(3f);
            GunController.instance.stunnedParticle.SetActive(false);
            GameplayManager.instance.PlayerDeath();
            CancelInvoke(nameof(ReviveInvincibilityWearOff));
            Debug.Log("You died");
        }

        bloodVignetteEffect.SetTrigger("damage");

        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Hit Reaction"))
            anim.SetTrigger("hurt");

        if(GunController.instance.isReloading) //shouldnt  be able to reload when hit
        {
            GunController.instance.isReloading = false;
            anim.SetBool("isReloading", false);
            if(GunController.instance.GetCurrentWeapon().weaponType != WeaponType.Shotgun) //shotgun gets bugged if this is done
            anim.SetTrigger("forceCancelReload");
        }


        if(character.hurtSounds.Length != 0)
        AudioManager.instance.PlayOneShotDontIfPlaying(character.hurtSounds[Random.Range(0, character.hurtSounds.Length)]);



        healthText.text = health.ToString();
        armorText.text = armor.ToString();
        if(armor <= 0)
        {
            armorBar.SetActive(false);
        } else
        {
            armorBar.SetActive(true);
        }


    }

    public void PlayerAddHealth(int value)
    {
        health += value;
        if(health >= maxHealth)
        {
            health = maxHealth;
            healthText.text = health.ToString();
        }
        healthText.text = health.ToString();
    }

    public void PlayerAddArmor(int value)
    {
        armor += value;
        if (armor >= maxArmor)
        {
            armor = maxArmor;
            armorText.text = armor.ToString();
            
        }
        armorBar.SetActive(true);
    }


    public void Respawn()
    {
        alive = true;
        GunController.instance.upperBodyRig.weight = 1;
        controller.alive = true;
        GunController.instance.stunnedParticle.SetActive(true);
        health = maxHealth;
        healthText.text = health.ToString();
        anim.SetBool("alive", true);
    }

    public void ReviveByGemClicked()
    {
       if( DataPersistenceManager.instance.gameData.gem >= 1)
        {
            RevivePlayer();


            DataPersistenceManager.instance.gameData.gem -= 1;
            DataPersistenceManager.instance.SaveGame();

        }
     }

    public void ReviveByWatchAdClicked()
    {

    }

    public void RevivePlayer()
    {
        alive = true;
        
        GunController.instance.Revived();
        controller.alive = true;
        health = maxHealth;
        healthText.text = health.ToString();
        anim.SetBool("alive", true);
        Instantiate(reviveParticleEffect, transform.position, Quaternion.identity);
        GameplayManager.instance.Revived();
        isReviveInvincible = true;
        GunController.instance.stunnedParticle.SetActive(true);
        Invoke(nameof(ReviveInvincibilityWearOff), 4);
        AudioManager.instance.PlayOneShot("revive");
    }

    void ReviveInvincibilityWearOff()
    {
        isReviveInvincible = false;
    }

        
        

    
}
