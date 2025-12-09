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

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
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
        if (effectData.HitVfxPrefab != null)
        {
            Vector3 spawnPos = args.HitPoint;

            // 如果没有可靠的命中点，则退化到目标位置（再退化到攻击者位置）
            if (spawnPos == Vector3.zero)
            {
                if (args.Target != null)
                {
                    spawnPos = args.Target.transform.position;
                }
                else if (args.Attacker != null)
                {
                    spawnPos = args.Attacker.transform.position;
                }
            }

            spawnPos += effectData.HitVfxOffset;

            Instantiate(effectData.HitVfxPrefab, spawnPos, Quaternion.identity);
        }

        if (effectData.HitSfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(effectData.HitSfx);
        }
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
            Instantiate(hitData.HitVfxPrefab, spawnPos, Quaternion.identity);
        }

        if (hitData.HitSfx != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitData.HitSfx);
        }
    }
}


