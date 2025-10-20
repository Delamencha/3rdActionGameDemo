using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlockState : PlayerBaseState
{
    private readonly int BlockBlendHash = Animator.StringToHash("BlockBlendTree");
    private readonly int FreeLookSpeedHash = Animator.StringToHash("FreeLookSpeed");

    private const float AnimatorDampTime = 0.1f;

    private const float CrossFadeDuration = 0.1f;

    private float unGroundedTimer = 0;

    public PlayerBlockState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.InputReader.TargetEvent += OnTargeting;
        stateMachine.InputReader.JumpEvent += OnJump;

        stateMachine.InputReader.DogeEvent += OnDodge;

        stateMachine.Animator.SetFloat(FreeLookSpeedHash, 0);

        stateMachine.Animator.CrossFadeInFixedTime(BlockBlendHash, CrossFadeDuration);
        stateMachine.Health.SetInvulnerable(true);
    }

    public override void Tick(float deltaTime)
    {
        //Move(deltaTime);

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


        if (!stateMachine.InputReader.IsBlocking)
        {
            ReturnToLocomotion();
        }

        Vector3 movement = calculateMovement();

        Move(movement * stateMachine.BlockWalkSpeed, deltaTime);

        if (stateMachine.InputReader.MovementValue == Vector2.zero)
        {
            stateMachine.Animator.SetFloat(FreeLookSpeedHash, 0, AnimatorDampTime, deltaTime);
            return;
        }

        stateMachine.Animator.SetFloat(FreeLookSpeedHash, 1f, AnimatorDampTime, deltaTime);

        FaceMovemnetDirection(movement, deltaTime);

    }

    private Vector3 calculateMovement()
    {

        Vector3 xWeight = stateMachine.MainCameraTransform.right;
        xWeight.y = 0;
        xWeight.Normalize();
        xWeight *= stateMachine.InputReader.MovementValue.x;

        Vector3 zWeight = stateMachine.MainCameraTransform.forward;
        zWeight.y = 0;
        zWeight.Normalize();
        zWeight *= stateMachine.InputReader.MovementValue.y;

        return xWeight + zWeight;
    }

    private void FaceMovemnetDirection(Vector3 movement, float deltaTime)
    {
        stateMachine.transform.rotation = Quaternion.Lerp(
            stateMachine.transform.rotation,
            Quaternion.LookRotation(movement),
            deltaTime * stateMachine.RotationDamping
            );

    }

    public override void Exit()
    {

        stateMachine.InputReader.JumpEvent -= OnJump;
        stateMachine.InputReader.TargetEvent -= OnTargeting;

        stateMachine.Health.SetInvulnerable(false);

        stateMachine.InputReader.DogeEvent -= OnDodge;
    }

    private void OnJump()
    {
        stateMachine.SwitchState(new PlayerJumpState(stateMachine));
    }

    private void OnTargeting()
    {
        if (stateMachine.Targeter.SelectTarget())
        {
            stateMachine.SwitchState(new PlayerTargetBlockState(stateMachine));
        }
    }

    private void OnDodge()
    {

        if (stateMachine.InputReader.MovementValue == Vector2.zero)
        {
            stateMachine.SwitchState(new PlayerDodgeState(stateMachine, new Vector2(0, -1)));
        }
        else
        {
            Vector3 movement = calculateMovement();

            stateMachine.transform.rotation = Quaternion.LookRotation(movement);
            stateMachine.SwitchState(new PlayerDodgeState(stateMachine, new Vector2(0, 1)));
        }

    }
}
