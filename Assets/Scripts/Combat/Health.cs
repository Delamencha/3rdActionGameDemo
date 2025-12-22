using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    private float health;

    private bool isInvunerable;

    /// <summary>
    /// Whether this character is currently in a blocking state.
    /// This should be set/reset by the character's state machine (e.g. Block states).
    /// </summary>
    public bool IsBlocking { get; set; }

    public event Action<ImpactType> OnTakeDamage;
    public event Action OnDie;

    public bool IsDead => health == 0;

    void Start()
    {
        health = maxHealth;
    }

    public void ActiveInvulnerable()
    {
        this.isInvunerable = true;
    }

    public void DeactiveInvulnerable()
    {
        this.isInvunerable = false;
    }

    public void DealDamage(float damageValue, float knockBack)
    {
        if (health <= 0) return;

        if (isInvunerable) return;

        health = Mathf.Max(0, health - damageValue);

        ImpactType currentImacpType = ImpactType.Light;

        switch (knockBack) 
        {
            case float k when k <= 2f:
                currentImacpType = ImpactType.Light;
                break;
            case float k when k > 2f && k <= 5f:
                currentImacpType = ImpactType.Light;
                break;
            case float k when k > 5f && k <= 8f:
                currentImacpType = ImpactType.Medium;
                break;
            case float k when k >8f:
                currentImacpType = ImpactType.Heavy;
                break;
        }


        //��stateMachine�д���impactState,����Ĵ�������
        OnTakeDamage?.Invoke(currentImacpType);

        if(health <= 0)
        {
            OnDie?.Invoke();
        }

        Debug.Log(health);

    }

    public float getHealth()
    {
        return health;
    }


}
