using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.TextCore.Text;

public class Shop : MonoBehaviour, IDataPersistence
{
    public static Shop instance;
    public GameObject shopScroll, weaponsTab, characterTab, previewPanel, areYouSurePanel, buyButton, weaponAmmoIcon, storeRedirectorPrompt;
    public Text infoText, weaponNameText, weaponAmmoInventory, priceText, areYouSureText, ammoWasteText, buyMagText, storeRedirectorText;
    public Text coinText, gemText;
    public Image buyButtonCurrencyImage;
    public RawImage characterPreviewImage, weaponPreviewImage;
    public Transform charPreviewModelsParent, wepPreviewModelsParent;
    public Color gemColor, coinColor;
    public Sprite gemSprite, coinSprite;
    AudioSource source;
    public AudioClip[] sounds;
    StartMenu startMenu;
    Weapon lastSelectedWep;
    Character lastSelectedChar;
    bool shopUnlocked;
    public CanvasGroup shopButtonGroup;
    public GameObject shopLockIcon, ammoWasteWarning;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    void Start()
    {
        source = GetComponent<AudioSource>();
        startMenu = GetComponent<StartMenu>();
        for (int i = 0; i < charPreviewModelsParent.transform.childCount; i++)
        {
            charPreviewModelsParent.transform.GetChild(i).gameObject.SetActive(false);
        }

        charPreviewModelsParent.transform.GetChild(WeaponsList.instance.GetCharacterByName(DataPersistenceManager.instance.gameData.equippedCharacter).characterId).gameObject.SetActive(true);
    }
    public void OnShopButtonClicked()
    {
        if (shopUnlocked)
        {
            coinText.text = DataPersistenceManager.instance.gameData.coin.ToString();
            gemText.text = DataPersistenceManager.instance.gameData.gem.ToString();
            shopScroll.SetActive(true);
            previewPanel.SetActive(true);
            CharactersTabClicked(false);
            WeaponsTabClicked(false);
            AudioManager.instance.PlayOneShot("panelopen");
        } else
        {
            GetComponent<StartMenu>().lockedPromptText.gameObject.SetActive(false);
            GetComponent<StartMenu>().lockedPromptText.gameObject.SetActive(true);
            GetComponent<StartMenu>().lockedPromptText.text = "Unlocked after Day 2";
            AudioManager.instance.PlayOneShot("buzz");
        }

    }
    public void WeaponsTabClicked(bool sound)
    {
        weaponsTab.SetActive(true);
        characterTab.SetActive(false);
        if (sound)
            AudioManager.instance.PlayOneShot("button1");
        lastSelectedChar = null;

        buyButton.SetActive(false);
        weaponNameText.text = "";
        weaponPreviewImage.gameObject.SetActive(false);
        buyMagText.text = "";
        weaponAmmoInventory.text = "";
        weaponAmmoIcon.SetActive(false);
        infoText.text = "";
        characterPreviewImage.gameObject.SetActive(false);
    }
    public void CharactersTabClicked(bool sound)
    {
        lastSelectedWep = null;

        weaponsTab.SetActive(false);
        characterTab.SetActive(true);
        if(sound)
        AudioManager.instance.PlayOneShot("button1");
        weaponNameText.text = "";
        buyButton.SetActive(false);
        weaponPreviewImage.gameObject.SetActive(false);
        buyMagText.text = "";
        weaponAmmoInventory.text = "";
        weaponAmmoIcon.SetActive(false);
        infoText.text = "";
    }
    public void UpdatePreviewPanelValues(Weapon weapon)
    {
        infoText.text = "MAG: " + weapon.maxMagAmmo + "\n DAMAGE: \n Limb: " + weapon.limbDamage + " Torso: " + weapon.bodyDamage + " Head: " + weapon.headDamage + "\n FIRING SPEED: " + (1 / weapon.cooldown).ToString("f0");
       
        weaponNameText.text = weapon.weaponName;
        priceText.text = weapon.magPrice.ToString();
        lastSelectedWep = weapon;
        buyButton.SetActive(true);
        weaponAmmoIcon.SetActive(true);
        weaponPreviewImage.gameObject.SetActive(true);
        characterPreviewImage.gameObject.SetActive(false);

        for (int i = 0; i < wepPreviewModelsParent.childCount; i++)
        {
            wepPreviewModelsParent.GetChild(i).gameObject.SetActive(false);
        }

        wepPreviewModelsParent.GetChild(lastSelectedWep.weaponId).gameObject.SetActive(true);

        buyButtonCurrencyImage.gameObject.SetActive(true);
        buyButtonCurrencyImage.sprite = coinSprite;
        buyButtonCurrencyImage.color = coinColor;
        buyButton.GetComponent<Button>().interactable = true;
        if (weapon.weaponType != WeaponType.Projectile)
            buyMagText.text = "BUY 1 MAG";
        else
            buyMagText.text = "BUY 5";
        if (DataPersistenceManager.instance.gameData.weaponsAmmo.ContainsKey(weapon.weaponName))
        {
            weaponAmmoInventory.text = DataPersistenceManager.instance.gameData.weaponsAmmo[weapon.weaponName].ToString();
        }
        else
        {
            Debug.Log("Doesnt exist usta!");
        }
    }
    public void UpdatePreviewPanelValues(Character character)
    {
        lastSelectedChar = character;
        infoText.text = "HEALTH: " + character.maxHealth + "\n" + "SPEED: " + character.speed + "\n" + character.characterDescription;
        characterPreviewImage.gameObject.SetActive(true);

        for (int i = 0; i < charPreviewModelsParent.transform.childCount; i++)
        {
            charPreviewModelsParent.transform.GetChild(i).gameObject.SetActive(false);
        }

        charPreviewModelsParent.transform.GetChild(character.characterId).gameObject.SetActive(true);
        characterPreviewImage.SetNativeSize();
        weaponNameText.text = character.characterName;
        if (DataPersistenceManager.instance.gameData.characters[character.characterName] == true) // if owned
        {
            buyButtonCurrencyImage.gameObject.SetActive(false);
            if (WeaponsList.instance.GetCharacterByName(DataPersistenceManager.instance.gameData.equippedCharacter) == character)// if selected character is equiiped
            {
                buyMagText.text = "EQUIPPED";
                priceText.text = "";
                buyButton.GetComponent<Button>().interactable = false;
            }
            else
            {
                buyMagText.text = "EQUIP";
                priceText.text = "";
                buyButton.GetComponent<Button>().interactable = true;
            }
        }
        else
        {
            buyButton.GetComponent<Button>().interactable = true;
            buyMagText.text = "BUY CHARACTER";
            priceText.text = character.characterPrice.ToString();
            buyButtonCurrencyImage.gameObject.SetActive(true);
            if (character.characterCurrency == Currency.Coin)
            {
                buyButtonCurrencyImage.sprite = coinSprite;
                buyButtonCurrencyImage.color = coinColor;
            }
            if (character.characterCurrency == Currency.Gem)
            {
                buyButtonCurrencyImage.sprite = gemSprite;
                buyButtonCurrencyImage.color = gemColor;
            }
        }

        if(lastSelectedChar.obtainedBy == Obtained.Store)
        buyButton.SetActive(true);
        else if(lastSelectedChar.obtainedBy == Obtained.Special)
        {
            if (DataPersistenceManager.instance.gameData.characters[lastSelectedChar.characterName])
            {// we own the special obtained character
                buyButton.SetActive(true);
            } else
            { // wedont
                buyButton.SetActive(false);
            }
        }
          

        weaponAmmoIcon.SetActive(false);

        weaponAmmoInventory.text = "";

    }
    public void OpenAreYouSurePanel()
    {
        if (lastSelectedWep != null && lastSelectedChar == null) //buying a weapon
        {
            ammoWasteWarning.SetActive(false);
            areYouSurePanel.SetActive(true);
            areYouSureText.text = "Are you sure you want to purchase\n one mag (" + lastSelectedWep.maxMagAmmo + " ammos) of " + lastSelectedWep.weaponName + "\n for " + lastSelectedWep.magPrice + " coins?";
            for (int i = 0; i < DataPersistenceManager.instance.gameData.weaponsAmmo.Count; i++)
            {
                if (WeaponTypeToSlotType(WeaponsList.instance.GetWeaponByName(DataPersistenceManager.instance.gameData.weaponsAmmo.ElementAt(i).Key).weaponType) == WeaponTypeToSlotType(lastSelectedWep.weaponType) && WeaponsList.instance.GetWeaponByName(DataPersistenceManager.instance.gameData.weaponsAmmo.ElementAt(i).Key) != lastSelectedWep)
                {
                    if (DataPersistenceManager.instance.gameData.weaponsAmmo.ElementAt(i).Value > 0)
                    {
                        ammoWasteWarning.SetActive(true);
                        ammoWasteText.text = "(You can only have one " + WeaponTypeToSlotType(lastSelectedWep.weaponType).ToString() + " in your inventory at once\nif you proceed, other " + lastSelectedWep.weaponType.ToString() + " ammos will be <color=red>WASTED</color>)";
                    }
                }
            }
        }
        else //buying a character
        {
            if (DataPersistenceManager.instance.gameData.characters[lastSelectedChar.characterName])
            //we own the char, equip
            {
        
                DataPersistenceManager.instance.gameData.equippedCharacter = lastSelectedChar.characterName;
                DataPersistenceManager.instance.SaveGame();
                UpdatePreviewPanelValues(lastSelectedChar);
               // ShopClose();
                AudioManager.instance.PlayOneShot("charequip");
            }
            else // we dont own
            {
                ammoWasteWarning.SetActive(false);
                areYouSurePanel.SetActive(true);
                areYouSureText.text = "Are you sure you want to purchase\nthe character " + lastSelectedChar.characterName + "?";
            }

        }


    }
    public void ShopClose(bool sound)
    {
        shopScroll.SetActive(false);
        previewPanel.SetActive(false);
        startMenu.UpdateCurrencyTexts();
        if(sound)
        AudioManager.instance.PlayOneShot("button2");

        for (int i = 0; i < charPreviewModelsParent.transform.childCount; i++)
        {
            charPreviewModelsParent.transform.GetChild(i).gameObject.SetActive(false);
        }

        charPreviewModelsParent.transform.GetChild(WeaponsList.instance.GetCharacterByName(DataPersistenceManager.instance.gameData.equippedCharacter).characterId).gameObject.SetActive(true);
    }
    public void ShopPurchaseYes()
    {
        if (lastSelectedWep != null && lastSelectedChar == null) //buying a wep
        {
            if (DataPersistenceManager.instance.gameData.coin >= lastSelectedWep.magPrice)
            {
                DataPersistenceManager.instance.gameData.coin -= lastSelectedWep.magPrice;
                if (DataPersistenceManager.instance.gameData.weaponsAmmo.ContainsKey(lastSelectedWep.weaponName))
                {
                    if (lastSelectedWep.weaponType != WeaponType.Projectile)
                        DataPersistenceManager.instance.gameData.weaponsAmmo[lastSelectedWep.weaponName] = DataPersistenceManager.instance.gameData.weaponsAmmo[lastSelectedWep.weaponName] + lastSelectedWep.maxMagAmmo;
                    else
                        DataPersistenceManager.instance.gameData.weaponsAmmo[lastSelectedWep.weaponName] = DataPersistenceManager.instance.gameData.weaponsAmmo[lastSelectedWep.weaponName] + 5;
                    for (int i = 0; i < DataPersistenceManager.instance.gameData.weaponsAmmo.Count; i++)
                    {
                        if (WeaponTypeToSlotType(WeaponsList.instance.GetWeaponByName(DataPersistenceManager.instance.gameData.weaponsAmmo.ElementAt(i).Key).weaponType) == WeaponTypeToSlotType(lastSelectedWep.weaponType) && WeaponsList.instance.GetWeaponByName(DataPersistenceManager.instance.gameData.weaponsAmmo.ElementAt(i).Key) != lastSelectedWep)
                        {
                            if (DataPersistenceManager.instance.gameData.weaponsAmmo.ElementAt(i).Value > 0)
                            {
                                DataPersistenceManager.instance.gameData.weaponsAmmo[DataPersistenceManager.instance.gameData.weaponsAmmo.ElementAt(i).Key] = 0;
                            }
                        }
                    }
                    DataPersistenceManager.instance.SaveGame();
                    UpdatePreviewPanelValues(lastSelectedWep);
                    coinText.text = DataPersistenceManager.instance.gameData.coin.ToString();
                    gemText.text = DataPersistenceManager.instance.gameData.gem.ToString();
                    AudioManager.instance.PlayOneShot("purchase");
                }
                else
                {
                    Debug.Log("Doesnt exist amk!");
                }
            }
            else
            {
                Debug.Log("Not enough coins!");
                AudioManager.instance.PlayOneShot("buzz");
                OpenStoreRedirector(Currency.Coin);
                areYouSurePanel.SetActive(false);
            }
            areYouSurePanel.SetActive(false);
        }
        else //buying a char
        {
            if (!DataPersistenceManager.instance.gameData.characters[lastSelectedChar.characterName])
            // we dont own the char, purchase
            {
                if (lastSelectedChar.characterCurrency == Currency.Coin)
                {
                    if (DataPersistenceManager.instance.gameData.coin >= lastSelectedChar.characterPrice)
                    {
                        DataPersistenceManager.instance.gameData.coin -= lastSelectedChar.characterPrice;
                    }
                    else
                    {
                        Debug.Log("Not enough coins!");
                        OpenStoreRedirector(Currency.Coin);
                        AudioManager.instance.PlayOneShot("buzz");
                        areYouSurePanel.SetActive(false);
                        return;
                    }
                }
                if (lastSelectedChar.characterCurrency == Currency.Gem)
                {
                    if (DataPersistenceManager.instance.gameData.gem >= lastSelectedChar.characterPrice)
                    {
                        DataPersistenceManager.instance.gameData.gem -= lastSelectedChar.characterPrice;
                    }
                    else
                    {
                        Debug.Log("Not enough gems!");
                        OpenStoreRedirector(Currency.Gem);
                        AudioManager.instance.PlayOneShot("buzz");
                        areYouSurePanel.SetActive(false);
                        return;
                    }
                }
                //returnlemeleri yaptik, satin almis gibi devam
                if (DataPersistenceManager.instance.gameData.characters.ContainsKey(lastSelectedChar.characterName))
                //saglama aldik
                {
                    DataPersistenceManager.instance.gameData.characters[lastSelectedChar.characterName] = true; //satin al
                    DataPersistenceManager.instance.gameData.equippedCharacter = lastSelectedChar.characterName; //equiple
                    DataPersistenceManager.instance.SaveGame();
                    UpdatePreviewPanelValues(lastSelectedChar);
                    coinText.text = DataPersistenceManager.instance.gameData.coin.ToString();
                    gemText.text = DataPersistenceManager.instance.gameData.gem.ToString();
                    AudioManager.instance.PlayOneShot("purchase");
                }
            }
            areYouSurePanel.SetActive(false);
        }

    }

    public void OpenStoreRedirector(Currency currency)
    {
        AudioManager.instance.PlayOneShot("smallui1");
        storeRedirectorPrompt.SetActive(true);
        if (currency == Currency.Coin)
        {
           
            storeRedirectorText.text = "You don't have enough coins.\r\nDo you wish to buy more?";
            Store.instance.CoinStore(false);
        } else if(currency == Currency.Gem)
        {
            Store.instance.GemStore(false);
            storeRedirectorText.text = "You don't have enough gems.\r\nDo you wish to buy more?";
        }
    }

    public void StoreRedirectorYes()
    {
        Store.instance.OpenStore();
        storeRedirectorPrompt.SetActive(false);
    }

    public void StoreRedirectorNo()
    {
        storeRedirectorPrompt.SetActive(false);
    }


    public void ShopPurchaseCancel()
    {
        areYouSurePanel.SetActive(false);
    }
    public void LoadData(GameData data)
    {
        
    }
    public void SaveData(GameData data)
    {
        if (data.lastDay == 1) //Shop is closed the first day
        {
            shopUnlocked = false;
            shopButtonGroup.alpha = 0.5f;
            shopLockIcon.SetActive(true);
        }
        else
        {
            shopUnlocked = true;
            shopButtonGroup.alpha = 1;
            shopLockIcon.SetActive(false);
        }

        if(data.characters.ContainsKey("Kachujin"))
        {
            if (data.characters["Kachujin"])
            {
                shopUnlocked = true;
                shopButtonGroup.alpha = 1;
                shopLockIcon.SetActive(false);
            }
        }
       
    }

    public void UnlockShop()
    {
        shopUnlocked = true;
        shopButtonGroup.alpha = 1;
        shopLockIcon.SetActive(false);
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
}
