using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour, IDataPersistence
{
    public Weapon wep;
    public GameObject locked;
    public Text text;
    Button button;

    public void LoadData(GameData data)
    {
        button = GetComponent<Button>();
        if (DataPersistenceManager.instance.gameData.lastDay >= wep.unlockDay)
        {
            if (locked != null)
                locked.SetActive(false);
            button.interactable = true;
            Debug.Log("hadi iyisin");

        }
        else
        {
            if (locked != null)
                locked.SetActive(true);
            button.interactable = false;
            text.text = "DAY " + wep.unlockDay.ToString();
        }
    }

    public void SaveData(GameData data)
    {
       
    }

    void Start()
    {
        button = GetComponent<Button>();
        if (DataPersistenceManager.instance.gameData.lastDay >= wep.unlockDay)
        {
            if (locked != null)
                locked.SetActive(false);
            button.interactable = true;
            Debug.Log("hadi iyisin");

        }
        else
        {
            if (locked != null)
                locked.SetActive(true);
            button.interactable = false;
            text.text = "DAY " + wep.unlockDay.ToString();
        }

    }
    
}
