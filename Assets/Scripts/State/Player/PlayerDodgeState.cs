using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodgeState : PlayerBaseState
{
    private Vector2 dodgingDirectionInput;
    private float remainingDodgeTime;

    private readonly int DodgeHash = Animator.StringToHash("DodgeBlendTree");

    private readonly int DodgeRightBlendHash = Animator.StringToHash("DodgeRight");
    private readonly int DodgeForwardBlendHash = Animator.StringToHash("DodgeForward");

    private const float CrossFadeDuration = 0.1f;

    public PlayerDodgeState(PlayerStateMachine stateMachine, Vector2 dogeDirectionInput) : base(stateMachine) 
    {
        this.dodgingDirectionInput = dogeDirectionInput;
    }

    public override void Enter()
    {

        remainingDodgeTime = stateMachine.DodgeDuration;

        stateMachine.Animator.SetFloat(DodgeForwardBlendHash, dodgingDirectionInput.y);
        stateMachine.Animator.SetFloat(DodgeRightBlendHash, dodgingDirectionInput.x);
        stateMachine.Animator.CrossFadeInFixedTime(DodgeHash, CrossFadeDuration);

        //…¡±‹ÃÌº”Œﬁµ–÷°
        stateMachine.Health.SetInvulnerable(true);


    }

    public override void Tick(float deltaTime)
    {
        Vector3 movement = new Vector3();

        movement += stateMachine.transform.right * dodgingDirectionInput.x * stateMachine.DodgeDistance / stateMachine.DodgeDuration;
        movement += stateMachine.transform.forward * dodgingDirectionInput.y * stateMachine.DodgeDistance / stateMachine.DodgeDuration;

        Move(movement, deltaTime);
        FaceTarget();

        remainingDodgeTime -= deltaTime;

        if(remainingDodgeTime <= 0)
        {
            ReturnToLocomotion();
            //stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
        }

    }

    public override void Exit()
    {
        stateMachine.Health.SetInvulnerable(false);
    }


}
