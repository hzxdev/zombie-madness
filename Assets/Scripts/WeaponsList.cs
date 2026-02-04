using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WeaponsList : MonoBehaviour
{
    public Weapon[] weapons;
    public Pickup[] pickups;
    public Character[] characters;
    public static WeaponsList instance;
    void Awake()
    {
        if (instance == null)
            instance = this;
    }
    private void Start()
    {
        DataPersistenceManager.instance.ConstructWeaponDict();
    }
    public Weapon GetWeaponByName(string name)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].weaponName == name)
                return weapons[i];
        }
        Debug.LogWarning("Silah bulunamadi yarram");
        return null;
    }
    public Character GetCharacterByName(string name)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].characterName == name)
                return characters[i];
        }
        Debug.LogWarning("karakter yok amkkk");
        return null;
    }
    public Character GetCharacterByID(int id)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i].characterId == id)
                return characters[i];
        }
        Debug.LogWarning("karakter yok amkkk");
        return null;
    }
    public Weapon GetWeaponByID(int id)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].weaponId == id)
                return weapons[i];
        }
        Debug.LogWarning("Silah bulunamadi yarram");
        return null;
    }
}