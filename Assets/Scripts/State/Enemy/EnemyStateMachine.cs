using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStateMachine : StateMachine
{
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public ForceReceiver ForceReceiver { get; private set; }
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [field: SerializeField] public NavMeshAgent Agent { get; private set; }
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public Target Target { get; private set; }
    [field: SerializeField] public Ragdoll Ragdoll { get; private set; }
    [field: SerializeField] public WeaponDamage WeaponDamage { get; private set; }
    [field: SerializeField] public Transform ProjectileSpawnPoint { get; private set; }

    [field: SerializeField] public float ChasingSpeed { get; private set; }

    [field: SerializeField] public float PlayerChasingRange { get; private set; }
    [field: SerializeField] public float AttackRange { get; private set; }

    [field: SerializeField] public float AttackDamage { get; private set; }

    [field: SerializeField] public float AttackKnockBack { get; private set; }
    [field: SerializeField] public EnemyAttackLibrary EnemyAttackLibrary { get; private set; }
    [field: SerializeField] public CommonEffectsData CommonEffectsData { get; private set; }

    public Health Player { get; private set; }

    [Header("SFX (Local)")]
    [Tooltip("角色本地的 AudioSource（用于挥舞音效等需要动画帧事件重复播放的音效）。")]
    [SerializeField] private AudioSource sfxSource;

    private AudioClip lastSwingSfx;

    // Root motion tuning (used by attacks / dodges that rely on root motion)
    private Transform rootMotionTarget;
    private float rootMotionStopDistance;
    private float rootMotionScale = 1f;
    private bool clampRootMotionToStopDistance;

    // Runtime cache/measurement for "clip root distance" (so you can run once, then reuse in-session)
    private readonly Dictionary<int, float> rootMotionDistanceCache = new Dictionary<int, float>();
    private int measuringRootMotionKey;
    private bool isMeasuringRootMotionDistance;
    private float measuredRootMotionDistance;

    private void Awake()
    {
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {

        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();

        Agent.updatePosition = false;
        Agent.updateRotation = false;

        SwitchState(new EnemyIdleState(this));
    }

    public void CacheLastSwingSfx(AudioClip clip)
    {
        lastSwingSfx = clip;
    }

    /// <summary>
    /// 由动画帧事件/代码调用：重放当前攻击缓存的挥舞音效。
    /// </summary>
    public void ReplayLastSwingSfx()
    {
        if (sfxSource == null) return;
        if (lastSwingSfx == null) return;
        sfxSource.PlayOneShot(lastSwingSfx);
    }

    public void PlayCommonSfx(AudioClip clip)
    {
        if (sfxSource == null) return;
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Configure root-motion scaling to move towards a target but stop around stopDistance.
    /// This is evaluated once (on state Enter) and applied each frame in OnAnimatorMove.
    /// </summary>
    public void ConfigureRootMotionTuning(Transform target, float stopDistance, float scale, bool clampToStop = true)
    {
        rootMotionTarget = target;
        rootMotionStopDistance = Mathf.Max(0f, stopDistance);
        rootMotionScale = Mathf.Max(0f, scale);
        clampRootMotionToStopDistance = clampToStop;
    }

    public void ClearRootMotionTuning()
    {
        rootMotionTarget = null;
        rootMotionStopDistance = 0f;
        rootMotionScale = 1f;
        clampRootMotionToStopDistance = false;
    }

    public bool TryGetCachedRootMotionDistance(int key, out float distance)
    {
        return rootMotionDistanceCache.TryGetValue(key, out distance);
    }

    public void BeginRootMotionDistanceMeasurement(int key)
    {
        measuringRootMotionKey = key;
        measuredRootMotionDistance = 0f;
        isMeasuringRootMotionDistance = true;
    }

    public float EndRootMotionDistanceMeasurement(bool cacheResult = true)
    {
        isMeasuringRootMotionDistance = false;
        float result = measuredRootMotionDistance;

        if (cacheResult && measuringRootMotionKey != 0 && result > 0.0001f)
        {
            rootMotionDistanceCache[measuringRootMotionKey] = result;
        }

        measuringRootMotionKey = 0;
        measuredRootMotionDistance = 0f;
        return result;
    }

    private void OnAnimatorMove()
    {
        if (Animator == null || Controller == null) return;
        if (!Animator.applyRootMotion) return;

        Vector3 delta = Animator.deltaPosition;
        delta.y = 0f; // planar displacement only for enemy attacks/dodges

        if (isMeasuringRootMotionDistance)
        {
            measuredRootMotionDistance += delta.magnitude;
        }

        if (rootMotionTarget != null)
        {
            Vector3 toTarget = rootMotionTarget.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Vector3 dir = toTarget.normalized;

                // Only scale the component moving towards the target; keep lateral motion unchanged.
                float towards = Vector3.Dot(delta, dir);
                Vector3 towardsDelta = dir * towards;
                Vector3 lateralDelta = delta - towardsDelta;

                if (towards > 0f)
                {
                    towardsDelta *= rootMotionScale;
                }

                delta = lateralDelta + towardsDelta;

                // Clamp to stop distance to avoid overshooting / passing through the player.
                if (clampRootMotionToStopDistance)
                {
                    float dist = toTarget.magnitude;
                    float remaining = dist - rootMotionStopDistance;

                    float t = Vector3.Dot(delta, dir);
                    if (remaining <= 0f)
                    {
                        // Already within stop distance: remove any further movement towards target.
                        if (t > 0f) delta -= dir * t;
                    }
                    else
                    {
                        // Don't move closer than stop distance.
                        if (t > remaining) delta -= dir * (t - remaining);
                    }
                }
            }
        }
        else
        {
            // No target: uniform scaling (default 1).
            delta *= rootMotionScale;
        }

        // Add external forces (impact / gravity) so root-motion states still respond to ForceReceiver.
        if (ForceReceiver != null)
        {
            delta += ForceReceiver.Movement * Time.deltaTime;
        }

        Controller.Move(delta);
    }

    private void OnEnable()
    {
        //Health.OnTakeDamage += HandleTakeDamage;
        //Health.OnDie += HandleDeath;
    }

    private void OnDisable()
    {
        //Health.OnTakeDamage -= HandleTakeDamage;
        //Health.OnDie -= HandleDeath;
    }

    private void HandleTakeDamage(ImpactType impactType)
    {
        SwitchState(new EnemyImpactState(this));
    }

    private void HandleDeath()
    {
        SwitchState(new EnemyDeadState(this));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, PlayerChasingRange);
    }



}
