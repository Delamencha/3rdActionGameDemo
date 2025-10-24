using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack Data")]
public class AttackData : ScriptableObject
{
    [Header("Animation")]
    public string AnimationName;
    public float TransitionDuration = 0.1f;

    [Header("Combo / Flow")]
    public int ComboStateIndex = -1;       // -1 means no next
    public float ComboAttackTime = 0.5f;   // normalized time to accept next input

    [Header("Timing / Motion")]
    public float ForceTime = 0f;        // normalized time to apply forward force
    public float AttackForce = 10f;

    [Header("Damage / Impact")]
    public float DamageValue = 10f;
    public float Knockback = 5f;

    [Header("Facing / Limits")]
    public float TotalTurnLimitDeg = 0f; // 0 = unlimited
}


