using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetJumpState : PlayerBaseState
{

    private Vector2 jumpDirectionInput;
    private readonly int TargetJumpHash = Animator.StringToHash("TargetJumpBlendTree");

    private readonly int TargetJumpRightBlendHash = Animator.StringToHash("TargetJumpRightSpeed");
    private readonly int TargetJumpForwardBlendHash = Animator.StringToHash("TargetJumpForwardSpeed");

    private const float CrossFadeDuration = 0.1f;

    private Vector3 momentum;

    public PlayerTargetJumpState(PlayerStateMachine stateMachine, Vector2 jumpDirectionInput) : base(stateMachine){

        this.jumpDirectionInput = jumpDirectionInput;

    }

    public override void Enter()
    {
        stateMachine.ForceReceiver.Jump(stateMachine.JumpForce);

        momentum = stateMachine.Controller.velocity;
        momentum.y = 0;

        stateMachine.Animator.SetFloat(TargetJumpForwardBlendHash, jumpDirectionInput.y);
        stateMachine.Animator.SetFloat(TargetJumpRightBlendHash, jumpDirectionInput.x);
        stateMachine.Animator.CrossFadeInFixedTime(TargetJumpHash, CrossFadeDuration);

    }

    public override void Tick(float deltaTime)
    {
        Move(momentum, deltaTime);

        if (stateMachine.Controller.velocity.y <= 0)
        {
            stateMachine.SwitchState(new PlayerFallState(stateMachine));
            return;
        }

        FaceTarget();
    }

    public override void Exit()
    {
        
    }


}
