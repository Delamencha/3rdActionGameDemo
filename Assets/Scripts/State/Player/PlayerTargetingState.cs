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

    private const float CrossFadeDuration = 0.1f;

    public PlayerTargetingState(PlayerStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {
        //Debug.Log("Entering Targeting State");
        stateMachine.InputReader.TargetEvent += OnTarget;
        stateMachine.InputReader.DogeEvent += OnDoge;
        stateMachine.InputReader.JumpEvent += OnJump;
        stateMachine.InputReader.SkillEvent += OnSkill;

        stateMachine.InputReader.RunEvent += OnRun;

        //stateMachine.Animator.Play(TargetingHash);
        stateMachine.Animator.CrossFadeInFixedTime(TargetingHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {
        if (stateMachine.InputReader.IsBlocking)
        {
            stateMachine.SwitchState(new PlayerTargetBlockState(stateMachine));;
            return;
        }

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

        Move(CalculateMovement(deltaTime) * stateMachine.TargetingMoveSpeed, deltaTime);

        //Face the target
        FaceTarget();

        UpdateAnimator(deltaTime);

    }

    public override void Exit()
    {
        //Debug.Log("Exiting Targeting State");
        stateMachine.InputReader.TargetEvent -= OnTarget;
        stateMachine.InputReader.DogeEvent -= OnDoge;
        stateMachine.InputReader.JumpEvent -= OnJump;
        stateMachine.InputReader.SkillEvent -= OnSkill;

        stateMachine.InputReader.RunEvent -= OnRun;

    }

    private void OnTarget()
    {
        stateMachine.Targeter.Cancel();
        stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
    }

    private void OnDoge()
    {
        if(stateMachine.InputReader.MovementValue == Vector2.zero)
        {
            return;
        }

        stateMachine.SwitchState(new PlayerDodgeState(stateMachine, stateMachine.InputReader.MovementValue));
        //在锁定状态下的闪避，应该是绕目标转圈或径向移动而非简单前后左右移动

    }

    private void OnJump()
    {
        stateMachine.SwitchState(new PlayerJumpState(stateMachine));
    }

    private void OnRun()
    {
        stateMachine.SwitchState(new PlayerTargetRunState(stateMachine));
    }

    private void OnSkill()
    {
        stateMachine.SwitchState(new PlayerSkillState(stateMachine));
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
