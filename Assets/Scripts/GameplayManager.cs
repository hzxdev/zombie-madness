using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using Newtonsoft.Json.Linq;
using Unity.Burst.CompilerServices;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum GameMode
{
    Standard,
    Survival,
    Resurrection
    
}
public static class Extensions
{
    public static int findIndex<T>(this T[] array, T item)
    {
        return Array.IndexOf(array, item);
    }
}
public class GameplayManager : MonoBehaviour, IDataPersistence
{
    public bool pc;
    public LevelProperties[] levels;
    public Material[] skyboxes;
    public int currentLevelId = 0;
    public  GameObject botPrefab, woodenCratePrefab;
    public GameObject inputBlockBarrierPanel, DAYCLEARpanel, dayFinishPanel, afterDeathPanel, youDied, pauseMenu, reviveByGemButton, reviveByAdButton, reviveBanner, reviveNoThanks, dayProgessBar, survivalFirstPrompt;

     public Transform playerSpawnpoint;
    public float spawnPeriod;
    public float comboExpireTime;
    public float comboLeftTime;
    int currentCombo = 0;
    bool comboOn = false;
    [HideInInspector]
    public bool levelCompleted, afterDeath, isGamePaused, inputButtonsFade;
    public float buttonsFadeSpeedAfterLevelComplete;
    public int totalKillCount;
    public static GameplayManager instance;
    Animator killEffectAnim;
    public Animator blackOut;
    public Text dayBeginText, coinsCollected, newUnlockedThingDayFinishedText;
    public Animator dayBeginTextAnim;
    public Button nextDayButton, tryAgainButton, dayClearBackToMenu;
    public Spawnpoint[] spawnpoints;
    public Transform trapSpsParent;
    Transform[] trapSpawnpoints;
    public GameObject beartrapPrefab;
    CrateSpawnpoint[] crateSpawnpoints;
    float timeSinceLastSpawn;
    int cratesSpawnedSinceDayStart;

    public int aliveEntities, coinsCollectedInDay;

    public string RootPoolName = "Pooled Objects";

    public List<ObjectPool> Pools;
    [HideInInspector]
    public bool tutorialOn;
    public Dictionary<int, GameObjectContainer> Map { get; } = new Dictionary<int, GameObjectContainer>();

    public Image[] uiInputButtonsImgs;
    float[] uiInputButtonsAlphaValues;
    public GameObject pauseMenuResume, pauseMenuSettings, pauseMenuBackToMenu, pauseMenuBack, PAUSEDtext, loadingScreenInsant;
    public Slider sfxSlider, musicSlider, sensitivitySlider;
    public AudioMixer masterMix;
    public float forHowMuchTimeAZombieWasntDead;
    public float timeAfterLastKillToTriggerAZombie;

    [Header("AUDIO")]
    public float bgMusicFqNormal;
    public float bgMusicFqLevelEnd;
    public AudioLowPassFilter bgMusicFilter;

    //for converting for level sos convenience
    public ZombieType[] zombieTypes;
    public GameObject[] zombiePrefabs;
    public GameMode gameMode;
    public int survivalConstZombieAtOnce = 10, survivalScore;
    public TextMeshProUGUI survivalTimerText, survivalTimeOutText, survivalScoreText;
    float survivalTimer, survivalTimerStartoffTime = 20;
    public GameObject addTimeEffect;
    PlayerHealth playerHealth;

    public Volume volume;
    public GameObject standardModeCompletedPanel;

    [System.Serializable]
    public class ZombieSpawner
    {
        public ZombieType zombieType;
      //  public GameObject prefab;
        public float chance;


       // public int maxCountInTheLevel; // Max amount of that zombie that can be encountered in the level at once
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;

#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif

    }

    void Start()
    {
        if(PlayerPrefs.GetInt("postprocess") == 1)
        {
            Camera.main.GetUniversalAdditionalCameraData().SetRenderer(0);
        } else
        {
            Destroy(volume.gameObject);
            Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            Camera.main.GetUniversalAdditionalCameraData().SetRenderer(1); // Disable
            // Enable
        }

        if(pc)
        {
            for (int i = 0; i < uiInputButtonsImgs.Length; i++)
            {
                uiInputButtonsImgs[i].gameObject.SetActive(false);
            }
        }
    }

    public void LoadData(GameData data) //called on Start from DataPersistenceManager
    {
        gameMode = DataPersistenceManager.instance.currentGamemode;

        currentLevelId = data.lastDay - 1;
        if (currentLevelId >= levels.Length && gameMode == GameMode.Standard)
        //the day after the final day, means completed the standard mode
        {
            standardModeCompletedPanel.SetActive(true);
            blackOut.gameObject.SetActive(false);
            isGamePaused = true;
            Time.timeScale = 0;
            AudioListener.pause = false;
            GunController.instance.GetComponent<CharacterManager>().DeactivateCurrentCharGraphics();
            return;
        }

        if(currentLevelId >= levels.Length)
            currentLevelId = levels.Length -1;//just for spawning stuff convenience

        Reset();
        survivalTimer = survivalTimerStartoffTime;
        if (DataPersistenceManager.instance.gameData.lastDay == 1)
            tutorialOn = true;
        if (DataPersistenceManager.instance.gameData.lastDay == 2)
            newUnlockedThingDayFinishedText.gameObject.SetActive(false);

        forHowMuchTimeAZombieWasntDead = 0;
        musicSlider.value = PlayerPrefs.GetFloat("music");

        if (musicSlider.value == 0)
            masterMix.SetFloat("Music", Mathf.Log10(0.001f) * 20);
        else
            masterMix.SetFloat("Music", Mathf.Log10(musicSlider.value / 10) * 20);


        sfxSlider.value = PlayerPrefs.GetFloat("sfx");

        if (sfxSlider.value == 0)
            masterMix.SetFloat("SFX", Mathf.Log10(0.001f) * 20);
        else
            masterMix.SetFloat("SFX", Mathf.Log10(sfxSlider.value / 10) * 20);

        killEffectAnim = GameObject.Find("KillEffect").GetComponent<Animator>();
        spawnpoints = FindObjectsOfType<Spawnpoint>();
        crateSpawnpoints = FindObjectsOfType<CrateSpawnpoint>();
        comboLeftTime = comboExpireTime;
        if(currentLevelId < levels.Length)
        RenderSettings.skybox = skyboxes[(int)levels[currentLevelId].weather];
        playerHealth = PlayerHealth.instance;
        timeAfterLastKillToTriggerAZombie = levels[currentLevelId].timeAfterLastKillToTriggerAZombie;


        Invoke(nameof(SpawnCrates), UnityEngine.Random.Range(5, 15));

        //Setting initial alpha values, to set them back when level loads again
        uiInputButtonsAlphaValues = new float[uiInputButtonsImgs.Length];
        for (int i = 0; i < uiInputButtonsImgs.Length; i++)
        {
            uiInputButtonsAlphaValues[i] = uiInputButtonsImgs[i].color.a;
        }
        trapSpawnpoints = new Transform[trapSpsParent.childCount];
        for (int i = 0; i < trapSpsParent.childCount; i++)
        {
            trapSpawnpoints[i] = trapSpsParent.GetChild(i);
        }

if(!tutorialOn)
        {
            for (int i = 0; i < 7; i++) //spawn traps
            {
                Instantiate(beartrapPrefab, trapSpawnpoints[UnityEngine.Random.Range(0, trapSpawnpoints.Length)].position, beartrapPrefab.transform.rotation);

            }
        }


        switch (gameMode)             
        {
            case GameMode.Standard:
                dayBeginText.text = "DAY " + (currentLevelId + 1).ToString();
                survivalTimerText.gameObject.SetActive(false);
                survivalTimeOutText.gameObject.SetActive(false);
                survivalScoreText.gameObject.SetActive(false);
                break;
            case GameMode.Survival:
                dayBeginText.text = "SURVIVAL";
                survivalTimerText.gameObject.SetActive(true);
                survivalScoreText.gameObject.SetActive(true);
                SpawnZombies();
                survivalTimer = survivalTimerStartoffTime;
                break;
            case GameMode.Resurrection:
                dayBeginText.text = "RESURRECTION";
                survivalTimerText.gameObject.SetActive(false);
                survivalTimeOutText.gameObject.SetActive(false);
                survivalScoreText.gameObject.SetActive(false);
               // SpawnZombies();
                break;

        }
        
        newUnlockedThingDayFinishedText.gameObject.SetActive(false);

        if(data.survivalHiScore == 0 && gameMode == GameMode.Survival)
        {
            survivalFirstPrompt.SetActive(true);
            isGamePaused = true;
            blackOut.gameObject.SetActive(false);
            Time.timeScale = 0;
        }
    }

    public void SurvivalFirstPromptOk()
    {
        survivalFirstPrompt.SetActive(false);
        isGamePaused = false;
        Time.timeScale = 1;
    }


    public void SaveData(GameData data)
    {
       // data.lastDay = currentLevelId + 1;
        data.coin += coinsCollectedInDay;
        coinsCollectedInDay = 0;
    }


    void Update()
    {
        forHowMuchTimeAZombieWasntDead += Time.deltaTime;
        if (comboOn)
        {
            comboLeftTime -= Time.deltaTime;
            if(comboLeftTime <= 0)
            {
                comboOn = false;
                currentCombo = 0;
                comboLeftTime = comboExpireTime;
            }
        }

        if (aliveEntities == 0)
            timeSinceLastSpawn += Time.deltaTime;
        if(timeSinceLastSpawn >= spawnPeriod)
        {
            if(aliveEntities == 0)
            {
                if(gameMode == GameMode.Standard)
                SpawnZombies();
                timeSinceLastSpawn = 0;
            }
            
        }

        //buttons fade out
        if (inputButtonsFade && !pc)
        {
            for (int i = 0; i < uiInputButtonsImgs.Length; i++)
            {
                uiInputButtonsImgs[i].color = new Color(uiInputButtonsImgs[i].color.r, uiInputButtonsImgs[i].color.g, uiInputButtonsImgs[i].color.b, Mathf.Lerp(uiInputButtonsImgs[i].color.a , 0, Time.deltaTime* buttonsFadeSpeedAfterLevelComplete));
                inputBlockBarrierPanel.SetActive(true);
                
            }

            bgMusicFilter.cutoffFrequency = Mathf.Lerp(bgMusicFilter.cutoffFrequency, bgMusicFqLevelEnd, Time.deltaTime / 2);
        }

        if(levelCompleted == false && !afterDeath)
        {
            bgMusicFilter.cutoffFrequency = Mathf.Lerp(bgMusicFilter.cutoffFrequency, bgMusicFqNormal, Time.deltaTime / 2);
        }

        if(Input.GetKeyDown(KeyCode.Escape)) // back button on mobile
        {
            isGamePaused = true;
            pauseMenu.SetActive(true);
            PauseMenuBackPressed();
            Time.timeScale = 0;
            AudioListener.pause = true;

        }
        if(gameMode == GameMode.Survival)
        {
            if (playerHealth.alive)
                survivalTimer -= Time.deltaTime;


            if (survivalTimer <= 0)
            {
                playerHealth.KillPlayer();
                survivalTimeOutText.gameObject.SetActive(true);
            }

            TimeSpan time = TimeSpan.FromSeconds(survivalTimer);

            //here backslash is must to tell that colon is
            //not the part of format, it just a character that we want in output
            string str;
            if (survivalTimer >= 5)
            {
                str = time.ToString(@"mm\:ss");
                survivalTimerText.color = Color.white;
                survivalTimerText.fontSize = 67;
            } else
            {
                str = time.ToString(@"ss\:ff");
                survivalTimerText.color = Color.red;
                survivalTimerText.fontSize = 80;
            }
            
            survivalTimerText.text = str;
        }

       
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        AudioListener.pause = false;
    }
   

    void ShowDayFinishPanels()
    {
        for (int i = 0; i < uiInputButtonsImgs.Length; i++)
        {
            uiInputButtonsImgs[i].gameObject.SetActive(false);
        }
        dayFinishPanel.SetActive(true);
        dayClearBackToMenu.interactable = true;
        DAYCLEARpanel.SetActive(true);

        foreach (var s in FindObjectsOfType<AudioSource>())
        {
            if(!s.gameObject.CompareTag("BackgroundMusic")) // mute all audio except background music
            s.mute = true;
        }


        if (currentLevelId == 0) // day 1 finished, shop uncloked
        {
            newUnlockedThingDayFinishedText.gameObject.SetActive(true);
            Debug.Log("day 1 finished, shop uncloked");
        } else
        {
            newUnlockedThingDayFinishedText.gameObject.SetActive(false);
        }
    }

    public void PlayerDeath()
    {
        Invoke(nameof(ShowReviveOptions), 2.5f);
        AssignSurvivalHighscore();
        afterDeath = true;
        if(!pc)
        inputButtonsFade = true;
        survivalTimerText.gameObject.SetActive(false);


    }

    public void ShowReviveOptions()
    {
       // youDied.SetActive(true);
        reviveByGemButton.SetActive(true);
        reviveByAdButton.SetActive(true);
        reviveBanner.SetActive(true);
        reviveNoThanks.SetActive(true);
    }

    public void ReviveOfferRejected()
    {
        ShowAfterDeathPanels();
        nextDayButton.interactable = true;
        reviveByGemButton.SetActive(false);
        reviveByAdButton.SetActive(false);
        reviveBanner.SetActive(false);
        reviveNoThanks.SetActive(false);
    }

    public void ShowAfterDeathPanels()
    {
        for (int i = 0; i < uiInputButtonsImgs.Length; i++)
        {
            uiInputButtonsImgs[i].gameObject.SetActive(false);
        }
        youDied.SetActive(true);
        afterDeathPanel.SetActive(true);
        foreach (var s in FindObjectsOfType<AudioSource>())
        {
            if (!s.gameObject.CompareTag("BackgroundMusic")) // mute all audio except background music
                s.mute = true;
        }

    }

    public void AddKill()
    {
        if (levelCompleted)
            return;
        comboOn = true;
        currentCombo++;
        totalKillCount++;
        comboLeftTime = comboExpireTime;

        if(gameMode != GameMode.Survival)
        {
            killEffectAnim.SetTrigger("kill");
            killEffectAnim.SetBool("killPlaying", true);
        }
       
        int killSoundNum = currentCombo; if (currentCombo > 5) killSoundNum = 5;
        AudioManager.instance.PlayOneShot("kill" + killSoundNum.ToString());

        if(gameMode == GameMode.Standard)
        {
            if (totalKillCount == levels[currentLevelId].zombiesToKillToComplete)
            {
                Debug.Log("Level completed");
                LevelCompleted();
                //  dayProgessBar.SetActive(true);
                //    dayProgessBar.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(900, 130);
            }

            if (totalKillCount == (int)(levels[currentLevelId].zombiesToKillToComplete / 4))
            {
                dayProgessBar.SetActive(false);
                dayProgessBar.SetActive(true);
                dayProgessBar.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(225, 130);
            }

            if (totalKillCount == (int)(levels[currentLevelId].zombiesToKillToComplete / 2))
            {
                dayProgessBar.SetActive(false);
                dayProgessBar.SetActive(true);
                dayProgessBar.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(450, 130);
            }

            if (totalKillCount == (int)(3 * (levels[currentLevelId].zombiesToKillToComplete / 4)))
            {
                dayProgessBar.SetActive(false);
                dayProgessBar.SetActive(true);
                dayProgessBar.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(675, 130);
            }

            if(!levelCompleted)
            {
                while (aliveEntities < levels[currentLevelId].maxZombiesAtOnce)
                {
                    SpawnRandomZombieRespectiveToCurrentDay();
                }
            }
           
        }

        if (gameMode == GameMode.Survival)
        {
            survivalTimerText.gameObject.SetActive(false);
            survivalTimerText.gameObject.SetActive(true);
            survivalScoreText.gameObject.SetActive(false);
            survivalScoreText.gameObject.SetActive(true);
            survivalTimer += 5;
           
            addTimeEffect.gameObject.SetActive(false);
            addTimeEffect.gameObject.SetActive(true);
            while (aliveEntities < survivalConstZombieAtOnce)
            {
                SpawnOneRandomZombie();
            }

         

        }


    }

    void LevelCompleted()
    {
        Invoke(nameof(ShowDayFinishPanels), 2.5f);
        levelCompleted = true;
        if(!pc)
        inputButtonsFade = true;
        nextDayButton.interactable = true;
        foreach (var item in FindObjectsOfType<ZombieTemp>())
        {
            item.Die(BodyPart.Part.Head, true);
        }
        
    }

    void SpawnZombies()
    {

        if (tutorialOn)
            return;
        /*
        if(gameMode == GameMode.Standard) //old code
        {
            int j = 0;
            for (int i = 0; i < spawnpoints.Length; i++)
            {
                if (!spawnpoints[i].IsVisible())
                {
                    j++;
                    if (j > levels[currentLevelId].maxZombiesAtOnce)
                        return;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(spawnpoints[i].transform.position, out hit, 100f, NavMesh.AllAreas))
                    {



                        Spawn(ZombieTypeToPrefab(levels[currentLevelId].spawners[GetRandomSpawn()].zombieType), hit.position, ZombieTypeToPrefab(levels[currentLevelId].spawners[GetRandomSpawn()].zombieType).transform.rotation);

                    }
                    else
                    {
                        Debug.LogError("Spawnpoints çok uzakta? I guess?");
                    }


                }
            }
        }
        */

        if (gameMode == GameMode.Standard)
        {
            int j = 0;
            for (int i = 0; i < spawnpoints.Length; i++)
            {

                j++;
                if (j > levels[currentLevelId].maxZombiesAtOnce)
                    return;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(spawnpoints[i].transform.position, out hit, 100f, NavMesh.AllAreas))
                {


                    Spawn(ZombieTypeToPrefab(levels[currentLevelId].spawners[GetRandomSpawn()].zombieType), hit.position, ZombieTypeToPrefab(levels[currentLevelId].spawners[GetRandomSpawn()].zombieType).transform.rotation);

                }
                else
                {
                    Debug.LogError("Spawnpoints çok uzakta? I guess?");
                }



            }
        }

        if (gameMode == GameMode.Survival)
        {
            int j = 0;
            for (int i = 0; i < spawnpoints.Length; i++)
            {
               
                    j++;
                    if (j > survivalConstZombieAtOnce)
                        return;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(spawnpoints[i].transform.position, out hit, 100f, NavMesh.AllAreas))
                    {


                        int r = UnityEngine.Random.Range(0, zombieTypes.Length);
                        Spawn(ZombieTypeToPrefab(zombieTypes[r]), hit.position, ZombieTypeToPrefab(zombieTypes[r]).transform.rotation);

                    }
                    else
                    {
                        Debug.LogError("Spawnpoints çok uzakta? I guess?");
                    }


                
            }
        }
             



    }


    void SpawnRandomZombieRespectiveToCurrentDay()
    {


        int randomsp = UnityEngine.Random.Range(0, spawnpoints.Length);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnpoints[randomsp].transform.position, out hit, 100f, NavMesh.AllAreas))
        {

            Spawn(ZombieTypeToPrefab(levels[currentLevelId].spawners[GetRandomSpawn()].zombieType), hit.position, ZombieTypeToPrefab(levels[currentLevelId].spawners[GetRandomSpawn()].zombieType).transform.rotation);


        }
        else
        {
            Debug.LogError("Spawnpoints çok uzakta? I guess?");
        }




    }
    void SpawnOneRandomZombie()
    {


        int randomsp = UnityEngine.Random.Range(0, spawnpoints.Length);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnpoints[randomsp].transform.position, out hit, 100f, NavMesh.AllAreas))
            {

                //Spawned one random zombie
                int r = UnityEngine.Random.Range(0, zombieTypes.Length);
                Spawn(ZombieTypeToPrefab(zombieTypes[r]), hit.position, ZombieTypeToPrefab(zombieTypes[r]).transform.rotation);
               

            }
            else
            {
                Debug.LogError("Spawnpoints çok uzakta? I guess?");
            }



        
    }

    public GameObject ZombieTypeToPrefab(ZombieType type)
    {
        int index = zombieTypes.findIndex(type);
        return zombiePrefabs[index];
    }

    void SpawnCrates()
    {
        if (tutorialOn)
            return;

        if (cratesSpawnedSinceDayStart >= levels[currentLevelId].crates)
            return;

        for (int i = 0; i < crateSpawnpoints.Length; i++)
        {
            if (!crateSpawnpoints[i].IsVisible())
            {
                cratesSpawnedSinceDayStart++;
                if (cratesSpawnedSinceDayStart > levels[currentLevelId].crates)
                    break;                 
                Instantiate(woodenCratePrefab, crateSpawnpoints[i].transform.position + new Vector3(0, 3, 0), Quaternion.identity);


            }
        }

        if (cratesSpawnedSinceDayStart < levels[currentLevelId].crates)
            Invoke(nameof(SpawnCrates), UnityEngine.Random.Range(5, 10));

    }

    int GetRandomSpawn()
    {
        float random = UnityEngine.Random.Range(0f, 1f);
        float numForAdding = 0;
        float total = 0;
        for (int i = 0; i < levels[currentLevelId].spawners.Length; i++)
        {
            total += levels[currentLevelId].spawners[i].chance;
        }
        for (int i = 0; i < levels[currentLevelId].spawners.Length; i++)
        {
            if(levels[currentLevelId].spawners[i].chance / total + numForAdding >= random)
            {
                return i;
            }
            else
            {
                numForAdding += levels[currentLevelId].spawners[i].chance / total;
            }
        }
        return 0;
    }


    private void SceneManagerOnSceneLoaded(Scene arg0, LoadSceneMode loadSceneMode)
    {
        //    Debug.Log("Resetting Object Pools");
        if(SceneManager.GetActiveScene().buildIndex == 1 && blackOut != null)
        blackOut.Play("BlackoutBlackToTrans");
        //Reset();
        //Data persistance dolayisiyla load gamee cekildi ^
    }

    private void CreatePools()
    {
        foreach (var item in Pools)
        {
            for (int i = 0; i < item.AmountToPool; i++)
            {
                CreatePooledObject(item);
            }
        }
    }

    public void Reset()
    {
        foreach (var pool in Pools)
        {
            if (pool == null)
                continue;

            foreach (var item in pool.Items)
            {
                if (item == null)
                    continue;

                if (item.Object != null)
                {
                    Destroy(item.Object);
                }

                item.Object = null;
                item.Pool = null;
                item.PoolingEnabledComponents.Clear();
            }
            pool.Items.Clear();
            pool.ParentPoolObject = null;
        }
        Map.Clear();
        CreatePools();
    }

    /// <summary>
    /// Find/Create the parent which pooled objects should be attached to.
    /// </summary>
    private GameObject GetParentPoolObject(string objectPoolName)
    {
        if (string.IsNullOrEmpty(objectPoolName))
            objectPoolName = RootPoolName;

        var parentObject = GameObject.Find(objectPoolName);
        if (parentObject != null)
            return parentObject;

        parentObject = new GameObject
        {
            name = objectPoolName
        };

        if (objectPoolName == RootPoolName)
            return parentObject;

        var root = GameObject.Find(RootPoolName) ?? GetParentPoolObject(RootPoolName);

        parentObject.transform.parent = root.transform;
        return parentObject;
    }

    /// <summary>
    /// Create a new item for a given pool
    /// </summary>
    private GameObjectContainer CreatePooledObject(ObjectPool pool)
    {
        if (pool.ObjectToPool == null)
        {
            throw new Exception($"Object pool entry '{pool.PoolName}' needs a prefab attached");
        }

        if (!pool.Initialized)
            pool.Initialize();

        var obj = Instantiate(pool.ObjectToPool);
        obj.name = obj.name;

        if (pool.ParentPoolObject == null)
            pool.ParentPoolObject = GetParentPoolObject(pool.PoolName);

        obj.transform.parent = pool.ParentPoolObject.transform;
        obj.SetActive(false);

        var container = new GameObjectContainer
        {
            Object = obj,
            ObjectId = obj.GetInstanceID(),
            Pool = pool,
            PoolingEnabledComponents = obj.GetComponents<IPoolable>().ToList(),
        };

        Map.Add(obj.GetInstanceID(), container);
        pool.Items.AddFirst(container);
        pool.Created++;
        return container;
    }

    /// <summary>
    /// Create an instance of an GameObject prefab. 
    /// A replacment for 'Instantiate(...)'.
    /// </summary>
    /// <param name="prefab">A game object prefab to create an instance of</param>
    /// <param name="position">The position of the spawned GameObject</param>
    /// <param name="rotation">The rotation of the spawned GameObject</param>
    /// <returns>pooled GameObject</returns>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var id = prefab.GetInstanceID();
        var pool = GetPoolForPrefab(id);
        if (pool == null)
        {
            pool = new ObjectPool(prefab, prefab.name, 25);
            Debug.Log($"Dynamically creating pool for prefab {prefab.name}");
            Pools.Add(pool);
            //throw new Exception($"Unable to find object pool for type");
        }

        var container = FindFreePoolItem(pool);
        if (container == null)
        {
            if (!pool.ShouldExpand)
                return null;

            container = CreatePooledObject(pool);
            pool.Expands++;
        }
        else
        {
            pool.Recycles++;
        }

        container.Spawns++;
        RecycleItem(container, position, rotation);
        return container.Object;
    }

    /// <summary>
    /// Return GameObject to the pool. 
    /// A replacement for 'Destroy(...)'. 
    /// </summary>
    /// <param name="o"></param>
    public void Despawn(GameObject o)
    {
        if (o == null)
            return;

        var container = GetContainer(o.GetInstanceID());
        if (container != null)
        {
            container.Despawns++;
            foreach (var c in container.PoolingEnabledComponents)
            {
                c.Despawn();
            }
        }

        o.SetActive(false);
    }

    /// <summary>
    /// Reset transform and call IPoolable.Spawn on components.
    /// </summary>
    private static void RecycleItem(GameObjectContainer container, Vector3 position, Quaternion rotation)
    {
        var t = container.Object.transform;
        t.rotation = rotation;
        t.position = position;
        container.Object.SetActive(true);
        container.Cycles++;

        foreach (var c in container.PoolingEnabledComponents)
        {
            c.Spawn();
        }
    }

    /// <summary>
    /// Get an item from a given pool that is not being used.
    /// </summary>
    private GameObjectContainer FindFreePoolItem(ObjectPool pool)
    {
        for (int i = 0; i < pool.Items.Count; i++)
        {
            var node = pool.Items.First;
            pool.Items.RemoveFirst();
            pool.Items.AddLast(node);

            // Clean out objects that no longer exist (because of scene unload etc)
            var obj = node.Value.Object;
            if (obj == null)
            {
                DestroyContainer(node.Value);
                continue;
            }

            if (!obj.activeInHierarchy)
            {
                node.Value.TimesSelected++;
                return node.Value;
            }

            node.Value.TimesSkipped++;
        }
        return null;
    }

    private void DestroyContainer(GameObjectContainer container)
    {
        container.Pool.Items.Remove(container);
        Map.Remove(container.ObjectId);
    }

    private ObjectPool GetPoolForPrefab(int prefabInstanceId)
    {
        for (int i = 0; i < Pools.Count; i++)
        {
            var pool = Pools[i];
            if (pool.Id == prefabInstanceId)
                return pool;
        }
        return null;
    }

    private GameObjectContainer GetContainer(int gameObjectInstanceId)
    {
        GameObjectContainer container;
        Map.TryGetValue(gameObjectInstanceId, out container);
        return container;
    }

    [Serializable]
    public class ObjectPool
    {
        public GameObject ObjectToPool;
        public string PoolName;
        public int AmountToPool;
        public bool ShouldExpand = true;

        public ObjectPool() { }

        public ObjectPool(GameObject prefab, string name, int amount, bool expandable = true)
        {
            ObjectToPool = prefab;
            PoolName = name;
            AmountToPool = amount;
            ShouldExpand = expandable;
        }

        public void Initialize()
        {
            if (ObjectToPool != null)
            {
                Id = ObjectToPool.GetInstanceID();
                Initialized = true;
            }
        }

        public bool Initialized { get; private set; }
        public int Id { get; private set; }
        public GameObject ParentPoolObject { get; set; }
        public LinkedList<GameObjectContainer> Items { get; } = new LinkedList<GameObjectContainer>();
        public int Recycles { get; set; }
        public int Created { get; set; }
        public int Expands { get; set; }
    }

    public interface IPoolable
    {
        void Spawn();
        void Despawn();
    }

    public class GameObjectContainer
    {
        public ObjectPool Pool;
        public GameObject Object;
        public List<IPoolable> PoolingEnabledComponents;
        public int Cycles;
        public int TimesSkipped;
        public int TimesSelected;
        public int Despawns;
        public int Spawns;
        public int ObjectId;
    }

    public void TryAgainButtonPressed()
    {       
        nextDayButton.interactable = false;
       // dayBeginText.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(2000, dayBeginText.GetComponent<RectTransform>().anchoredPosition.y);
        blackOut.SetTrigger("levelLoaded");
        Invoke(nameof(ReloadCurrentDay), 1);
    }

    public void ReloadCurrentDay()
    {
        forHowMuchTimeAZombieWasntDead = 0;
        aliveEntities = 0;
        totalKillCount = 0;
        cratesSpawnedSinceDayStart = 0;
        survivalScore = 0;
        UpdateSurvivalScore();
        afterDeath = false;
        afterDeathPanel.SetActive(false);
        youDied.SetActive(false);
        inputBlockBarrierPanel.SetActive(false);
        if(!pc)
        inputButtonsFade = false;
        levelCompleted = false;
        GunController.instance.ChangeToSlotAndUpdateWeapon(GunController.instance.slots[GunController.instance.currentSlotId]);
        GunController.instance.transform.position = playerSpawnpoint.position; //Player return to starting point
        GunController.instance.transform.rotation = playerSpawnpoint.rotation;
        PlayerHealth.instance.Respawn();
        timeAfterLastKillToTriggerAZombie = levels[currentLevelId].timeAfterLastKillToTriggerAZombie;


        for (int i = 0; i < FindObjectsOfType<WoodenCrate>().Length; i++)
        {
            Destroy(FindObjectsOfType<WoodenCrate>()[i].gameObject);
        }

        for (int i = 0; i < FindObjectsOfType<WeaponPickup>().Length; i++)
        {
            Destroy(FindObjectsOfType<WeaponPickup>()[i].gameObject);
        }

        for (int i = 0; i < FindObjectsOfType<PickupObject>().Length; i++)
        {
            Destroy(FindObjectsOfType<PickupObject>()[i].gameObject);
        }

        for (int i = 0; i < FindObjectsOfType<DroppedWeapon>().Length; i++)
        {
            Destroy(FindObjectsOfType<DroppedWeapon>()[i].gameObject);
        }

        for (int i = 0; i < FindObjectsOfType<Grenade>().Length; i++)
        {
            Destroy(FindObjectsOfType<Grenade>()[i].gameObject);
        }

        for (int i = 0; i < FindObjectsOfType<Beartrap>().Length; i++)
        {
            if (!FindObjectsOfType<Beartrap>()[i].isPersistent)
                Destroy(FindObjectsOfType<Beartrap>()[i].gameObject);
        }

        Invoke(nameof(SpawnCrates), UnityEngine.Random.Range(5, 15));

        for (int i = 0; i < trapSpsParent.childCount; i++)
        {
            trapSpawnpoints[i] = trapSpsParent.GetChild(i);
        }


        if (!tutorialOn)
        {
            for (int i = 0; i < levels[currentLevelId].beartraps; i++) //spawn traps
            {
                int r = UnityEngine.Random.Range(0, trapSpawnpoints.Length);
                Instantiate(beartrapPrefab, trapSpawnpoints[r].position, beartrapPrefab.transform.rotation);

            }
        }
            

        for (int i = 0; i < uiInputButtonsImgs.Length; i++)
        {
            uiInputButtonsImgs[i].color = new Color(uiInputButtonsImgs[i].color.r, uiInputButtonsImgs[i].color.g, uiInputButtonsImgs[i].color.b, uiInputButtonsAlphaValues[i]);
            uiInputButtonsImgs[i].gameObject.SetActive(true);
        }
        GunController.instance.UpdateThrowNearButtonStatus();
        foreach (var s in FindObjectsOfType<AudioSource>())
        {
            s.mute = false;
        }
        Reset();

        dayBeginText.text = "DAY " + (currentLevelId + 1).ToString();
        if (!tutorialOn)
        {
            if(dayBeginTextAnim.gameObject.activeSelf)
            dayBeginTextAnim.SetTrigger("play");
        }
           

        DataPersistenceManager.instance.gameData.lastDay = currentLevelId + 1;
        DataPersistenceManager.instance.SaveGame();
        survivalTimeOutText.gameObject.SetActive(false);

        switch (gameMode)
        {
            case GameMode.Standard:
                dayBeginText.text = "DAY " + (currentLevelId + 1).ToString();
                SpawnZombies();
                survivalTimerText.gameObject.SetActive(false);
                survivalScoreText.gameObject.SetActive(false);
                break;
            case GameMode.Survival:
                dayBeginText.text = "SURVIVAL";
                SpawnZombies();
                survivalTimer = survivalTimerStartoffTime;
                survivalTimerText.gameObject.SetActive(true);
                survivalScoreText.gameObject.SetActive(true);
                break;
            case GameMode.Resurrection:
                dayBeginText.text = "RESURRECTION";
              //  SpawnZombies();
                survivalTimerText.gameObject.SetActive(false);
                survivalScoreText.gameObject.SetActive(false);
                break;

        }
    }

    public void OnApplicationQuit()
    {
       // DataPersistenceManager.instance.SaveGame();
    }

    public void NextDayButtonPressed()
    {
        currentLevelId += 1;
        nextDayButton.interactable = false;
        dayBeginText.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(2000, dayBeginText.GetComponent<RectTransform>().anchoredPosition.y);
        blackOut.SetTrigger("levelLoaded");
        Invoke(nameof(LoadNextDay), 1);
       
    }

    public void Revived()
    {
       
        youDied.SetActive(false);
        reviveBanner.SetActive(false);
        reviveByAdButton.SetActive(false);
        reviveByGemButton.SetActive(false);
        reviveNoThanks.SetActive(false);
        afterDeath = false;
        afterDeathPanel.SetActive(false);
        inputBlockBarrierPanel.SetActive(false);
        if(!pc)
        inputButtonsFade = false;
        levelCompleted = false;
        if(survivalTimer < survivalTimerStartoffTime)
        survivalTimer = survivalTimerStartoffTime;
        if(gameMode == GameMode.Survival)
        {
            survivalTimerText.gameObject.SetActive(true);
            survivalScoreText.gameObject.SetActive(true);
            survivalTimeOutText.gameObject.SetActive(false);
            AssignSurvivalHighscore();
        }
        

        for (int i = 0; i < uiInputButtonsImgs.Length; i++)
        {
            uiInputButtonsImgs[i].color = new Color(uiInputButtonsImgs[i].color.r, uiInputButtonsImgs[i].color.g, uiInputButtonsImgs[i].color.b, uiInputButtonsAlphaValues[i]);
            uiInputButtonsImgs[i].gameObject.SetActive(true);
        }
        foreach (var s in FindObjectsOfType<AudioSource>())
        {
            s.mute = false;
        }
        GunController.instance.UpdateThrowNearButtonStatus();
    }

    public void LoadNextDay()
    {
        if(currentLevelId >= levels.Length && gameMode == GameMode.Standard)
            //the day after the final day, means completed the standard mode
        {
            standardModeCompletedPanel.SetActive(true);
            blackOut.gameObject.SetActive(false);
            isGamePaused = true;
            Time.timeScale = 0;
            AudioListener.pause = false;
            GunController.instance.GetComponent<CharacterManager>().DeactivateCurrentCharGraphics();
            forHowMuchTimeAZombieWasntDead = 0;
            aliveEntities = 0;
            totalKillCount = 0;
            cratesSpawnedSinceDayStart = 0;
            DAYCLEARpanel.SetActive(false);
            dayFinishPanel.SetActive(false);
            inputBlockBarrierPanel.SetActive(false);
            return;
        }

        forHowMuchTimeAZombieWasntDead = 0;
        timeAfterLastKillToTriggerAZombie = levels[currentLevelId].timeAfterLastKillToTriggerAZombie;
        aliveEntities = 0;
        totalKillCount = 0;
        cratesSpawnedSinceDayStart = 0;
        DAYCLEARpanel.SetActive(false);
        dayFinishPanel.SetActive(false);
        inputBlockBarrierPanel.SetActive(false);
        if(!pc)
        inputButtonsFade = false;
        levelCompleted = false;
        GunController.instance.transform.position = playerSpawnpoint.position; //Player return to starting point
        GunController.instance.transform.rotation = playerSpawnpoint.rotation;
        PlayerHealth.instance.Respawn();
        newUnlockedThingDayFinishedText.gameObject.SetActive(false);

        for (int i = 0; i < FindObjectsOfType<WoodenCrate>().Length; i++)
        {
            Destroy(FindObjectsOfType<WoodenCrate>()[i].gameObject);
        }

        for (int i = 0; i < FindObjectsOfType<WeaponPickup>().Length; i++)
        {
            Destroy(FindObjectsOfType<WeaponPickup>()[i].gameObject);
        }

        for (int i = 0; i < FindObjectsOfType<PickupObject>().Length; i++)
        {
            Destroy(FindObjectsOfType<PickupObject>()[i].gameObject);
        }

        for (int i = 0; i < FindObjectsOfType<DroppedWeapon>().Length; i++)
        {
            Destroy(FindObjectsOfType<DroppedWeapon>()[i].gameObject);
        }

        for (int i = 0; i < FindObjectsOfType<Grenade>().Length; i++)
        {
            Destroy(FindObjectsOfType<Grenade>()[i].gameObject);
        }

        for (int i = 0; i < FindObjectsOfType<Beartrap>().Length; i++)
        {
            if(!FindObjectsOfType<Beartrap>()[i].isPersistent)
            Destroy(FindObjectsOfType<Beartrap>()[i].gameObject);
        }

        for (int i = 0; i < trapSpsParent.childCount; i++)
        {
            trapSpawnpoints[i] = trapSpsParent.GetChild(i);
        }

        if(!tutorialOn)
        {
            for (int i = 0; i < levels[currentLevelId].beartraps; i++) //spawn traps
            {
                int r = UnityEngine.Random.Range(0, trapSpawnpoints.Length);
                Instantiate(beartrapPrefab, trapSpawnpoints[r].position, beartrapPrefab.transform.rotation);

            }
        }
       

        Invoke(nameof(SpawnCrates), UnityEngine.Random.Range(5, 15));

        for (int i = 0; i < uiInputButtonsImgs.Length; i++)
        {
            uiInputButtonsImgs[i].color = new Color(uiInputButtonsImgs[i].color.r, uiInputButtonsImgs[i].color.g, uiInputButtonsImgs[i].color.b, uiInputButtonsAlphaValues[i]);
            uiInputButtonsImgs[i].gameObject.SetActive(true);
        }
        GunController.instance.UpdateThrowNearButtonStatus();
        foreach (var s in FindObjectsOfType<AudioSource>())
        {        
                s.mute = false;
        }
        Reset();
        
        dayBeginText.text = "DAY " + (currentLevelId + 1).ToString();
        if (!tutorialOn)
        {
            if (dayBeginTextAnim.gameObject.activeSelf)
                dayBeginTextAnim.SetTrigger("play");
        }



        if (currentLevelId < levels.Length)
            RenderSettings.skybox = skyboxes[(int)levels[currentLevelId].weather];


        DataPersistenceManager.instance.gameData.lastDay = currentLevelId + 1;
        DataPersistenceManager.instance.SaveGame();
    }

    public void SkipLevelDEV()
    {
        LevelCompleted();
        isGamePaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        AudioListener.pause = false;
    }

    public void PauseMenuSettingsPressed()
    {
        PAUSEDtext.SetActive(false);
        pauseMenuResume.SetActive(false);
        pauseMenuSettings.SetActive(false);
        pauseMenuBackToMenu.SetActive(false);
        pauseMenuBack.SetActive(true);
        sfxSlider.gameObject.SetActive(true);
        musicSlider.gameObject.SetActive(true);
        sensitivitySlider.gameObject.SetActive(true);
    }

    public void PauseMenuBackPressed()
    {     
        PAUSEDtext.SetActive(true);
        pauseMenuResume.SetActive(true);
        pauseMenuSettings.SetActive(true);
        pauseMenuBackToMenu.SetActive(true);
        pauseMenuBack.SetActive(false);
        sfxSlider.gameObject.SetActive(false);
        musicSlider.gameObject.SetActive(false);
        sensitivitySlider.gameObject.SetActive(false);
    }

    public void UpdateCoinsCollected()
    {
        coinsCollected.gameObject.SetActive(false);
        coinsCollected.gameObject.SetActive(true);
        coinsCollected.text = "+" + coinsCollectedInDay.ToString();

    }

    public void UpdateSurvivalScore()
    {
     
        survivalScoreText.text = "SCORE: " + survivalScore.ToString();
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

    public void PauseButton()
    {
        isGamePaused = true;
        pauseMenu.SetActive(true);
        PauseMenuBackPressed();
        Time.timeScale = 0;
        AudioListener.pause = true;
    }

    void OnApplicationFocus(bool pauseStatus)
    {
        if (pauseStatus)
        {
            //your app is NO LONGER in the background
        }
        else
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
                return;
            //your app is now in the background
            isGamePaused = true;
            pauseMenu.SetActive(true);
            PauseMenuBackPressed();
            Time.timeScale = 0;
            AudioListener.pause = true;
        }
    }

    public void StandardModeCompletedPanelBackToMenu()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;

        DataPersistenceManager.instance.gameData.completedStandardMode = true;
        if(DataPersistenceManager.instance.gameData.characters.ContainsKey("Ninja"))
        DataPersistenceManager.instance.gameData.characters["Ninja"] = true;

        DataPersistenceManager.instance.SaveGame();
        
        loadingScreenInsant.SetActive(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void BackToMenuSave()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        DataPersistenceManager.instance.SaveGame();
        loadingScreenInsant.SetActive(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void DayClearBackToMenu()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        DataPersistenceManager.instance.gameData.lastDay = DataPersistenceManager.instance.gameData.lastDay + 1;
        DataPersistenceManager.instance.SaveGame();
        dayClearBackToMenu.interactable = false;
        loadingScreenInsant.SetActive(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void BackToMenuDontSave()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        // DataPersistenceManager.instance.SaveGame();
        loadingScreenInsant.SetActive(true);

        AssignSurvivalHighscore();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void AssignSurvivalHighscore()
    {
        if(gameMode == GameMode.Survival)
        {
            if(DataPersistenceManager.instance.gameData.survivalHiScore < survivalScore)
            {
                DataPersistenceManager.instance.gameData.survivalHiScore = survivalScore;
                DataPersistenceManager.instance.SaveGame();
            }
        }
    }

    public void WatchAdToRevive()
    {
        Rewarded.instance.WhichLastRewardedAd = WhichRewardedAd.Revive;
        Rewarded.instance.ShowRewardedAd();
    }

    void LoadStartMenu()
    {
        
    }

    public float MapFunc(float in_min, float in_max, float out_min, float out_max, float x)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }


}
