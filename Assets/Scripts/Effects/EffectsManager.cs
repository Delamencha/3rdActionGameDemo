using UnityEngine;
using Combat;
using System.Collections;
using Cinemachine;

/// <summary>
/// Optional hook for VFX prefabs instantiated by EffectsManager to receive the AttackEventArgs context.
/// </summary>
public interface IAttackVfxInitializable
{
    void Initialize(AttackEventArgs args);
}

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

    [Header("Camera Impulse (Cinemachine)")]
    [Tooltip("用于生成震动的 CinemachineImpulseSource（请在 Inspector 中拖拽设置）。")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("VFX")]
    [Tooltip("场景中的特效根节点名称；不存在时会在原点自动创建。")]
    [SerializeField] private string vfxRootName = "VFXRoot";

    private Transform vfxRoot;
    private Transform mainCameraTransform;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        EnsureVfxRoot();
        CacheMainCameraTransform();
    }
    
    private void CacheMainCameraTransform()
    {
        if (mainCameraTransform != null) return;
        if (Camera.main != null) mainCameraTransform = Camera.main.transform;
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

        // If caller didn't specify triggers, default to Swing for backward compatibility.
        var trigger = args.Trigger == AttackEffectTrigger.None ? AttackEffectTrigger.Swing : args.Trigger;

        // Swing: VFX + SFX
        if ((trigger & AttackEffectTrigger.Swing) != 0)
        {
            // 播放攻击挥舞音效（例如出手时）
            if (effectData.SwingSfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(effectData.SwingSfx);
            }

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
                    TryInitializeAttackVfx(instance, args);
                }
                ScheduleDestroyVfx(instance, effectData.SwingVfxDuration);
            }
        }

        // Camera impulse: Cinemachine
        if ((trigger & AttackEffectTrigger.CameraImpulse) != 0)
        {
            TriggerCameraImpulse(args.Attacker, effectData);
        }

        // Hit: VFX + SFX
        if ((trigger & AttackEffectTrigger.Hit) != 0)
        {
            // Hit SFX
            if (effectData.HitSfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(effectData.HitSfx);
            }

            // Hit VFX
            if (effectData.HitVfxPrefab != null)
            {
                Vector3 spawnPos;

                // Prefer hit point if enabled and valid
                if (effectData.SpawnHitVfxAtHitPoint && args.HitPoint != Vector3.zero)
                {
                    spawnPos = args.HitPoint;
                }
                else if (args.Target != null)
                {
                    spawnPos = args.Target.transform.position;
                }
                else if (args.Attacker != null)
                {
                    spawnPos = args.Attacker.transform.position;
                }
                else
                {
                    spawnPos = Vector3.zero;
                }

                spawnPos += effectData.HitVfxOffset;

                EnsureVfxRoot();
                Quaternion spawnRot = Quaternion.identity;
                if (effectData.AlignHitVfxToCamera)
                {
                    CacheMainCameraTransform();
                    if (mainCameraTransform != null)
                    {
                        spawnRot = BuildCameraOrientedHitVfxRotation(mainCameraTransform, effectData.HitVfxScreenRollDeg);
                    }
                }

                var instance = Instantiate(effectData.HitVfxPrefab, spawnPos, spawnRot, vfxRoot);
                ScheduleDestroyVfx(instance, effectData.HitVfxDuration);
            }
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

    private void TryInitializeAttackVfx(GameObject instance, AttackEventArgs args)
    {
        if (instance == null) return;

        // Unity can't directly GetComponentsInChildren by interface type reliably in all versions;
        // scan MonoBehaviours and invoke interface when present.
        var behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            var b = behaviours[i];
            if (b is IAttackVfxInitializable initializable)
            {
                initializable.Initialize(args);
            }
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

    private void TriggerCameraImpulse(GameObject attacker, AttackEffectData effectData)
    {
        if (effectData == null) return;
        if (!effectData.EnableCameraImpulse) return;
        if (impulseSource == null) return;
        if (attacker == null) return;

        Vector3 dir = effectData.CameraImpulseDirection;
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = Vector3.back;
        }
        dir.Normalize();

        if (effectData.CameraImpulseDirectionIsLocalToAttacker)
        {
            dir = attacker.transform.TransformDirection(dir);
        }

        float amplitude = Mathf.Max(0f, effectData.CameraImpulseAmplitude);
        float duration = Mathf.Max(0f, effectData.CameraImpulseDuration);
        AnimationCurve curve = effectData.CameraImpulseCurve;

        // One-shot
        if (duration <= 0.0001f)
        {
            impulseSource.GenerateImpulseWithVelocity(dir * amplitude);
            return;
        }

        StartCoroutine(DriveImpulseOverTime(impulseSource, dir, amplitude, duration, curve));
    }

    private IEnumerator DriveImpulseOverTime(CinemachineImpulseSource source, Vector3 dir, float amplitude, float duration, AnimationCurve curve)
    {
        if (source == null) yield break;
        if (duration <= 0f) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = curve != null ? Mathf.Max(0f, curve.Evaluate(t)) : 1f;
            source.GenerateImpulseWithVelocity(dir * (amplitude * scale));

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 构造一个“更容易观看”的命中特效旋转：
    /// - 让特效的本地 +X（YOZ 平面的法线）尽量朝向相机（使用 -camera.forward）
    /// - 让特效的本地 +Y 近似对齐相机 up，保证屏幕空间的稳定
    /// - 最后绕本地 +X 做一次 roll（屏幕空间角度），用于体现攻击方向（右->左为 0 度）
    /// </summary>
    private static Quaternion BuildCameraOrientedHitVfxRotation(Transform cam, float screenRollDeg)
    {
        Vector3 x = -cam.forward;
        if (x.sqrMagnitude < 0.0001f) x = Vector3.forward;
        x.Normalize();

        Vector3 y = cam.up;
        if (y.sqrMagnitude < 0.0001f) y = Vector3.up;
        y.Normalize();

        // Orthonormalize
        Vector3 z = Vector3.Cross(x, y);
        if (z.sqrMagnitude < 0.0001f)
        {
            // Fallback in degenerate case
            y = Vector3.up;
            z = Vector3.Cross(x, y);
        }
        z.Normalize();
        y = Vector3.Cross(z, x).normalized;

        // Build rotation where local axes map to world axes: X=x, Y=y, Z=z
        Matrix4x4 m = Matrix4x4.identity;
        m.SetColumn(0, new Vector4(x.x, x.y, x.z, 0f));
        m.SetColumn(1, new Vector4(y.x, y.y, y.z, 0f));
        m.SetColumn(2, new Vector4(z.x, z.y, z.z, 0f));
        Quaternion rot = m.rotation;

        // Roll around local X axis (screen space rotation)
        if (Mathf.Abs(screenRollDeg) > 0.0001f)
        {
            rot = rot * Quaternion.AngleAxis(screenRollDeg, Vector3.right);
        }

        return rot;
    }
}


