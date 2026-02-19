using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameData
{
    public int lastDay;
    public int coin;
    public int gem;
    public SerializableDictionary<string, int> weaponsAmmo;
    public SerializableDictionary<string, bool> characters;
    public SerializableDictionary<int, bool> turrets;
    public string equippedCharacter;
    public int survivalHiScore;
    public bool completedStandardMode;
    public bool ealCodeRedeemed;
    public bool twitterCodeRedemeed;
    public bool tiktokCodeRedemeed;
    public bool stockCode1;
    public bool stockCode2;
    public bool stockCode3;
    public bool charmCode;
    public bool alexCode;
    public bool specialOffer1Completed;
    public int specialOffer1AdsWatched;

    // the values defined in this constructor will be the default values
    // the game starts with when there's no data to load
    public GameData()
    {
        lastDay = 1;
        coin = 500;
        gem = 10;
        survivalHiScore = 0;
        weaponsAmmo = new SerializableDictionary<string, int>();
        characters = new SerializableDictionary<string, bool>();
        turrets = new SerializableDictionary<int, bool>();
        equippedCharacter = "SWAT";
        completedStandardMode = false;
        ealCodeRedeemed = false;
        twitterCodeRedemeed = false;
        tiktokCodeRedemeed = false;
        stockCode1 = false;
        stockCode2 = false;
        stockCode3 = false;
        alexCode = false;
        charmCode = false;
        specialOffer1Completed = false;
        specialOffer1AdsWatched = 0;
    }
}