using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Enemy Attack Data")]
public class EnemyAttakData : ScriptableObject
{
    [Header("Animation")]
    public string AnimationName;
    public float TransitionDuration = 0.1f;
    public bool applyRootMotion = false;

    [Header("Curve")]
    public string curveName;

    [Header("Type")]
    public EnemyAttackType EnemyAttackType = EnemyAttackType.MeleeAttack;

    [Header("Damage / Impact")]
    public List<float> damageValue = new List<float>();
    public List<float> knockbackValue = new List<float>();
    public KnockbackType knockbackType = KnockbackType.AwayFromAttacker;

    [Header("Facing / Limits")]
    public float TotalTurnLimitDeg = 60f;
}


