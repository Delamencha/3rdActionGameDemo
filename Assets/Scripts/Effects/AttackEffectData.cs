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

    [Header("Swing VFX Transform")]
    [Tooltip("SwingVfxPrefab 生成时相对攻击者(Attacker)的本地偏移位置（单位：米）。")]
    public Vector3 SwingVfxLocalPosition = Vector3.zero;

    [Tooltip("SwingVfxPrefab 生成时相对攻击者(Attacker)的欧拉角旋转偏移（单位：度）。")]
    public Vector3 SwingVfxLocalEuler = Vector3.zero;

    [Tooltip("SwingVfxPrefab 实例的缩放（世界缩放；通常 1,1,1）。")]
    public Vector3 SwingVfxScale = Vector3.one;

    [Header("Swing VFX Timing")]
    [Tooltip("SwingVfxPrefab 出现时间（动画归一化时间，范围 [0,1]）。当 Attack normalizedTime >= 该值时触发一次。")]
    [Range(0f, 1f)]
    public float SwingVfxSpawnNormalizedTime = 0f;

    [Tooltip("SwingVfxPrefab 实例的持续时间（秒）。<= 0 表示不强制使用该值（EffectsManager 会尝试从粒子系统自动估算）。")]
    [Min(0f)]
    public float SwingVfxDuration = 0f;

    [Tooltip("命中目标时播放的特效（常用）。")]
    public GameObject HitVfxPrefab;

    [Tooltip("HitVfxPrefab 实例的持续时间（秒）。<= 0 表示不强制使用该值（EffectsManager 会尝试从粒子系统自动估算）。")]
    [Min(0f)]
    public float HitVfxDuration = 0f;

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

    [Header("Camera Impulse (Cinemachine)")]
    [Tooltip("是否启用攻击镜头震动（通过 Cinemachine Impulse Source/Listener）。")]
    public bool EnableCameraImpulse = false;

    [Tooltip("震动触发时机（动画归一化时间，范围 [0,1]）。当 Attack normalizedTime >= 该值时触发一次。")]
    [Range(0f, 1f)]
    public float CameraImpulseNormalizedTime = 0f;

    [Header("Camera Impulse Direction")]
    [Tooltip("震动方向向量（建议填写单位向量）。")]
    public Vector3 CameraImpulseDirection = new Vector3(0f, 0f, -1f);

    [Tooltip("如果勾选，则 CameraImpulseDirection 视为攻击者(Attacker)的本地方向；否则为世界方向。")]
    public bool CameraImpulseDirectionIsLocalToAttacker = true;

    [Header("Camera Impulse Shape")]
    [Tooltip("震动强度缩放（最终会影响给 Impulse Source 的速度/力度）。")]
    [Min(0f)]
    public float CameraImpulseAmplitude = 1f;

    [Tooltip("震动持续时长（秒）。如果你使用的是 ImpulseDefinition 的 Envelope，这个值通常用于简化设置/驱动。")]
    [Min(0f)]
    public float CameraImpulseDuration = 0.15f;

    [Tooltip("震动强度随时间变化曲线（0~1 秒区间）。X=归一化时间(0..1)，Y=强度倍率。")]
    public AnimationCurve CameraImpulseCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
}


