using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Agreement : MonoBehaviour, IDataPersistence
{

    public bool open;
    public GameObject panel, inputBlocker;
    public Toggle tick;
    public Button continueButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToCredits()
    {
        Application.OpenURL("https://sites.google.com/view/zombiemadness-credits/ana-sayfa?authuser=5");
    }


    public void GoToPrivacyPolicy()
    {
        Application.OpenURL("https://sites.google.com/view/zombie-madness-privacy-policy/ana-sayfa?authuser=5/");
    }

    public void GoToTermsAndConditions()
    {
        Application.OpenURL("https://sites.google.com/view/zombie-madness-termsconditions/ana-sayfa?authuser=5");
    }
    public void OnTickChanged()
    {
        if(tick.isOn)
        {
            continueButton.interactable = true;
        } else
        {
            continueButton.interactable = false;

        }
    }

    public void OnContinueButtonClicked()
    {
        PlayerPrefs.SetInt("agreedtermsandconditions", 1);
        inputBlocker.SetActive(false);
        panel.SetActive(false);
        AudioManager.instance.PlayOneShot("button2");
    }

    public void LoadData(GameData data)
    {
        if(PlayerPrefs.GetInt("agreedtermsandconditions") == 0)
        {
            if(open)
            {
                panel.SetActive(true);
                inputBlocker.SetActive(true);
            }
            
        } else if (PlayerPrefs.GetInt("agreedtermsandconditions") == 1)
        {

            panel.SetActive(false);
            inputBlocker.SetActive(false);
        }
    }

    public void SaveData(GameData data)
    {
       
    }
}
