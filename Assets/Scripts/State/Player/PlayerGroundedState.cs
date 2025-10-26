using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{

    private readonly int GroundedHash = Animator.StringToHash("Grounded");
    private const float CrossFadeDuration = 0.1f;

    public PlayerGroundedState(PlayerStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {
        
        stateMachine.Animator.CrossFadeInFixedTime(GroundedHash, CrossFadeDuration);
        stateMachine.InputReader.DogeEvent += OnDodge;
    }

    public override void Tick(float deltaTime)
    {
        if (GetNormalizedTime(stateMachine.Animator, "Grounded") >= 1f)
        {
            ReturnToLocomotion(true);
        }
    }

    public override void Exit()
    {
        stateMachine.InputReader.DogeEvent -= OnDodge;
    }

    private void OnDodge()
    {
        if (stateMachine.IsStateTransitionAllowed("PlayerDodgeState"))
        {
            stateMachine.SwitchState(new PlayerDodgeState(stateMachine,
                stateMachine.InputReader.MovementValue == Vector2.zero ? new Vector2(0, -1) : stateMachine.InputReader.MovementValue));
        }
    }
}
