using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 负责“局部 Hitstop”的冻结/恢复：只冻结该角色（Animator/StateMachine/ForceReceiver/NavMeshAgent）。
/// 通过序列化的 stateMachine 获取引用，避免 GetComponent 扫描。
/// </summary>
public class HitstopReceiver : MonoBehaviour
{
    [SerializeField] private StateMachine stateMachine;

    [Header("Hitstop Cooldown")]
    [Tooltip("命中停滞的冷却时间（秒，使用 unscaled time）。冷却期间不会开启新的 hitstop 轮次。")]
    [Min(0f)]
    [SerializeField] private float hitstopCooldown = 0.1f;

    private Animator animator;
    private ForceReceiver forceReceiver;
    private NavMeshAgent agent;

    private bool frozen;
    private float nextAllowedStartUnscaledTime;

    private float animatorSpeedBackup;
    private bool stateMachineEnabledBackup;
    private bool forceReceiverEnabledBackup;
    private bool agentEnabledBackup;

    private void Awake()
    {
        if (stateMachine == null)
        {
            Debug.LogWarning("[HitstopReceiver] stateMachine not assigned.");
            return;
        }

        if (stateMachine is PlayerStateMachine p)
        {
            animator = p.Animator;
            forceReceiver = p.ForceReceiver;
            agent = null;
        }
        else if (stateMachine is EnemyStateMachine e)
        {
            animator = e.Animator;
            forceReceiver = e.ForceReceiver;
            agent = e.Agent;
        }
        else
        {
            Debug.LogWarning("[HitstopReceiver] Unsupported StateMachine type: " + stateMachine.GetType().Name);
        }
    }

    public void Freeze()
    {
        if (frozen) return;
        frozen = true;

        if (animator != null)
        {
            animatorSpeedBackup = animator.speed;
            animator.speed = 0f;
        }

        if (stateMachine != null)
        {
            stateMachineEnabledBackup = stateMachine.enabled;
            stateMachine.enabled = false;
        }

        if (forceReceiver != null)
        {
            forceReceiverEnabledBackup = forceReceiver.enabled;
            forceReceiver.enabled = false;
        }

        if (agent != null)
        {
            agentEnabledBackup = agent.enabled;
            agent.enabled = false;
        }
    }

    public void Unfreeze()
    {
        if (!frozen) return;
        frozen = false;

        if (animator != null) animator.speed = animatorSpeedBackup;
        if (stateMachine != null) stateMachine.enabled = stateMachineEnabledBackup;
        if (forceReceiver != null) forceReceiver.enabled = forceReceiverEnabledBackup;
        if (agent != null) agent.enabled = agentEnabledBackup;
    }

    public bool CanStartHitstop(float nowUnscaledTime)
    {
        return nowUnscaledTime >= nextAllowedStartUnscaledTime;
    }

    public void NotifyHitstopStarted(float nowUnscaledTime)
    {
        nextAllowedStartUnscaledTime = nowUnscaledTime + hitstopCooldown;
    }
}


