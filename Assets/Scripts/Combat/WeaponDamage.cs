using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    [SerializeField] private Collider myCollider;
    

    private List<Collider> alreadyColliderWith = new List<Collider>();

    private float damageValue;
    private float knockBack;
    private KnockbackType knockBackType;
    //暂时只考虑一次攻击只会与一个盾牌交互
    private Health blockedHealth; // recorded owner of the blocking shield
    
    public event Action<Target> OnTargetHit;

    private void OnEnable()
    {
        alreadyColliderWith.Clear();
        blockedHealth = null;
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == myCollider) return;

        if (alreadyColliderWith.Contains(other)) return;
        
        alreadyColliderWith.Add(other);

        //Debug.Log("Other: " + other.gameObject.name);

        // 1) If we hit a shield, perform one-time directional block check and record owner if front-blocked
        if (other.CompareTag("Shield"))
        {
            if ( other.TryGetComponent<ShieldReference>(out ShieldReference shieldRef) && shieldRef.Health != null)
            {

                if(myCollider.gameObject.GetComponent<Health>() != null && shieldRef.Health == myCollider.gameObject.GetComponent<Health>()) return;

                // Compute attack direction = shield.position - closestPointOnShield
                Vector3 closestPointOnShield = other.ClosestPoint(myCollider.transform.position);
                Vector3 attackDirection = (other.transform.position - closestPointOnShield).normalized;

                // Angle between attackDirection and shield forward
                float angle = Vector3.Angle(other.transform.forward, attackDirection);

                Debug.Log("Angle: " + angle);

                // If angle > 90, treat as frontal attack => block
                if (angle > 90f)
                {
                    blockedHealth = shieldRef.Health;
                }

            }
            // If already checked this enable, still exit early when colliding with shield
            return;
        }

        if (other.TryGetComponent<Health>(out Health health))
        {
            // 2) If the hit belongs to the same Health that blocked with shield, skip damage but still apply force
            if (health != blockedHealth)
            {
                health.DealDamage(damageValue, knockBack);
            }
        }

        if(other.TryGetComponent<ForceReceiver>(out ForceReceiver forceReceiver))
        {
            //Vector3 direction = (other.transform.position - myCollider.transform.position).normalized ;
            Vector3 direction = GetKnockbackDirection(myCollider.transform, other.transform);
            forceReceiver.AddForce(direction * knockBack);
        }

        // 3) Notify soft lock system about target hit
        if (other.TryGetComponent<Target>(out var target))
        {
            OnTargetHit?.Invoke(target);
        }

    }

    public void SetAttack(float damageValue, float knockBack, KnockbackType knockbackType)
    {
        this.damageValue = damageValue;

        this.knockBack = knockBack;

        this.knockBackType = knockbackType;

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
