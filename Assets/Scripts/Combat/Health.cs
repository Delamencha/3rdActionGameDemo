using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    private float health;

    
    void Start()
    {
        health = maxHealth;
    }

    public void DealDamage(float damageValue)
    {
        if (health <= 0) return;

        health = Mathf.Max(0, health - damageValue);

        Debug.Log(health);

    }


}
