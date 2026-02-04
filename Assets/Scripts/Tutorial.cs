using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public bool tutorialOn;
    public GameObject movementMarker, movementArrow, movementHighlighter, weaponSlotHL, fireHighlighter, jumpHightlihter, exampleCrate, crateArrow;
    public static Tutorial instance;
    public Text tutorialText;
    public string[] texts;
    public int step;
    bool crateGone;

    private void Awake()
    {
        if (instance == null)        
        instance = this;
       
 
    }


    void Start()
    {
        if (DataPersistenceManager.instance.gameData.lastDay != 1)
        {
            Destroy(exampleCrate);
            return;
        }
            

  
        tutorialOn = true;
        GameplayManager.instance.tutorialOn = true;
        //movementHighlighter.SetActive(true);
        movementMarker.SetActive(true);
        movementArrow.SetActive(true);
        weaponSlotHL.SetActive(false);
        step = 1;
        tutorialText.gameObject.SetActive(true);
        tutorialText.text = texts[0];
        GameplayManager.instance.dayBeginTextAnim.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(tutorialOn)
        {
            if (exampleCrate == null && !crateGone)
            {
                CrateBroken();
                crateGone = true;
            }
        }
       
    }

    public void WeaponSwitched()
    {
        if (DataPersistenceManager.instance.gameData.lastDay != 1)
            return;

        if (step != 2)
            return;

        tutorialText.text = texts[2];
        weaponSlotHL.SetActive(false);
        fireHighlighter.SetActive(true);
        step = 3;
    }

    public void MovementMarkerCollided()
    {
        if (DataPersistenceManager.instance.gameData.lastDay != 1)
            return;
        movementMarker.SetActive(false);
        movementArrow.SetActive(false);
        movementHighlighter.SetActive(false);
        tutorialText.text = texts[1];
        weaponSlotHL.SetActive(true);
        step = 2; 
    }

    public void Shooted()
    {
        if (step != 3)
            return;

        tutorialText.text = texts[3];
        fireHighlighter.SetActive(false);
        jumpHightlihter.SetActive(true);
        step = 4;
    }

    public void Jumped()
    {
        if (step != 4)
            return;


            tutorialText.text = texts[4];
            jumpHightlihter.SetActive(false);
        if(crateArrow != null)
        crateArrow.SetActive(true);
        step = 5;
        if (crateGone)
            CrateBroken();
    }

    void CrateBroken()
    {
        if (step != 5)
            return;

        tutorialText.text = texts[5];
        tutorialOn = false;
      //  crateArrow.SetActive(false);
        Invoke(nameof(DisableText), 4.5f);
        Invoke(nameof(RestartDay), 4);
    }

    void DisableText()
    {
        tutorialText.gameObject.SetActive(false);
        GameplayManager.instance.tutorialOn = false;
       
        GameplayManager.instance.dayBeginTextAnim.gameObject.SetActive(true);
    }

    void RestartDay()
    {
        GameplayManager.instance.TryAgainButtonPressed();
    }

}
