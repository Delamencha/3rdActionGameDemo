using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

public class DistanceMonitor : MonoBehaviour
{
    public BehaviorTree behaviorTree;
    public Transform player;
    public Transform enemy;

    private Health enemyHealth;
    [SerializeField] private WeaponDamage weaponDamage;

    [Header("Blackboard Variables")]
    public string distanceVariableName = "PlayerDis";
    public string enemyHealthVariableName = "ownHealth";
    public string jumpAwayCounterVariableName = "JumpAwayCounter";


    // This is expected to be updated by an external Behavior Designer bridge script (blackboard -> MonoBehaviour).
    public int jumpAwayCounter { get; set; }

    [Header("Jump Away Counter Settings")]
    [Tooltip("When this enemy takes damage, JumpAwayCounter will be increased by this amount.")]
    public int onTakeDamageAdd = 2;
    [Tooltip("When this enemy causes damage (attack hit), JumpAwayCounter will be increased by this amount.")]
    public int onCauseDamageAdd = 1;


    private void Start()
    {

        enemyHealth = GetComponent<Health>();

    }

    private void OnEnable()
    {
        // `Start` might not have run yet (and this component may be toggled on/off).
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<Health>();
        }

        if (enemyHealth != null)
        {
            enemyHealth.OnTakeDamage += HandleTakeDamage;
        }

        CacheWeaponDamageIfNeeded();
        if (weaponDamage != null)
        {
            weaponDamage.OnCauseDamage += HandleCauseDamage;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnTakeDamage -= HandleTakeDamage;
        }

        if (weaponDamage != null)
        {
            weaponDamage.OnCauseDamage -= HandleCauseDamage;
        }
    }

    void Update()
    {
        if (behaviorTree != null && player != null && enemy != null && enemyHealth != null)
        {
            float currentDistance = Vector3.Distance(player.position, enemy.position);

            // ֱ��������Ϊ���ڰ����
            behaviorTree.SetVariableValue(distanceVariableName, currentDistance);

            behaviorTree.SetVariableValue(enemyHealthVariableName, enemyHealth.getHealth());

            
        }
    }

    private void HandleTakeDamage(ImpactType impactType)
    {
        IncrementJumpAwayCounter(onTakeDamageAdd);
    }

    private void HandleCauseDamage()
    {
        IncrementJumpAwayCounter(onCauseDamageAdd);
    }

    private void IncrementJumpAwayCounter(int delta)
    {
        if (behaviorTree == null || string.IsNullOrEmpty(jumpAwayCounterVariableName)) return;

        // `jumpAwayCounter` is expected to already reflect the current blackboard value (via external bridge).
        int res = jumpAwayCounter + delta;
        behaviorTree.SetVariableValue(jumpAwayCounterVariableName, res);
        jumpAwayCounter = res;
    }

    private void CacheWeaponDamageIfNeeded()
    {
        if (weaponDamage != null) return;
        weaponDamage = GetComponentInChildren<WeaponDamage>(true);
    }
}
