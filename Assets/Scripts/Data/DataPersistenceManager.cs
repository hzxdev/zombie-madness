using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;
    public GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    public Weapon[] weapons;
    public Character[] characters;
    public GameMode currentGamemode;
    public static DataPersistenceManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Found more than one Data Persistence Manager in the scene.");
            Destroy(this.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
    }


    public void NewGame()
    {
        this.gameData = new GameData();
        PlayerPrefs.SetFloat("music", 5);
        PlayerPrefs.SetFloat("sfx", 5);
        PlayerPrefs.SetFloat("sensitivity", 2);
        PlayerPrefs.SetFloat("resolution", 7);
        PlayerPrefs.SetInt("agreedtermsandconditions", 0);
    }
    public void LoadGame()
    {
        // load any saved data from a file using the data handler
        instance = this;
        this.gameData = dataHandler.Load();
        // if no data can be loaded, initialize to a new game
        if (this.gameData == null)
        {
            Debug.Log("No data was found. Initializing data to defaults.");
            PlayerPrefs.SetInt("postprocess", 1);
            NewGame();
        }
        // push the loaded data to all other scripts that need it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }
    public void SaveGame()
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        //?st? g?t?mden att?m gerekirse sil!
        // pass the data to other scripts so they can update it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(gameData);
        }
        // save that data to a file using the data handler
        dataHandler.Save(gameData);
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
        ConstructWeaponDictStartMenu();
        SaveGame(); //new
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
    public void ConstructWeaponDict() // This will happen in game scene because will be called from WeaponsList
    {




    }
    public void ConstructWeaponDictStartMenu()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (!DataPersistenceManager.instance.gameData.weaponsAmmo.ContainsKey(weapons[i].weaponName))
                DataPersistenceManager.instance.gameData.weaponsAmmo.Add(weapons[i].weaponName, 0);

            gameData.weaponsAmmo["CombatKnife"] = 1;
        }
        for (int i = 0; i < characters.Length; i++)
        {
            if (!DataPersistenceManager.instance.gameData.characters.ContainsKey(characters[i].characterName))
                DataPersistenceManager.instance.gameData.characters.Add(characters[i].characterName, false);
            gameData.characters["SWAT"] = true;
        }

        for (int i = 0; i < 3; i++)
        {
            if (!DataPersistenceManager.instance.gameData.turrets.ContainsKey(i))
                DataPersistenceManager.instance.gameData.turrets.Add(i, false);
            gameData.turrets[0] = true;
        }
        if (gameData.characters.ContainsKey("Zombie") && gameData.survivalHiScore >= 50000)
        {
           /* if(!gameData.characters["Zombie"])
            {
                Debug.Log("You won character Zombie!");
                gameData.characters["Zombie"] = true;
                SaveGame();
            }
           */
       
        }
    }
}