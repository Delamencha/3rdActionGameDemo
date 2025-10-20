using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFreeLookState : PlayerBaseState
{
    private readonly int FreeLookSpeedHash = Animator.StringToHash("FreeLookSpeed") ;
    private readonly int FreeLookHash = Animator.StringToHash("FreeLookBlendTree");

    private const float AnimatorDampTime = 0.1f;

    private const float CrossFadeDuration = 0.1f;

    private bool shouldFade;

    private float unGroundedTimer = 0;

    public PlayerFreeLookState(PlayerStateMachine stateMachine,bool shouldFade = true) : base(stateMachine)
    {
        this.shouldFade = shouldFade;
    }

    public override void Enter()
    {

       // Debug.Log("Enter");
        stateMachine.InputReader.TargetEvent += OnTargeting;
        stateMachine.InputReader.JumpEvent += OnJump;
        stateMachine.InputReader.RunEvent += OnRun;
        stateMachine.InputReader.SkillEvent += OnSkill;
        stateMachine.InputReader.DogeEvent += OnDodge;

        stateMachine.Animator.SetFloat(FreeLookSpeedHash, 0);
        //stateMachine.Animator.Play(FreeLookHash);
        if (shouldFade)
        {
            stateMachine.Animator.CrossFadeInFixedTime(FreeLookHash, CrossFadeDuration);
        }
        else
        {
            stateMachine.Animator.Play(FreeLookHash);
        }
        

    }


    public override void Tick(float deltaTime)
    {
        if (stateMachine.InputReader.IsBlocking)
        {
            stateMachine.SwitchState(new PlayerBlockState(stateMachine));
            return;
        }

        if (stateMachine.InputReader.IsAttacking)
        {
            stateMachine.SwitchState(new PlayerAttackState(stateMachine, 0));
            return;
        }

        if (!stateMachine.Controller.isGrounded)
        {
            unGroundedTimer += deltaTime;
            if(unGroundedTimer > 0.5f)
            {
                stateMachine.SwitchState(new PlayerFallState(stateMachine));
            }
        }
        else
        {
            unGroundedTimer = 0;
        }

        //Debug.Log(stateMachine.InputReader.MovementValue);

        Vector3 movement = calculateMovement();

        //直接使用translate移动，忽视collider
        //movement.x = stateMachine.InputReader.MovementValue.x;
        //movement.y = 0;
        //movement.z = stateMachine.InputReader.MovementValue.y;
        //stateMachine.transform.Translate(movement * deltaTime);

        //使用characterController 移动，但未考虑重力
        //stateMachine.Controller.Move(movement * stateMachine.FreeLookMoveSpeed * deltaTime);

        //从playerBaseState调用：不同state均需要考虑重力，避免重力在切换状态时重新计算
        Move(movement * stateMachine.FreeLookMoveSpeed, deltaTime);


        if (stateMachine.InputReader.MovementValue == Vector2.zero)
        {
            stateMachine.Animator.SetFloat(FreeLookSpeedHash, 0, AnimatorDampTime, deltaTime);
            return;
        }

        stateMachine.Animator.SetFloat(FreeLookSpeedHash, 1f, AnimatorDampTime, deltaTime);

        FaceMovemnetDirection(movement, deltaTime);

    }


    public override void Exit()
    {
        //Debug.Log("Exit");

        stateMachine.InputReader.JumpEvent -= OnJump;
        stateMachine.InputReader.TargetEvent -= OnTargeting;
        stateMachine.InputReader.RunEvent -= OnRun;
        stateMachine.InputReader.SkillEvent -= OnSkill;
        stateMachine.InputReader.DogeEvent -= OnDodge;
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

    //private void OnJump()
    //{
    //    stateMachine.SwitchState(new PlayerTestState(stateMachine));
    //}
    private void OnTargeting()
    {
        if (stateMachine.Targeter.SelectTarget())
        {
            stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
        }
        
    }

    private void OnJump()
    {
        stateMachine.SwitchState(new PlayerJumpState(stateMachine));
    }


    private void OnRun()
    {
        stateMachine.SwitchState(new PlayerFreeRunState(stateMachine));
    }

    private void OnSkill()
    {
        stateMachine.SwitchState(new PlayerSkillState(stateMachine));
    }

    //非锁定状态Dodge :  有方向输入时，快速完成转向然后Forward dodge; 无方向输入时，back dodge
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
