using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum Currency
{
    Coin,
    Gem
}

public enum Obtained
{
    Store,
    Special
}
[CreateAssetMenu(fileName = "New Character", menuName = "New Character")]
public class Character : ScriptableObject
{
    public string characterName;
    public int characterId;
    public GameObject model;
    public Sprite characterPreviewImage;
    public Avatar avatar;
    public Currency characterCurrency;
    public Obtained obtainedBy;
    public int characterPrice;
    public float damageMultiplier;
    public int maxHealth;
    public float speed;
    public MultiAimConstraintData.Axis rightHandAimAxis = MultiAimConstraintData.Axis.X; //default for SWAT is Aim Axis X, Up Axis Z
    public MultiAimConstraintData.Axis rightHandUpxis = MultiAimConstraintData.Axis.Z;
    public string[] hurtSounds;

    [TextArea(20, 20)]
    public string characterDescription;
}
