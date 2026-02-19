using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerupCurrency
{
    Coin,
    Gem
}

[CreateAssetMenu(fileName = "NewPowerup", menuName = "NewPowerup")]
public class PowerUp : ScriptableObject
{

    public string powerupName;
    public PowerupCurrency powerupCurrency;
    public int powerupPrice;
    public bool isPersistent;


    [TextArea(20, 20)]
    public string powerupDescription;



}
