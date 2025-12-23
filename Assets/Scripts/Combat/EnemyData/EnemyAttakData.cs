using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Enemy Attack Data")]
public class EnemyAttakData : ScriptableObject
{
    [Header("Animation")]
    public string AnimationName;
    public float TransitionDuration = 0.1f;
    public bool applyRootMotion = false;

    [Header("Root Motion Distance Match (Optional)")]
    [Tooltip("If enabled, scales this attack's root motion so the enemy ends up around EnemyStateMachine.AttackRange from the player (computed once on Enter).")]
    public bool enableDistanceMatch = false;
    [Tooltip("Approximate total planar root-motion distance of this animation clip (meters). Used to compute scale = neededDistance / clipRootDistance.")]
    public float clipRootDistance = 0f;
    [Tooltip("Clamp scale to avoid extreme stretching/shrinking.")]
    public float minRootScale = 0f;
    public float maxRootScale = 3f;

    [Header("Curve")]
    public string curveName;

    [Header("Type")]
    public EnemyAttackType EnemyAttackType = EnemyAttackType.MeleeAttack;

    [Header("Projectile (Range Attack)")]
    [Tooltip("Prefab to spawn when EnemyAttackType is RangeAttack. Prefab should have a Projectile component.")]
    public GameObject projectilePrefab;

    [Tooltip("When to spawn the projectile during the attack animation (normalized time of Animator tag 'Attack').")]
    [Range(0f, 1f)]
    public float projectileSpawnNormalizedTime = 0.3f;

    [Tooltip("Optional spawn offset (local space) applied on top of the chosen spawn point.")]
    public Vector3 projectileSpawnOffset = Vector3.zero;

    [Tooltip("Projectile movement speed (m/s).")]
    public float projectileSpeed = 12f;

    [Tooltip("Projectile lifetime before auto-destroy (seconds).")]
    public float projectileLifetime = 5f;

    [Tooltip("Delay before enabling the projectile hit collider (seconds). Useful to avoid immediate self-collisions at spawn).")]
    public float projectileColliderEnableDelay = 0f;

    [Tooltip("If enabled, projectile will home towards the player each frame.")]
    public bool projectileHoming = false;

    [Tooltip("Homing rotation speed (deg/s) when projectileHoming is enabled.")]
    public float projectileHomingTurnSpeedDeg = 720f;

    [Header("Damage / Impact")]
    public List<float> damageValue = new List<float>();
    public List<float> knockbackValue = new List<float>();
    public KnockbackType knockbackType = KnockbackType.AwayFromAttacker;

    [Header("Facing / Limits")]
    public float TotalTurnLimitDeg = 60f;
}


