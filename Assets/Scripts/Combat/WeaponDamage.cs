using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Combat;

public class WeaponDamage : MonoBehaviour
{
    [SerializeField] private Collider myCollider;

    private List<Collider> alreadyColliderWith = new List<Collider>();
    private HashSet<Health> alreadyDamagedHealth = new HashSet<Health>();

    private float damageValue;
    private float knockBack;
    private KnockbackType knockBackType;
    private AttackEffectData currentAttackEffect;
    
    public event Action<Target> OnTargetHit;
    /// <summary>
    /// Fired when this weapon successfully causes damage to a target (not blocked).
    /// Useful for AI/behavior logic (e.g. counting "hit landed").
    /// </summary>
    public event Action OnCauseDamage;

    private void OnEnable()
    {
        alreadyColliderWith.Clear();
        alreadyDamagedHealth.Clear();
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == myCollider) return;

        if (alreadyColliderWith.Contains(other)) return;

       // Debug.Log("Other: " + other.gameObject.name);

        alreadyColliderWith.Add(other);

        //Debug.Log("Other: " + other.gameObject.name);

        // Only interact with Hurtbox colliders (Weapon layer <-> Hurtbox layer should also be configured in Physics matrix).
        var hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox == null || hurtbox.OwnerHealth == null)
        {
            return;
        }

        // Resolve Health from Hurtbox (avoid ambiguity with CharacterController capsule collider).
        var health = hurtbox.OwnerHealth;
        bool isBlocked = false;

        if (health != null)
        {
            // Avoid multi-hit when multiple hurtbox colliders belong to the same target within one swing.
            if (alreadyDamagedHealth.Contains(health)) return;

            // Prevent self-hit (attacker's weapon hitting attacker's own colliders).
            var selfHealth = myCollider != null ? myCollider.GetComponentInParent<Health>() : null;
            if (selfHealth != null && selfHealth == health) return;

            if (health.IsBlocking && IsFrontalHit(health.transform, myCollider != null ? myCollider.transform : transform))
            {
                // Blocked: no damage, and also do not raise Hit events (same behavior as old shield path).
                isBlocked = true;
            }
            else
            {
                alreadyDamagedHealth.Add(health);
                health.DealDamage(damageValue, knockBack);
                OnCauseDamage?.Invoke();

                // Raise "Hit" event for VFX/SFX/Hitstop systems only on successful damage.
                if (currentAttackEffect != null)
                {
                    var args = new AttackEventArgs
                    {
                        Attacker = myCollider != null ? myCollider.gameObject : null,
                        Target = health.gameObject,
                        //HitPoint = other.ClosestPoint(myCollider != null ? myCollider.transform.position : transform.position),
                        HitPoint = other.ClosestPoint(transform.position),
                        EffectData = currentAttackEffect,
                        Trigger = AttackEffectTrigger.Hit
                    };

                    CombatEvents.RaiseAttackPerformed(args);
                }
            }
        }

        // Apply knockback force even if blocked (feels better and matches prior behavior).
        var forceReceiver = health.GetComponentInParent<ForceReceiver>();
        if (forceReceiver != null)
        {
            Vector3 direction = GetKnockbackDirection(myCollider != null ? myCollider.transform : transform, other.transform);
            forceReceiver.AddForce(direction * knockBack);
        }

        // Notify soft lock system about target hit (blocked or not).
        var target = health.GetComponentInParent<Target>();
        if (target != null)
        {
            OnTargetHit?.Invoke(target);
        }

    }

    public void SetAttack(float damageValue, float knockBack, KnockbackType knockbackType, AttackEffectData attackEffect)
    {
        this.damageValue = damageValue;

        this.knockBack = knockBack;

        this.knockBackType = knockbackType;
        this.currentAttackEffect = attackEffect;

    }

    private static bool IsFrontalHit(Transform defender, Transform attacker)
    {
        if (defender == null || attacker == null) return false;

        Vector3 toAttacker = attacker.position - defender.position;
        toAttacker.y = 0f;
        if (toAttacker.sqrMagnitude < 0.0001f) return true;
        toAttacker.Normalize();

        Vector3 defenderForward = defender.forward;
        defenderForward.y = 0f;
        if (defenderForward.sqrMagnitude < 0.0001f) return true;
        defenderForward.Normalize();

        // Attacker is in front hemisphere of defender when dot > 0 (angle < 90).
        return Vector3.Dot(defenderForward, toAttacker) > 0f;
    }

    public Vector3 GetKnockbackDirection(Transform attacker, Transform hitTarget)
    {
        Vector3 direction = myCollider.transform.forward;

        switch (knockBackType)
        {
            case KnockbackType.Forward:
                direction = attacker.forward;
                break;

            case KnockbackType.AwayFromAttacker:
                direction = hitTarget.position - attacker.position;
                direction.y = 0; // 保持配置的Y分量
                break;

            case KnockbackType.TowardsAttacker:
                direction = attacker.position - hitTarget.position;
                break;

            case KnockbackType.Upwards:
                direction = Vector3.up;
                break;

        }

        return direction.normalized;

    }

}
