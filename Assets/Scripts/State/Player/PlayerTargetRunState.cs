using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetRunState : PlayerBaseState
{

    private readonly int TargetRunHash = Animator.StringToHash("TargetRunBlendTree");

    private readonly int TargetingRightBlendHash = Animator.StringToHash("TargetingRightSpeed");
    private readonly int TargetingForwardBlendHash = Animator.StringToHash("TargetingForwardSpeed");

    private const float AnimatorDampTime = 0.1f;

    private const float CrossFadeDuration = 0.1f;

    public PlayerTargetRunState(PlayerStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {
        Debug.Log("Entering Target Run State");

        stateMachine.InputReader.TargetEvent += OnTarget;
        stateMachine.InputReader.DogeEvent += OnDoge;
        stateMachine.InputReader.JumpEvent += OnJump;

        stateMachine.InputReader.RunEvent += OnRun;

        stateMachine.Animator.CrossFadeInFixedTime(TargetRunHash, CrossFadeDuration);
    }



    public override void Tick(float deltaTime)
    {

        if (stateMachine.InputReader.IsAttacking)
        {
            stateMachine.SwitchState(new PlayerAttackState(stateMachine, 4));
            return;
        }

        if (stateMachine.Targeter.CurrentTarget == null)
        {
            stateMachine.SwitchState(new PlayerFreeRunState(stateMachine));
            return;
        }

        Move(CalculateMovement(deltaTime) * stateMachine.FreeRunSpeed, deltaTime);

        FaceTarget();

        UpdateAnimator(deltaTime);

    }

    public override void Exit()
    {
        Debug.Log("Exiting Target Run State");

        stateMachine.InputReader.TargetEvent -= OnTarget;
        stateMachine.InputReader.DogeEvent -= OnDoge;
        stateMachine.InputReader.JumpEvent -= OnJump;

        stateMachine.InputReader.RunEvent -= OnRun;
    }



    private Vector3 CalculateMovement(float deltaTime)
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
        if (stateMachine.InputReader.MovementValue.x == 0 && stateMachine.InputReader.MovementValue.y == 0)
        {
            stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
            return;
        }


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

    private void OnJump()
    {
        stateMachine.SwitchState(new PlayerJumpState(stateMachine));
    }

    private void OnDoge()
    {
        if (stateMachine.InputReader.MovementValue == Vector2.zero)
        {
            return;
        }

        stateMachine.SwitchState(new PlayerDodgeState(stateMachine, stateMachine.InputReader.MovementValue));
    }

    private void OnTarget()
    {
        stateMachine.Targeter.Cancel();
        stateMachine.SwitchState(new PlayerFreeRunState(stateMachine));
    }

    private void OnRun()
    {
        stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
    }

}
