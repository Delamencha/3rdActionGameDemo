using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    private float health;

    private bool isInvunerable;

    public event Action OnTakeDamage;
    public event Action OnDie;

    public bool IsDead => health == 0;

    void Start()
    {
        health = maxHealth;
    }

    public void SetInvulnerable(bool isInvunerable)
    {
        this.isInvunerable = isInvunerable;
    }

    public void DealDamage(float damageValue)
    {
        if (health <= 0) return;

        if (isInvunerable) return;

        health = Mathf.Max(0, health - damageValue);

        //在stateMachine中触发impactState,更广的触发层面
        OnTakeDamage?.Invoke();

        if(health <= 0)
        {
            OnDie?.Invoke();
        }

        Debug.Log(health);

    }


}
