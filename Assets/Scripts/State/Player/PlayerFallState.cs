using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    private readonly int FallHash = Animator.StringToHash("Fall");

    private const float CrossFadeDuration = 0.1f;

    private Vector3 momentum;


    public PlayerFallState(PlayerStateMachine stateMachine) : base(stateMachine) {}


    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(FallHash, CrossFadeDuration);
        momentum = stateMachine.Controller.velocity;
        momentum.y = 0;

        if (stateMachine.LedgeDetector != null)
        {
            stateMachine.LedgeDetector.OnLedgeDetect += HandleLedgeDetect;
        }
        

    }

    public override void Tick(float deltaTime)
    {

        Move(momentum, deltaTime);
        FaceTarget();

        if (stateMachine.Controller.isGrounded)
        {
            stateMachine.SwitchState(new PlayerGroundedState(stateMachine));
        }

        
    }

    public override void Exit()
    {   
        if(stateMachine.LedgeDetector != null)
        {
            stateMachine.LedgeDetector.OnLedgeDetect -= HandleLedgeDetect;
        }
        
    }

    private void HandleLedgeDetect( Vector3 ledgeForward, Vector3 colsestPoint)
    {
        stateMachine.SwitchState(new PlayerHangingState(stateMachine, ledgeForward, colsestPoint));
    }

}
