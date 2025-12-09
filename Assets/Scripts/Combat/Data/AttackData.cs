using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack Data")]
public class AttackData : ScriptableObject
{


    [Header("Animation")]
    public string AnimationName;
    public float TransitionDuration = 0.1f;

    [Header("Combo / Flow")]
    //������������
    public int LightComboStateIndex = -1;       // -1 means no next
    public int HeavyComboStateIndex = -1;
    public float ComboAttackTime = 0.5f;   // normalized time to accept next input
    public float AnimationCancelTime = 0.5f; //normalized time to cancel current animation & jump to another state

    [Header("Timing / Motion")]
    public float ForceTime = 0f;        // normalized time to apply forward force
    public float AttackForce = 10f;

    [Header("Damage / Impact")]
    public float DamageValue = 10f;
    public float Knockback = 5f;
    public KnockbackType knockbackType = KnockbackType.AwayFromAttacker;

    [Header("Facing / Limits")]
    public float TotalTurnLimitDeg = 0f;

    [Header("VFX / SFX")]
    [Tooltip("本次攻击使用的特效与音效配置，如果为空则不播放额外效果。")]
    public AttackEffectData AttackEffect;

}


