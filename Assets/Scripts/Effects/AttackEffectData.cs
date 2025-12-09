using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack Effect Data")]
public class AttackEffectData : ScriptableObject
{
    [Header("Identification")]
    [Tooltip("用于在调试时区分不同的攻击效果数据，可选。")]
    public string EffectId;

    [Header("VFX")]
    [Tooltip("攻击挥舞或起手时播放的特效（可选）。")]
    public GameObject SwingVfxPrefab;

    [Tooltip("命中目标时播放的特效（常用）。")]
    public GameObject HitVfxPrefab;

    [Tooltip("命中特效生成在命中点时的偏移量。")]
    public Vector3 HitVfxOffset = Vector3.zero;

    [Header("SFX")]
    [Tooltip("攻击挥舞/出手时播放的音效。")]
    public AudioClip SwingSfx;

    [Tooltip("命中目标时播放的音效。")]
    public AudioClip HitSfx;

    [Header("Spawn Settings")]
    [Tooltip("如果为 true，则命中特效优先生成在 WeaponDamage 命中点附近；否则生成在目标位置。")]
    public bool SpawnHitVfxAtHitPoint = true;
}


