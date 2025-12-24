using UnityEngine;

/// <summary>
/// “命中判定”用的 Hurtbox：挂在敌人骨骼子物体（Capsule/Box/SphereCollider, IsTrigger=true）。
/// 目的：在保留 CharacterController（移动碰撞）的同时，实现更精细的武器命中判定。
/// </summary>
public class Hurtbox : MonoBehaviour
{
    [Tooltip("该 Hurtbox 归属的 Health（建议在 Inspector 指定；未指定则会在 Awake 时从父级自动查找）。")]
    [SerializeField] private Health ownerHealth;

    public Health OwnerHealth => ownerHealth;

    private void Awake()
    {
        if (ownerHealth == null)
        {
            ownerHealth = GetComponentInParent<Health>();
        }
    }
}


