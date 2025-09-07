using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetingState : PlayerBaseState
{
    
    private readonly int TargetingHash = Animator.StringToHash("TargetingBlendTree");

    private readonly int TargetingRightBlendHash = Animator.StringToHash("TargetingRightSpeed");
    private readonly int TargetingForwardBlendHash = Animator.StringToHash("TargetingForwardSpeed");

    private const float AnimatorDampTime = 0.1f;

    public PlayerTargetingState(PlayerStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {
        Debug.Log("Entering Targeting State");
        stateMachine.InputReader.CancelEvent += OnCancel; 

        stateMachine.Animator.Play(TargetingHash);
    }



    public override void Tick(float deltaTime)
    {
        if (stateMachine.InputReader.IsAttacking)
        {
            stateMachine.SwitchState(new PlayerAttackState(stateMachine, 0));
            return;
        }

        if (stateMachine.Targeter.CurrentTarget == null)
        {
            stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
            return;
        }

        Move(CalculateMovement() * stateMachine.TargetingMoveSpeed, deltaTime);

        //Face the target
        FaceTarget();

        UpdateAnimator(deltaTime);

    }

    public override void Exit()
    {
        Debug.Log("Exiting Targeting State");
        stateMachine.InputReader.CancelEvent -= OnCancel;

    }

    private void OnCancel()
    {
        stateMachine.Targeter.Cancel();
        stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
    }

    private Vector3 CalculateMovement()
    {

        Vector3 movement = new Vector3();

        movement += stateMachine.transform.right * stateMachine.InputReader.MovementValue.x;

        movement += stateMachine.transform.forward * stateMachine.InputReader.MovementValue.y;

        return movement;
    }

    private void UpdateAnimator(float deltaTime)
    {
        //if (stateMachine.InputReader.MovementValue == Vector2.zero)
        //{
        //    stateMachine.Animator.SetFloat(TargetingRightBlendHash, 0, AnimatorDampTime, deltaTime);
        //    stateMachine.Animator.SetFloat(TargetingForwardBlendHash, 0, AnimatorDampTime, deltaTime);
        //    return;
        //}

        if (stateMachine.InputReader.MovementValue.x == 0)
        {
            stateMachine.Animator.SetFloat(TargetingRightBlendHash, 0, AnimatorDampTime, deltaTime);
        }
        else 
        {
            float value = stateMachine.InputReader.MovementValue.x > 0 ? 1f : -1f; 
            stateMachine.Animator.SetFloat(TargetingRightBlendHash, value, AnimatorDampTime, deltaTime);
        }
        

        if (stateMachine.InputReader.MovementValue.y == 0)
        {
            stateMachine.Animator.SetFloat(TargetingForwardBlendHash, 0, AnimatorDampTime, deltaTime);
        }
        else 
        {
            float value = stateMachine.InputReader.MovementValue.y > 0 ? 1f : -1f;
            stateMachine.Animator.SetFloat(TargetingForwardBlendHash, value, AnimatorDampTime, deltaTime);
        }
       
    }

}
