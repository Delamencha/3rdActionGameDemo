using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetBlockState : PlayerBaseState
{
    private readonly int TargetBlockHash = Animator.StringToHash("TargetBlockBlendTree");

    private readonly int TargetingRightBlendHash = Animator.StringToHash("TargetingRightSpeed");
    private readonly int TargetingForwardBlendHash = Animator.StringToHash("TargetingForwardSpeed");

    private const float AnimatorDampTime = 0.1f;

    private const float CrossFadeDuration = 0.1f;

    private float unGroundedTimer = 0;

    public PlayerTargetBlockState(PlayerStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {

        stateMachine.InputReader.TargetEvent += OnTarget;
        stateMachine.InputReader.DogeEvent += OnDoge;
        stateMachine.InputReader.JumpEvent += OnJump;

        stateMachine.Animator.CrossFadeInFixedTime(TargetBlockHash, CrossFadeDuration);
        stateMachine.Health.SetInvulnerable(true);
    }



    public override void Tick(float deltaTime)
    {

        if (!stateMachine.InputReader.IsBlocking)
        {
            ReturnToLocomotion();
        }

        if (stateMachine.Targeter.CurrentTarget == null)
        {
            stateMachine.SwitchState(new PlayerBlockState(stateMachine));
            return;
        }

        if (!stateMachine.Controller.isGrounded)
        {
            unGroundedTimer += deltaTime;
            if (unGroundedTimer > 0.5f)
            {
                stateMachine.SwitchState(new PlayerFallState(stateMachine));
                return;
            }
        }
        else
        {
            unGroundedTimer = 0;
        }

        Move(CalculateMovement(deltaTime) * stateMachine.BlockWalkSpeed, deltaTime);

        FaceTarget();

        UpdateAnimator(deltaTime);

    }

    public override void Exit()
    {
        stateMachine.InputReader.TargetEvent -= OnTarget;
        stateMachine.InputReader.DogeEvent -= OnDoge;
        stateMachine.InputReader.JumpEvent -= OnJump;

        stateMachine.Health.SetInvulnerable(false);
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
        stateMachine.SwitchState(new PlayerBlockState(stateMachine));
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
