using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{

    [SerializeField] private GameObject[] weaponDamage;

    public void EnableWeapon(int idx = 0)
    {
        if(idx <= weaponDamage.Length && weaponDamage[idx] != null)
        {
            weaponDamage[idx].SetActive(true);
        }
        else
        {
            Debug.LogError("inValid idx");
        }
        
    }

    public void DisableWeapon(int idx = 0)
    {
        if (idx <= weaponDamage.Length && weaponDamage[idx] != null)
        {
            weaponDamage[idx].SetActive(false);
        }
        else
        {
            Debug.LogError("inValid idx");
        }

    }

}
