using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{

    [SerializeField] private GameObject weaponDamage;

    public void EnableWeapon()
    {
        weaponDamage.SetActive(true);
    }

    public void DisableWeapon()
    {
        weaponDamage.SetActive(false);
    }

}
