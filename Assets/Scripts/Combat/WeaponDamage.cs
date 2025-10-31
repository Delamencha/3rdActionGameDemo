using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    [SerializeField] private Collider myCollider;
    

    private List<Collider> alreadyColliderWith = new List<Collider>();

    private float damageValue;
    private float knockBack;
    //暂时只考虑一次攻击只会与一个盾牌交互
    private Health blockedHealth; // recorded owner of the blocking shield
    

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

        Debug.Log("Other: " + other.gameObject.name);

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
            Vector3 direction = (other.transform.position - myCollider.transform.position).normalized ;
            forceReceiver.AddForce(direction * knockBack);
        }

    }

    public void SetAttack(float damageValue, float knockBack)
    {
        this.damageValue = damageValue;

        this.knockBack = knockBack;

    }

}
