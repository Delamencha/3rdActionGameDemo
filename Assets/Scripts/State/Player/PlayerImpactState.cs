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
            stateMachine.Health.SetInvulnerable(true);
        }
        else
        {
            stateMachine.Animator.CrossFadeInFixedTime(ImpactHash, CrossFadeDuration);
        }

        
    }

    public override void Tick(float deltaTime)
    {

        Move(deltaTime);

        duration -= deltaTime;
        if(duration <= 0)
        {
            stateMachine.Health.SetInvulnerable(false);
            ReturnToLocomotion();
        }

    }

    public override void Exit()
    {
        
    }


}
