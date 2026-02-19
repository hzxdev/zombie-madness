using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
//using UnityEditor.Rendering.LookDev;
using UnityEngine;



public class Store : MonoBehaviour, IDataPersistence
{
    public static Store instance;
    public GameObject store, gemStore, coinStore, storeAreYouSure, exclamation, specialOfferKachujinStoreSlot, rewardPromptPanel;
    public TextMeshProUGUI storeAreYouText, rewardPromptText, kachujinProgressText;
    int currentOfferId;
    public int[] coinOfferPricesInGems;
    public int gemToCoinPrice = 250;

    public void Awake()
    {
        if(instance == null)
            instance = this;
    }

    public void OpenStore()
    {
        store.SetActive(true);
        GetComponent<Shop>().ShopClose(false);
        AudioManager.instance.PlayOneShot("panelopen");
        GemStore(false);

    }

    public void GemStore(bool sound)
    {
        coinStore.SetActive(false);
        gemStore.SetActive(true);
        if(sound)
        AudioManager.instance.PlayOneShot("button1");
    }

    public void CoinStore(bool sound)
    {
        coinStore.SetActive(true);
        gemStore.SetActive(false);
        if (sound)
            AudioManager.instance.PlayOneShot("button1");
    }

    public void CloseStore()
    {
        store.SetActive(false);
        AudioManager.instance.PlayOneShot("button2");
    }

    public void BuyCoin(int offer)
    {
        storeAreYouSure.SetActive(true);
        currentOfferId = offer - 1;
        switch (offer)
        {
            case 1:
                storeAreYouText.text = "Are you sure you want to purchase 250 coins for 1 gem?";
                break;
            case 2:
                storeAreYouText.text = "Are you sure you want to purchase 2500 coins for 10 gems?";
                break;
            case 3:
                storeAreYouText.text = "Are you sure you want to purchase 12500 coins for 50 gems?";
                break;
            case 4:
                storeAreYouText.text = "Are you sure you want to purchase 50000 coins for 200 gems?";
                break;
        }
    }

    public void StoreAreYouSureYes()
    {
        if (DataPersistenceManager.instance.gameData.gem >= coinOfferPricesInGems[currentOfferId])
        {
            DataPersistenceManager.instance.gameData.gem -= coinOfferPricesInGems[currentOfferId];

            DataPersistenceManager.instance.gameData.coin = DataPersistenceManager.instance.gameData.coin + coinOfferPricesInGems[currentOfferId] * gemToCoinPrice;
                DataPersistenceManager.instance.SaveGame();
                Shop.instance.coinText.text = DataPersistenceManager.instance.gameData.coin.ToString();
                Shop.instance.gemText.text = DataPersistenceManager.instance.gameData.gem.ToString();
                AudioManager.instance.PlayOneShot("purchase");
            
        }
        else
        { 
            AudioManager.instance.PlayOneShot("buzz");
            Shop.instance.OpenStoreRedirector(Currency.Gem);
            storeAreYouSure.SetActive(false);
        }
        storeAreYouSure.SetActive(false);

    }

    public void StoreAreYouSureNo()
    {
        storeAreYouSure.gameObject.SetActive(false);
        AudioManager.instance.PlayOneShot("button2");
    }

    public void WatchAd1Gem()
    {
        Rewarded.instance.WhichLastRewardedAd = WhichRewardedAd.Watch1AdGem;
        Rewarded.instance.ShowRewardedAd();
    }

    public void WatchAdToGetKachujin()
    {
        if(!DataPersistenceManager.instance.gameData.specialOffer1Completed)
        {
            Rewarded.instance.WhichLastRewardedAd = WhichRewardedAd.WatchAdToGetKachujin;
            Rewarded.instance.ShowRewardedAd();
        }
    }

    public void KachujinProgressUpdate()
    {
        kachujinProgressText.text = "WATCH AD! (" + DataPersistenceManager.instance.gameData.specialOffer1AdsWatched + "/3)";

    }
    public void KachujinReward()
    {
      //  specialOfferKachujinStoreSlot.SetActive(false);
        RewardPrompt("Kachujin");
    }

    public void RewardPromptPanelOk()
    {
        rewardPromptPanel.SetActive(false);
        AudioManager.instance.PlayOneShot("button2");

    }

    public void RewardPrompt(string rewardName)
    {
        rewardPromptPanel.SetActive(true);
        AudioManager.instance.PlayOneShot("redeem");
        rewardPromptText.text = "Congrats! You've earned the reward " + rewardName + "!";
    }

    public void Purchase10GemsCompleted()
    {
        DataPersistenceManager.instance.gameData.coin = DataPersistenceManager.instance.gameData.coin + coinOfferPricesInGems[currentOfferId] * gemToCoinPrice;
    }

    public void LoadData(GameData data)
    {
        if(data.specialOffer1Completed)
        {
            exclamation.SetActive(false);
        } else
        {
            exclamation.SetActive(true);
        }
        if (data.characters.ContainsKey("Kachujin"))
        {
            if (data.characters["Kachujin"])
                exclamation.SetActive(false);
        }

        kachujinProgressText.text = "WATCH AD! (" + DataPersistenceManager.instance.gameData.specialOffer1AdsWatched + "/3)";
    }

    public void SaveData(GameData data)
    {
        kachujinProgressText.text = "WATCH AD! (" + DataPersistenceManager.instance.gameData.specialOffer1AdsWatched + "/3)";
    }
}