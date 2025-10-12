using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    private readonly int FallHash = Animator.StringToHash("Fall");
    private readonly int GroundedHash = Animator.StringToHash("Grounded");
    private const float CrossFadeDuration = 0.1f;

    private Vector3 momentum;

    private bool hasLand;

    public PlayerFallState(PlayerStateMachine stateMachine) : base(stateMachine) { }


    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(FallHash, CrossFadeDuration);
        momentum = stateMachine.Controller.velocity;
        momentum.y = 0;

        hasLand = false;

        if (stateMachine.LedgeDetector != null)
        {
            stateMachine.LedgeDetector.OnLedgeDetect += HandleLedgeDetect;
        }
        

    }

    public override void Tick(float deltaTime)
    {
        

        if (stateMachine.Controller.isGrounded)
        {
            //ReturnToLocomotion();
            if (!hasLand)
            {

                stateMachine.Animator.CrossFadeInFixedTime(GroundedHash, CrossFadeDuration);
                hasLand = true;
            }
            else
            {
                if(GetNormalizedTime(stateMachine.Animator, "Grounded") >= 1f)
                {
                    ReturnToLocomotion();
                }
            }
        }
        else
        {
            Move(momentum, deltaTime);
            FaceTarget();
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
