using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum WhichRewardedAd
{
    Watch1AdGem,
    WatchAdToGetKachujin,
    Revive
}

public class Rewarded : MonoBehaviour
{

    public bool pc;
    public static Rewarded instance;
    public WhichRewardedAd WhichLastRewardedAd;
    RewardedAd rewardedAd;

    private void Awake()
    {
        if(pc)
        {
            gameObject.SetActive(false);
                return;
        }

        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    
    // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
    
    private string kachujinUnitId = "ca-app-pub-2998759451818554/5326894350";
    private string watchAdTo1GemUnitId = "ca-app-pub-2998759451818554/1279289396";
    private string reviveUnitId = "ca-app-pub-2998759451818554/1132498472";

    private string rewardedTestUnitId = "ca-app-pub-3940256099942544/5224354917";

    public string _adUnitId = "ca-app-pub-2998759451818554/1279289396";
#elif UNITY_IPHONE
  private string _adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
  private string _adUnitId = "unused";
#endif




    void Start()
    {
        if (pc)
        {
            gameObject.SetActive(false);
                return;
        }


        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            LoadRewardedAd();
        });
    }

    public void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest.Builder().Build();

        /*
#if UNITY_EDITOR

        RewardedAd.Load(rewardedTestUnitId, adRequest,
          (RewardedAd ad, LoadAdError error) =>
          {
              // if error is not null, the load request failed.
              if (error != null || ad == null)
              {
                  Debug.LogError("Rewarded ad failed to load an ad " +
                                 "with error : " + error.GetMessage());
                  return;
              }

              Debug.Log("Rewarded ad loaded with response : "
                        + ad.GetResponseInfo());

              rewardedAd = ad;
              RegisterEventHandlers(rewardedAd);
          });
        return;
#endif
        */
        // send the request to load the ad.
        RewardedAd.Load(_adUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error.GetMessage());
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                rewardedAd = ad;
                RegisterEventHandlers(rewardedAd);
            });
    }


    public void ShowRewardedAd()
    {
        const string rewardMsg =
            "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                if(WhichLastRewardedAd == WhichRewardedAd.Watch1AdGem)
                {
                    RewardUserWatchAd1Gem();
                }
                if (WhichLastRewardedAd == WhichRewardedAd.WatchAdToGetKachujin)
                {
                    RewardUserToProgressCharacterKachujin();
                }
                if (WhichLastRewardedAd == WhichRewardedAd.Revive)
                {
                    RewardUserRevive();
                }
            });
        }
    }


    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
            LoadRewardedAd();
        };
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RewardUserWatchAd1Gem()
    {
        DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 1;
        DataPersistenceManager.instance.SaveGame();
        CancelInvoke(nameof(UpdateWatchAd1GemTextTMPProblem));
        Invoke(nameof(UpdateWatchAd1GemTextTMPProblem), 0.75f);
       
    }

    public void RewardUserRevive()
    {
        PlayerHealth.instance.RevivePlayer();
    }

    public void RewardUserToProgressCharacterKachujin()
    {
        DataPersistenceManager.instance.gameData.specialOffer1AdsWatched = DataPersistenceManager.instance.gameData.specialOffer1AdsWatched + 1;
        if(DataPersistenceManager.instance.gameData.specialOffer1AdsWatched >= 3)
        {
            DataPersistenceManager.instance.gameData.specialOffer1Completed = true;
            DataPersistenceManager.instance.gameData.characters["Kachujin"] = true;
            CancelInvoke(nameof(KachujinProgressUpdateReward));
            Invoke(nameof(KachujinProgressUpdateReward), 0.75f);
        }
            DataPersistenceManager.instance.SaveGame();
        DataPersistenceManager.instance.LoadGame();
        CancelInvoke(nameof(KachujinProgressUpdate));
        Invoke(nameof(KachujinProgressUpdate), 0.75f);

    }

    public void KachujinProgressUpdate()
    {
        Store.instance.KachujinProgressUpdate();

    }

    void KachujinProgressUpdateReward()
    {
        Store.instance.KachujinReward();

    }

    void UpdateWatchAd1GemTextTMPProblem()
    {
        Store.instance.RewardPrompt("1 Gem");
    }



}
