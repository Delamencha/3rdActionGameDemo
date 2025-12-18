using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Combat;

/// <summary>
/// 监听命中事件并对攻击者/受击者执行“局部 Hitstop”（不修改 Time.timeScale）。
/// 叠加策略：
/// - 若角色已处于 hitstop，再次触发会延长停滞时间
/// - 但“本轮 hitstop”的总时长不会超过（首次触发时长 * 2）
/// </summary>
public class HitstopManager : MonoBehaviour
{
    private struct HitstopSession
    {
        public float StartUnscaledTime;
        public float BaseDuration;
        public float EndUnscaledTime;
    }

    // Per-receiver hitstop session info (unscaled seconds).
    private readonly Dictionary<HitstopReceiver, HitstopSession> sessions = new Dictionary<HitstopReceiver, HitstopSession>();
    private readonly List<HitstopReceiver> toUnfreeze = new List<HitstopReceiver>();
    private Coroutine tickRoutine;

    private void OnEnable()
    {
        CombatEvents.OnAttackPerformed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        CombatEvents.OnAttackPerformed -= OnAttackPerformed;
    }

    private void OnAttackPerformed(AttackEventArgs args)
    {
        if ((args.Trigger & AttackEffectTrigger.Hit) == 0) return;

        var data = args.EffectData;
        if (data == null || !data.EnableHitstop) return;

        float attackerDur = Mathf.Max(0f, data.HitstopAttackerDuration);
        float victimDur = Mathf.Max(0f, data.HitstopVictimDuration);
        if (attackerDur <= 0f && victimDur <= 0f) return;

        if (args.Attacker != null && attackerDur > 0f)
        {
            RequestHitstop(args.Attacker, attackerDur);
        }
        if (args.Target != null && victimDur > 0f)
        {
            RequestHitstop(args.Target, victimDur);
        }

        if (tickRoutine == null && sessions.Count > 0)
        {
            tickRoutine = StartCoroutine(TickHitstop());
        }
    }

    private void RequestHitstop(GameObject root, float duration)
    {
        if (root == null) return;

        // Prefer receiver on the same root; fall back to parent to be more robust to different hierarchies.
        HitstopReceiver receiver = root.GetComponent<HitstopReceiver>();
        if (receiver == null) receiver = root.GetComponentInParent<HitstopReceiver>();
        if (receiver == null) return;

        float now = Time.unscaledTime;
        float requestEnd = now + duration;

        if (!sessions.TryGetValue(receiver, out var session) || session.EndUnscaledTime <= now)
        {
            // Cooldown is only checked when starting a NEW hitstop session for this receiver.
            if (!receiver.CanStartHitstop(now))
            {
                Debug.Log($"[Hitstop] Cooldown active for '{receiver.name}', skipping new hitstop session.");
                return;
            }

            receiver.Freeze();
            receiver.NotifyHitstopStarted(now);

            session = new HitstopSession
            {
                StartUnscaledTime = now,
                BaseDuration = duration,
                EndUnscaledTime = requestEnd
            };
            sessions[receiver] = session;
            return;
        }

        // Overlap extension with cap: cap is based on the FIRST trigger in this session.
        float capEnd = session.StartUnscaledTime + (2f * session.BaseDuration);
        float extended = Mathf.Max(session.EndUnscaledTime, requestEnd);
        float newEnd = Mathf.Min(extended, capEnd);

        session.EndUnscaledTime = newEnd;
        sessions[receiver] = session;

        if (newEnd >= capEnd - 0.0001f)
        {
            Debug.Log($"[Hitstop] Extension capped for '{receiver.name}'. base={session.BaseDuration:F3}s cap={2f * session.BaseDuration:F3}s");
        }
    }

    private IEnumerator TickHitstop()
    {
        while (sessions.Count > 0)
        {
            float now = Time.unscaledTime;

            toUnfreeze.Clear();
            foreach (var kv in sessions)
            {
                if (kv.Key == null || now >= kv.Value.EndUnscaledTime)
                {
                    toUnfreeze.Add(kv.Key);
                }
            }

            for (int i = 0; i < toUnfreeze.Count; i++)
            {
                var r = toUnfreeze[i];
                if (r != null)
                {
                    r.Unfreeze();
                }
                sessions.Remove(r);
            }

            yield return null;
        }

        tickRoutine = null;
    }
}


