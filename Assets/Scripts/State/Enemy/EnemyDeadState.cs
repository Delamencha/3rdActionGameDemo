using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{
    private readonly int DeathHash = Animator.StringToHash("Death");
    private const float CrossFadeDuration = 0.1f;

    public bool IsFinished { get; private set; }

    public EnemyDeadState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        //stateMachine.Ragdoll.ToggleRagdoll(true);
        IsFinished = false;

        // Stop navigation/motion
        if (stateMachine.Agent != null)
        {
            if (stateMachine.Agent.isOnNavMesh)
            {
                stateMachine.Agent.ResetPath();
            }
            stateMachine.Agent.velocity = Vector3.zero;
        }

        // Ensure we don't keep previous state's root motion settings (attack/dodge may enable it)
        stateMachine.Animator.applyRootMotion = false;
        stateMachine.ClearRootMotionTuning();

        // Play death animation (keep consistent with PlayerDeadState)
        stateMachine.Animator.CrossFadeInFixedTime(DeathHash, CrossFadeDuration);

        stateMachine.WeaponDamage.gameObject.SetActive(false);
        //��������Ҫ��target���Ƴ�
        GameObject.Destroy(stateMachine.Target);
    }

    public override void Tick(float deltaTime)
    {
        // Mark finished when death animation completes (requires the death state to be tagged "Death")
        // if (!IsFinished && GetNormalizedTime(stateMachine.Animator, "Death") >= 1f)
        // {
        //     IsFinished = true;
        // }
    }

    public override void Exit()
    {
        
    }

}
