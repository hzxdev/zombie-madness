using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour
{
    public bool pc;
    public GameObject IAPresultpanel;
    public TextMeshProUGUI IAPresultText;
    string gems10 = "com.hzxgames.zombiemadness.gems10";
    string gems50 = "com.hzxgames.zombiemadness.gems50";
    string gems200 = "com.hzxgames.zombiemadness.gems200";

    private void Awake()
    {
        if (pc)
        {
            gameObject.SetActive(false);
            return;
        }
    }

        public void OnPurchaseCompleted(Product product)
    {
        if(product.definition.id == gems10)
        {
            //bought 10 gems
            DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 10;
            DataPersistenceManager.instance.SaveGame();
     
        }
        if (product.definition.id == gems50)
        {
            //bought 50 gems
            DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 50;
            DataPersistenceManager.instance.SaveGame();
        }
        if (product.definition.id == gems200)
        {
            //bought 200 gems
            DataPersistenceManager.instance.gameData.gem = DataPersistenceManager.instance.gameData.gem + 200;
            DataPersistenceManager.instance.SaveGame();
        }
        IAPresultpanel.SetActive(true);
        IAPresultText.text = "Purchasing was successful!";
        AudioManager.instance.PlayOneShot("purchase");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        IAPresultpanel.SetActive(true);
        IAPresultText.text = "Purchasing failed.\nReason: " + reason;
        AudioManager.instance.PlayOneShot("buzz");
    }

    public void IAPresultPanelClose()
    {
        IAPresultpanel.SetActive(false);
        AudioManager.instance.PlayOneShot("button2");
    }
}
