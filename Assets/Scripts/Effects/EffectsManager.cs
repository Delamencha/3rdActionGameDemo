using UnityEngine;
using Combat;

/// <summary>
/// 统一处理攻击/受击相关的特效与音效。
/// - 订阅 CombatEvents 中的战斗事件
/// - 根据 AttackEffectData / HitEffectData 播放对应的 VFX / SFX
/// </summary>
public class EffectsManager : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("用于播放攻击/受击音效的 AudioSource；若为空，则会在运行时自动尝试获取自身的 AudioSource。")]
    [SerializeField] private AudioSource audioSource;

    [Header("VFX")]
    [Tooltip("场景中的特效根节点名称；不存在时会在原点自动创建。")]
    [SerializeField] private string vfxRootName = "VFXRoot";

    private Transform vfxRoot;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        EnsureVfxRoot();
    }

    private void EnsureVfxRoot()
    {
        if (vfxRoot != null) return;

        GameObject root = GameObject.Find(vfxRootName);
        if (root == null)
        {
            root = new GameObject(vfxRootName);
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
        }

        vfxRoot = root.transform;
    }

    private void OnEnable()
    {
        CombatEvents.OnAttackPerformed += OnAttackPerformed;
        CombatEvents.OnDamaged += OnDamaged;
    }

    private void OnDisable()
    {
        CombatEvents.OnAttackPerformed -= OnAttackPerformed;
        CombatEvents.OnDamaged -= OnDamaged;
    }

    /// <summary>
    /// 攻击事件回调：播放攻击相关特效与音效。
    /// 当前仅用于调试，可按需要继续扩展。
    /// </summary>
    private void OnAttackPerformed(AttackEventArgs args)
    {
        var effectData = args.EffectData;
        if (effectData == null)
        {
            return;
        }


        // 播放攻击挥舞音效（例如出手时）
        if (effectData.SwingSfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(effectData.SwingSfx);
        }

        // 命中表现：特效 + 音效
        // 这里只做一个最小可见版本：在命中点或目标身上实例化 HitVfxPrefab，并播放 HitSfx。
        //if (effectData.HitVfxPrefab != null)
        //{
        //    Vector3 spawnPos = args.HitPoint;

        //    // 如果没有可靠的命中点，则退化到目标位置（再退化到攻击者位置）
        //    if (spawnPos == Vector3.zero)
        //    {
        //        if (args.Target != null)
        //        {
        //            spawnPos = args.Target.transform.position;
        //        }
        //        else if (args.Attacker != null)
        //        {
        //            spawnPos = args.Attacker.transform.position;
        //        }
        //    }

        //    spawnPos += effectData.HitVfxOffset;

        //    Instantiate(effectData.HitVfxPrefab, spawnPos, Quaternion.identity);
        //}

        if (effectData.SwingVfxPrefab != null)
        {
            // Swing 特效：以攻击者 Transform 为基准应用“相对位置/旋转/缩放”
            Transform attackerTr = args.Attacker != null ? args.Attacker.transform : null;

            Vector3 spawnPos = attackerTr != null
                ? attackerTr.TransformPoint(effectData.SwingVfxLocalPosition)
                : (args.Attacker != null ? args.Attacker.transform.position : Vector3.zero) + effectData.SwingVfxLocalPosition;

            Quaternion spawnRot = attackerTr != null
                ? attackerTr.rotation * Quaternion.Euler(effectData.SwingVfxLocalEuler)
                : Quaternion.Euler(effectData.SwingVfxLocalEuler);

            EnsureVfxRoot();
            var instance = Instantiate(effectData.SwingVfxPrefab, spawnPos, spawnRot, vfxRoot);
            if (instance != null)
            {
                instance.transform.localScale = effectData.SwingVfxScale;
            }
            ScheduleDestroyVfx(instance, effectData.SwingVfxDuration);
        }

        //if (effectData.HitSfx != null && audioSource != null)
        //{
        //    audioSource.PlayOneShot(effectData.HitSfx);
        //}
    }

    /// <summary>
    /// 受击事件回调：当前只是预留，可在之后需要时实现。
    /// </summary>
    private void OnDamaged(DamageEventArgs args)
    {
        var hitData = args.HitEffectData;
        if (hitData == null)
        {
            return;
        }

        // 简单示例：在受击者位置播放一个通用受击特效和音效。
        if (hitData.HitVfxPrefab != null && args.Victim != null)
        {
            Vector3 spawnPos = args.Victim.transform.position + hitData.HitVfxOffset;
            EnsureVfxRoot();
            Instantiate(hitData.HitVfxPrefab, spawnPos, Quaternion.identity, vfxRoot);
        }

        if (hitData.HitSfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitData.HitSfx);
        }
    }

    private void ScheduleDestroyVfx(GameObject instance, float configuredDurationSeconds)
    {
        if (instance == null) return;

        float duration = configuredDurationSeconds;
        if (duration <= 0f)
        {
            duration = TryGetAutoVfxDurationSeconds(instance);
        }

        if (duration > 0f)
        {
            Destroy(instance, duration);
        }
    }

    /// <summary>
    /// 尝试从粒子系统估算一个“合理的自动销毁时间”，避免忘配持续时间导致特效实例堆积。
    /// </summary>
    private float TryGetAutoVfxDurationSeconds(GameObject instance)
    {
        float max = -1f;

        var particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var ps = particleSystems[i];
            var main = ps.main;

            // Looping VFX shouldn't be auto-destroyed unless explicitly configured.
            if (main.loop)
            {
                continue;
            }

            // duration + startDelay + startLifetime(max)
            float startDelay = main.startDelay.constantMax;
            float startLifetime = main.startLifetime.constantMax;
            float estimate = main.duration + startDelay + startLifetime;
            if (estimate > max) max = estimate;
        }

        return max;
    }
}


