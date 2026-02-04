using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEditor;
using UnityEngine.Rendering;
using TMPro;

public class StartMenu : MonoBehaviour, IDataPersistence
{
    public Animator blackout;
    bool loadingGameScene;
    public GameObject infoPanel, settingsPanel, infoPanelButtons, infoPanelCreditsContent, resurrectionButton, survivalButton;
    public GameObject loadingText;
    public Scrollbar settingsScrollBar, infoScrollBar;
    public Slider sfxSlider, musicSlider;
    public Dropdown resolutionDropdown;
    public AudioSource bgMusic;
    public Text dayText, coinText, gemText;
    public AudioMixer masterMix;
    public InputField redeemIF;
    public Toggle postProcessTogle;
    public TextMeshProUGUI lockedPromptText;
    int resolutionDegree;
    [HideInInspector]
    public bool survivalUnlocked, resurrectionUnlocked;
    public GameObject survivalLockedIcon, redeemResultPanel;
    public TextMeshProUGUI survivalHiScoreText, redeemResultPanelText;
    public Button redeemButton;
   
     string ealRedeemCode = "ealligenç", developerRedeemCode = "deVstuff19+",
         tiktokCode = "426831",  twitterCode = "537175", stockCode1 = "248054", stockCode2 = "105738"
        , stockCode3 = "046739", alexCode = "649382", charmCode = "473951";
    public int gemAmount, coinAmount;
    

    void Start()
    {
        Application.targetFrameRate = 60;


        switch (PlayerPrefs.GetInt("resolution"))
        {
            case 10:
                resolutionDropdown.value = 0;
                break;
            case 9:
                resolutionDropdown.value = 1;
                break;
            case 8:
                resolutionDropdown.value = 2;
                break;
            case 7:
                resolutionDropdown.value = 3;
                break;
            case 6:
                resolutionDropdown.value = 4;
                break;

        }


        if (PlayerPrefs.GetInt("postprocess") == 1)
        {
            postProcessTogle.isOn = true;
          //  GameObject.FindObjectOfType<Volume>().weight = 1;
        }
           
        else
        {
           // GameObject.FindObjectOfType<Volume>().weight = 0;
            postProcessTogle.isOn = false;
        }

        if(PlayerPrefs.GetInt("resolution") == 0)
        {
            PlayerPrefs.SetInt("resolution", 7);
            ChangeResolution(7);
        }
        else
        ChangeResolution(PlayerPrefs.GetInt("resolution"));

        if(PlayerPrefs.GetInt("postprocess") == 0)

        loadingText.SetActive(false);


       
            

        sfxSlider.value = PlayerPrefs.GetFloat("sfx");
        musicSlider.value = PlayerPrefs.GetFloat("music");
    }
    
    public void InfoPanelOpenCredits()
    {
        AudioManager.instance.PlayOneShot("button1");
        infoPanelButtons.SetActive(false);
        infoPanelCreditsContent.SetActive(true);
    }
    public void PostProcessToggle()
    {
        if(postProcessTogle.isOn)
        {
            PlayerPrefs.SetInt("postprocess", 1);
           // GameObject.FindObjectOfType<Volume>().weight = 1;
        }
        
        else
        {
        //    GameObject.FindObjectOfType<Volume>().weight = 0;
            PlayerPrefs.SetInt("postprocess", 0);
        }
           

    }

    public void ResolutionDropdownValueChanged()
    {
        switch(resolutionDropdown.value)
        {
            case 0:
                ChangeResolution(10);
                PlayerPrefs.SetInt("resolution", 10);
                break;
            case 1:
                ChangeResolution(9);
                PlayerPrefs.SetInt("resolution", 9);
                break;
            case 2:
                ChangeResolution(8);
                PlayerPrefs.SetInt("resolution", 8);
                break;
            case 3:
                ChangeResolution(7);
                PlayerPrefs.SetInt("resolution", 7);
                break;
            case 4:
                ChangeResolution(6);
                PlayerPrefs.SetInt("resolution", 6);
                break;
        }
    }

    void ChangeResolution(int qualityOutOfTen)
    {
        int xx = Display.main.systemWidth;
        int yy = Display.main.systemHeight;

        Screen.SetResolution(Convert.ToInt32((xx * qualityOutOfTen / 10)), Convert.ToInt32(yy * qualityOutOfTen / 10), true);
        Camera.main.aspect = xx / yy;
    }

    public void ContinueButtonPressed()
    {
        if(loadingGameScene == false)
        {
            blackout.SetTrigger("levelLoaded");
            AudioManager.instance.PlayOneShot("starthit");
            DataPersistenceManager.instance.currentGamemode = GameMode.Standard;

            //start lerping bg music

            loadingText.SetActive(true);
            loadingGameScene = true;
            Invoke(nameof(StartDay), 2f);
        }
       

    }

    public void SurvivalButtonPressed()
    {
        if(survivalUnlocked)
        {
            if (loadingGameScene == false)
            {
                blackout.SetTrigger("levelLoaded");
                AudioManager.instance.PlayOneShot("starthit");
                DataPersistenceManager.instance.currentGamemode = GameMode.Survival;

                //start lerping bg music

                loadingText.SetActive(true);
                loadingGameScene = true;
                Invoke(nameof(StartDay), 2f);
            }
        } else
        {
            lockedPromptText.text = "Unlocked after Day 5";
            lockedPromptText.gameObject.SetActive(false);
            lockedPromptText.gameObject.SetActive(true);
            AudioManager.instance.PlayOneShot("buzz");
        }
       
    }

    public void ResurrectionButtonPressed()
    {
        if (resurrectionUnlocked)
        {
            if (loadingGameScene == false)
            {
                blackout.SetTrigger("levelLoaded");
                AudioManager.instance.PlayOneShot("starthit");
                DataPersistenceManager.instance.currentGamemode = GameMode.Resurrection;

                //start lerping bg music

                loadingText.SetActive(true);
                loadingGameScene = true;
                Invoke(nameof(StartDay), 2f);
            }
        }
        else
        {
            lockedPromptText.text = "Coming soon...";
            lockedPromptText.gameObject.SetActive(false);
            lockedPromptText.gameObject.SetActive(true);
            AudioManager.instance.PlayOneShot("buzz");
        }

    }

    public void SwitchMode()
    {
       
        if(resurrectionButton.activeSelf)
        {
            resurrectionButton.SetActive(false);
            survivalButton.SetActive(true);

        } else
        {
            resurrectionButton.SetActive(true);
            survivalButton.SetActive(false);
        }
        AudioManager.instance.PlayOneShot("button1");
    }
    public void StartDay()
    {
       
        DataPersistenceManager.instance.SaveGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
       
    }

    public void InfoButtonOpen()
    {
        AudioManager.instance.PlayOneShot("button1");
        infoPanel.SetActive(true);
        infoScrollBar.value = 1;
    }

    public void InfoButtonClose()
    {
        AudioManager.instance.PlayOneShot("button2");
        infoPanelCreditsContent.SetActive(false);
        infoPanelButtons.SetActive(true);
        infoPanel.SetActive(false);
    }

    public void SettingsButtonOpen()
    {
        settingsPanel.SetActive(true);
        settingsScrollBar.value = 1;
        AudioManager.instance.PlayOneShot("button1");
    }

    public void SettingsButtonClose()
    {
        settingsPanel.SetActive(false);
        AudioManager.instance.PlayOneShot("button2");
    }

    public void WipePlayerData()
    {
        PlayerPrefs.DeleteAll();
        sfxSlider.value = 5;
        musicSlider.value = 5;
        PlayerPrefs.SetFloat("music",5);
        PlayerPrefs.SetFloat("sfx", 5);

        DataPersistenceManager.instance.NewGame();
        UpdateMusicValue();
    }

    public void RedeemCode()
    {
        /*
        if(redeemIF.text == "para")
        {
            DataPersistenceManager.instance.gameData.coin = DataPersistenceManager.instance.gameData.coin + 10000;
            DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 100;
            DataPersistenceManager.instance.SaveGame();
        }

        if (redeemIF.text == "skipday")
        {
            DataPersistenceManager.instance.gameData.lastDay = DataPersistenceManager.instance.gameData.lastDay + 1;
            DataPersistenceManager.instance.SaveGame();
            
        }

        if (redeemIF.text == "backday")
        {
            DataPersistenceManager.instance.gameData.lastDay = DataPersistenceManager.instance.gameData.lastDay - 1;
            DataPersistenceManager.instance.SaveGame();

        }*/


      /*  if (redeemIF.text == developerRedeemCode)
        {
            
            
                DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 500;
                DataPersistenceManager.instance.SaveGame();
                RedeemCodeResultPanel(true);
                return;
                    
        }
        */

        if (redeemIF.text == ealRedeemCode)
        {
            if(DataPersistenceManager.instance.gameData.ealCodeRedeemed == false)
            {
                DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 100;
                DataPersistenceManager.instance.gameData.ealCodeRedeemed = true;
                DataPersistenceManager.instance.SaveGame();
                RedeemCodeResultPanel(true);
                redeemResultPanelText.text = "Maþallah EAL! (Biraz geciktin bro, artýk 100 elmas veriyoruz)";
                return;
            } else
            {
                RedeemCodeResultPanel(false);
            } 
      
        }

        if (redeemIF.text == tiktokCode)
        {
            if (DataPersistenceManager.instance.gameData.tiktokCodeRedemeed == false)
            {
                DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 100;
                DataPersistenceManager.instance.gameData.tiktokCodeRedemeed = true;
                DataPersistenceManager.instance.SaveGame();
                RedeemCodeResultPanel(true);
                return;
            }
            else
            {
                RedeemCodeResultPanel(false);
            }

        }


        if (redeemIF.text == twitterCode)
        {
            if (DataPersistenceManager.instance.gameData.twitterCodeRedemeed == false)
            {
                DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 100;
                DataPersistenceManager.instance.gameData.twitterCodeRedemeed = true;
                DataPersistenceManager.instance.SaveGame();
                RedeemCodeResultPanel(true);
                return;
            }
            else
            {
                RedeemCodeResultPanel(false);
            }

        }

        if (redeemIF.text == stockCode1)
        {
            if (DataPersistenceManager.instance.gameData.stockCode1 == false)
            {
                DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 50;
                DataPersistenceManager.instance.gameData.stockCode1 = true;
                DataPersistenceManager.instance.SaveGame();
                RedeemCodeResultPanel(true);
                return;
            }
            else
            {
                RedeemCodeResultPanel(false);
            }

        }

        if (redeemIF.text == stockCode2)
        {
            if (DataPersistenceManager.instance.gameData.stockCode2 == false)
            {
                DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 100;
                DataPersistenceManager.instance.gameData.stockCode2 = true;
                DataPersistenceManager.instance.SaveGame();
                RedeemCodeResultPanel(true);
                return;
            }
            else
            {
                RedeemCodeResultPanel(false);
            }

        }

        if (redeemIF.text == stockCode3)
        {
            if (DataPersistenceManager.instance.gameData.stockCode3 == false)
            {
                DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 200;
                DataPersistenceManager.instance.gameData.stockCode3 = true;
                DataPersistenceManager.instance.SaveGame();
                RedeemCodeResultPanel(true);
                return;
            }
            else
            {
                RedeemCodeResultPanel(false);
            }

        }

        if (redeemIF.text == alexCode)
        {
            if (DataPersistenceManager.instance.gameData.alexCode == false)
            {
                DataPersistenceManager.instance.gameData.characters["Alex"] = true;
                DataPersistenceManager.instance.gameData.alexCode = true;
                DataPersistenceManager.instance.SaveGame();
                RedeemCodeResultPanel(true);
                Shop.instance.UnlockShop();
                return;
            }
            else
            {
                RedeemCodeResultPanel(false);
            }

        }

        if (redeemIF.text == charmCode)
        {
            if (DataPersistenceManager.instance.gameData.charmCode == false)
            {
                DataPersistenceManager.instance.gameData.characters["Charm"] = true;
                DataPersistenceManager.instance.gameData.charmCode = true;
                DataPersistenceManager.instance.SaveGame();
                RedeemCodeResultPanel(true);
                Shop.instance.UnlockShop();
                return;
            }
            else
            {
                RedeemCodeResultPanel(false);
            }

        }

        RedeemCodeResultPanel(false);
        


    }

    public void RedeemCodeResultPanel(bool success)
    {
        redeemResultPanel.SetActive(true);
        redeemIF.text = "";

        if(success)
        {
            AudioManager.instance.PlayOneShot("redeem");
            redeemResultPanelText.text = "Redeeming code was succesful!";
        } else
        {
            redeemResultPanelText.text = "Code doesn't exist or was used before.";
            redeemButton.interactable = false;
            AudioManager.instance.PlayOneShot("buzz");
            CancelInvoke(nameof(ReactivateRedeemButton));
            Invoke(nameof(ReactivateRedeemButton), 2);
        }
    }

    public void RedeemResultPanelClose()
    {
        redeemResultPanel.SetActive(false);
        AudioManager.instance.PlayOneShot("button2");
    }

    void ReactivateRedeemButton()
    {
        redeemButton.interactable = true;
    }

    public void UpdateSliderValues()
    {
        PlayerPrefs.SetFloat("music", musicSlider.value);
        PlayerPrefs.SetFloat("sfx", sfxSlider.value);
        UpdateMusicValue();
    }

    public void OnSFXSliderValueChange()
    {
        if (sfxSlider.value == 0)
            masterMix.SetFloat("SFX", Mathf.Log10(0.001f) * 20);
        else
            masterMix.SetFloat("SFX", Mathf.Log10(sfxSlider.value / 10) * 20);

        PlayerPrefs.SetFloat("sfx", sfxSlider.value);
    }

    public void OnMusicSliderValueChange()
    {
        if (musicSlider.value == 0)
            masterMix.SetFloat("Music", Mathf.Log10(0.001f) * 20);
        else
            masterMix.SetFloat("Music", Mathf.Log10(musicSlider.value / 10) * 20);

        PlayerPrefs.SetFloat("music", musicSlider.value);
    }

    void UpdateMusicValue()
    {
        bgMusic.volume = musicSlider.value/10;
    }

    public void LoadData(GameData data)
    {
        dayText.text = "DAY " + data.lastDay.ToString();
        coinText.text = data.coin.ToString();
        gemText.text = data.gem.ToString();     
        
        if(data.lastDay > 5)
        {
            survivalUnlocked = true;
         //   resurrectionUnlocked = true;
            survivalHiScoreText.gameObject.SetActive(true);
            survivalHiScoreText.text = "HI-SCORE: " + data.survivalHiScore.ToString();
            survivalLockedIcon.SetActive(false);
        } else
        {
            survivalUnlocked = false;
            resurrectionUnlocked = false;
            survivalHiScoreText.gameObject.SetActive(false);
            survivalLockedIcon.SetActive(true);
        }
    }

    public void UpdateCurrencyTexts()
    {
        coinText.text = DataPersistenceManager.instance.gameData.coin.ToString();
        gemText.text = DataPersistenceManager.instance.gameData.gem.ToString();
    }

    public void SaveData(GameData data)
    {
        dayText.text = "DAY " + data.lastDay.ToString();
        coinText.text = data.coin.ToString();
        gemText.text = data.gem.ToString();
    }
}
