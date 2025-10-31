using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerImpactState : PlayerBaseState
{
    private readonly int LigthImpactHash = Animator.StringToHash("LightImpact");
    private readonly int MediumImpactHash = Animator.StringToHash("MediumImpact");
    private readonly int HeavyImpactHash = Animator.StringToHash("HeavyImpact");

    private const float CrossFadeDuration = 0.1f;

    private float duration = 1f;

    private ImpactType impactType;

    public PlayerImpactState(PlayerStateMachine stateMachine, ImpactType impactType) : base(stateMachine)
    {
        this.impactType = impactType;
    }

    public override void Enter()
    {
        if (impactType == ImpactType.Heavy)
        {
            stateMachine.Animator.CrossFadeInFixedTime(HeavyImpactHash, CrossFadeDuration);
            duration = 2.5f;

            stateMachine.Health.ActiveInvulnerable();
        }else if(impactType == ImpactType.Medium)
        {
            stateMachine.Animator.CrossFadeInFixedTime(MediumImpactHash, CrossFadeDuration);
            duration = 0.75f;
        }
        else if (impactType == ImpactType.Light)
        {
            stateMachine.Animator.CrossFadeInFixedTime(LigthImpactHash, CrossFadeDuration);
            duration = 0.65f;
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
            if (impactType == ImpactType.Heavy)
            {
                stateMachine.SwitchState(new PlayerGetupState(stateMachine));
            }
            else
            {
                stateMachine.Health.DeactiveInvulnerable();
                ReturnToLocomotion();
            }

        }

    }

    public override void Exit()
    {
        stateMachine.DeactivateInputBuffer();
    }


}
