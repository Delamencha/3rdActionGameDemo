using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private readonly int JumpHash = Animator.StringToHash("Jump");
    private const float CrossFadeDuration = 0.1f;

    private Vector3 momentum;

    public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }


    public override void Enter()
    {
        stateMachine.ForceReceiver.Jump(stateMachine.JumpForce);

        //保存水平位移，实现跨跳
        momentum = stateMachine.Controller.velocity;
        momentum.y = 0;

        stateMachine.Animator.CrossFadeInFixedTime(JumpHash, CrossFadeDuration);

        if(stateMachine.LedgeDetector != null)
        {
            stateMachine.LedgeDetector.OnLedgeDetect += HandleLedgeDetect;
        }
        
    }

    public override void Tick(float deltaTime)
    {
        Move(momentum, deltaTime);

        if(stateMachine.Controller.velocity.y <= 0)
        {
            stateMachine.SwitchState(new PlayerFallState(stateMachine));
            return;
        }

        FaceTarget();
    }

    public override void Exit()
    {   
        if(stateMachine.LedgeDetector != null)
        {
            stateMachine.LedgeDetector.OnLedgeDetect -= HandleLedgeDetect;
        }
        
    }

    private void HandleLedgeDetect( Vector3 ledgeForward, Vector3 closestPoint)
    {
        stateMachine.SwitchState(new PlayerHangingState(stateMachine, ledgeForward, closestPoint));
    }

}
