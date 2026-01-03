using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Combat;


public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [Header("Hit Flash (optional)")]
    [SerializeField] private HitFlashController hitFlashController;

    private float health;

    private bool isInvunerable;

    public bool IsInvulnerable => isInvunerable;

    /// <summary>
    /// True when the character is currently in a "perfect dodge" window.
    /// Set/reset by state machines (e.g., PlayerDodgeState).
    /// </summary>
    public bool IsPerfectDodging { get; set; }

    /// <summary>
    /// True when the character is currently in a "perfect block" window.
    /// Set/reset by state machines (e.g., PlayerBlockState) using real time.
    /// </summary>
    public bool IsPerfectBlocking { get; set; }

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

        ApplyDamageInternal(damageValue, knockBack);

        // Visual feedback: victim hit flash (optional)
        if (hitFlashController != null)
        {
            hitFlashController.Play();
        }

        Debug.Log(health);

    }

    /// <summary>
    /// 由 WeaponDamage 等系统调用：把“是否算命中/是否播放受击特效音效”的判定交给 Health。
    /// - 若处于无敌帧（isInvunerable），则视为成功闪避：不扣血、不触发受击事件/特效。
    /// - 若成功造成伤害，则可在此统一触发 CombatEvents 的 Hit 表现。
    /// </summary>
    public bool TryApplyAttackHit(GameObject attacker, float damageValue, float knockBack, Vector3 hitPoint, AttackEffectData attackEffect)
    {
        if (health <= 0) return false;
        if (isInvunerable)
        {
            // Perfect dodge feedback (optional): play common SFX when in perfect dodge window.
            //暂时不区分dodge是否perfect
            //if (IsPerfectDodging)
            //{
                var psm = GetComponentInParent<PlayerStateMachine>();
                if (psm != null && psm.CommonEffectsData != null && psm.CommonEffectsData.perfectDodgeSFX != null)
                {
                    psm.PlayCommonSfx(psm.CommonEffectsData.perfectDodgeSFX);
                }
            //}

            return false;
        }

        ApplyDamageInternal(damageValue, knockBack);

        // Visual feedback: victim hit flash (optional)
        if (hitFlashController != null)
        {
            hitFlashController.Play();
        }

        // Trigger VFX/SFX/Hitstop for a successful hit
        if (attackEffect != null)
        {
            var args = new AttackEventArgs
            {
                Attacker = attacker,
                Target = gameObject,
                HitPoint = hitPoint == Vector3.zero ? transform.position : hitPoint,
                EffectData = attackEffect,
                Trigger = AttackEffectTrigger.Hit
            };
            CombatEvents.RaiseAttackPerformed(args);
        }

        return true;
    }

    /// <summary>
    /// Called when an incoming attack is successfully blocked (normal block for now).
    /// This lets Health decide whether to play block-related SFX/VFX based on current state.
    /// </summary>
    public void NotifyBlocked(GameObject attacker, Vector3 hitPoint)
    {
        // Perfect block decision is owned by Health (state-driven window).
        bool isPerfectBlock = IsBlocking && IsPerfectBlocking;
        AudioClip clip = null;

        var psm = GetComponentInParent<PlayerStateMachine>();
        if (psm != null && psm.CommonEffectsData != null)
        {
            clip = isPerfectBlock ? psm.CommonEffectsData.perfectBlockSFX : psm.CommonEffectsData.blockSFX;
            psm.PlayCommonSfx(clip);
            return;
        }

        var esm = GetComponentInParent<EnemyStateMachine>();
        if (esm != null && esm.CommonEffectsData != null)
        {
            clip = isPerfectBlock ? esm.CommonEffectsData.perfectBlockSFX : esm.CommonEffectsData.blockSFX;
            esm.PlayCommonSfx(clip);
        }
    }

    private void ApplyDamageInternal(float damageValue, float knockBack)
    {
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
            case float k when k > 8f:
                currentImacpType = ImpactType.Heavy;
                break;
        }

        OnTakeDamage?.Invoke(currentImacpType);

        if (health <= 0)
        {
            OnDie?.Invoke();
        }
    }

    public float getHealth()
    {
        return health;
    }


}
