using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static Cinemachine.DocumentationSortingAttribute;
using static GameplayManager;

public class CharacterManager : MonoBehaviour, IDataPersistence
{
    public CharInfo[] charInfos;
    public TwoBoneIKConstraint leftHandRig;
    public MultiAimConstraint rightHandRig, upperBodyRig;
    public RigBuilder rigBuilder;

    Character currentChar;
    [HideInInspector]
    public CharInfo currentCharInfo;

    [System.Serializable]
    public class CharInfo
    {
        public Character character;
        public Transform hips, geo, spine, rightHand, leftArm, leftForeArm, leftHand, weaponsParent, nonIKWeaponsParent;           
    }
    // Start is called before the first frame update
    void Awake()
    {
        
    }
         
    void Start()
    {

        currentChar = WeaponsList.instance.GetCharacterByName(DataPersistenceManager.instance.gameData.equippedCharacter);

        if(DataPersistenceManager.instance.currentGamemode == GameMode.Resurrection)
            currentChar = WeaponsList.instance.GetCharacterByName("Zombie");

        currentCharInfo = charInfos[currentChar.characterId];

        gameObject.name = currentChar.model.name;


        for (int i = 0; i < charInfos.Length; i++)
        {
            if (charInfos[i].geo.gameObject != null)
            {
                if (charInfos[i].geo != currentCharInfo.geo)
                {
                    Destroy(charInfos[i].geo.gameObject);
                }
            }


            if (charInfos[i].hips.gameObject != null)
            {
                if (charInfos[i].hips != currentCharInfo.hips)
                {
                    Destroy(charInfos[i].hips.gameObject);
                }
            }

        }

        currentCharInfo.geo.gameObject.SetActive(true);
        transform.localScale = currentCharInfo.geo.transform.localScale;
        currentCharInfo.hips.gameObject.SetActive(true);
        GetComponent<Animator>().avatar = currentChar.avatar;
        leftHandRig.data.root = currentCharInfo.leftArm;
        leftHandRig.data.mid = currentCharInfo.leftForeArm;
        leftHandRig.data.tip = currentCharInfo.leftHand;
        upperBodyRig.data.constrainedObject = currentCharInfo.spine;
        rightHandRig.data.constrainedObject = currentCharInfo.rightHand;
        rightHandRig.data.aimAxis = currentChar.rightHandAimAxis;
        rightHandRig.data.upAxis = currentChar.rightHandUpxis;
        rigBuilder.Build();
        
        GetComponent<Animator>().Rebind();
        GetComponent<Animator>().SetFloat("blendSpeedMultiplier", currentChar.speed / 4);
    }

    public void DeactivateCurrentCharGraphics()
    {
        currentChar = WeaponsList.instance.GetCharacterByName(DataPersistenceManager.instance.gameData.equippedCharacter);
        currentCharInfo = charInfos[currentChar.characterId];
        currentCharInfo.geo.gameObject.SetActive(false);
        currentCharInfo.hips.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            rightHandRig.data.aimAxis = currentChar.rightHandAimAxis;
            rightHandRig.data.upAxis = currentChar.rightHandUpxis;
            rigBuilder.Build();
        }
    }

    public void LoadData(GameData data)
    {
       
    }

    public void SaveData(GameData data)
    {
       
    }
}
