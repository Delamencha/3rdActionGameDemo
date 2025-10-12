using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFreeRunState : PlayerBaseState
{
    private readonly int FreeRunSpeedHash = Animator.StringToHash("FreeRunSpeed");
    private readonly int FreeRunHash = Animator.StringToHash("FreeRunBlendTree");

    private const float AnimatorDampTime = 0.1f;

    private const float CrossFadeDuration = 0.1f;

    private float unGroundedTimer = 0;

    public PlayerFreeRunState(PlayerStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {
        stateMachine.InputReader.TargetEvent += OnTargeting;
        stateMachine.InputReader.JumpEvent += OnJump;
        stateMachine.InputReader.RunEvent += OnRun;

        stateMachine.Animator.SetFloat(FreeRunSpeedHash, 1);
        stateMachine.Animator.CrossFadeInFixedTime(FreeRunHash, CrossFadeDuration);

    }



    public override void Tick(float deltaTime)
    {
        if (stateMachine.InputReader.IsAttacking)
        {
            stateMachine.SwitchState(new PlayerAttackState(stateMachine, 0));
            return;
        }

        if (!stateMachine.Controller.isGrounded)
        {
            unGroundedTimer += deltaTime;
            if (unGroundedTimer > 0.5f)
            {
                stateMachine.SwitchState(new PlayerFallState(stateMachine));
            }
        }
        else
        {
            unGroundedTimer = 0;
        }

        Vector3 movement = calculateMovement();
        Move(movement * stateMachine.FreeRunSpeed, deltaTime);

        if (stateMachine.InputReader.MovementValue == Vector2.zero)
        {
            stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
            return;
        }

        stateMachine.Animator.SetFloat(FreeRunSpeedHash, 1f, AnimatorDampTime, deltaTime);

        FaceMovemnetDirection(movement, deltaTime);
    }

    public override void Exit()
    {
        stateMachine.InputReader.TargetEvent -= OnTargeting;
        stateMachine.InputReader.JumpEvent -= OnJump;
        stateMachine.InputReader.RunEvent -= OnRun;
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

    private void OnRun()
    {
        stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
    }

    private void OnJump()
    {
        stateMachine.SwitchState(new PlayerJumpState(stateMachine));
    }

    private void OnTargeting()
    {
        if (stateMachine.Targeter.SelectTarget())
        {
            stateMachine.SwitchState(new PlayerTargetRunState(stateMachine));
        }
            
    }

}
