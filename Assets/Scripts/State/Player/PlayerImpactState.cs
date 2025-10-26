using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerImpactState : PlayerBaseState
{
    private readonly int ImpactHash = Animator.StringToHash("Impact");

    private const float CrossFadeDuration = 0.1f;

    private float duration = 1f;

    private bool isLargeImpact;

    public PlayerImpactState(PlayerStateMachine stateMachine, bool isLargeImpact) : base(stateMachine)
    {
        this.isLargeImpact = isLargeImpact;
    }

    public override void Enter()
    {
        if (isLargeImpact)
        {
            stateMachine.Animator.CrossFadeInFixedTime("LargeImpact", CrossFadeDuration);
            duration = 4.2f;

            stateMachine.Health.ActiveInvulnerable();
        }
        else
        {
            stateMachine.Animator.CrossFadeInFixedTime(ImpactHash, CrossFadeDuration);
        }

        //stateMachine.ActivateInputBuffer();
        
    }

    public override void Tick(float deltaTime)
    {

        Move(deltaTime);

        // 尝试在窗口内应用预输入，命中则直接切换并返回
        //在特定时间后开始检测预输入，需考量使用time还是normalizedTime
        //if (stateMachine.ApplyBufferedInput()) return;

        duration -= deltaTime;
        if(duration <= 0)
        {
            stateMachine.Health.DeactiveInvulnerable();
            ReturnToLocomotion();
        }

    }

    public override void Exit()
    {
        stateMachine.DeactivateInputBuffer();
    }


}
