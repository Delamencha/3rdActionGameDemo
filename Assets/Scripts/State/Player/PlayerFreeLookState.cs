using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFreeLookState : PlayerBaseState
{
    private readonly int FreeLookSpeedHash = Animator.StringToHash("FreeLookSpeed") ;
    private readonly int FreeLookHash = Animator.StringToHash("FreeLookBlendTree");

    private const float AnimatorDampTime = 0.1f;

    public PlayerFreeLookState(PlayerStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {
        Debug.Log("Enter");
        //stateMachine.InputReader.JumpEvent += OnJump;
        stateMachine.InputReader.TargetEvent += OnTargeting;

        stateMachine.Animator.Play(FreeLookHash);

    }

    public override void Tick(float deltaTime)
    {
        if (stateMachine.InputReader.IsAttacking)
        {
            stateMachine.SwitchState(new PlayerAttackState(stateMachine, 0));
            return;
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

        stateMachine.Animator.SetFloat(FreeLookSpeedHash, 1, AnimatorDampTime, deltaTime);

        FaceMovemnetDirection(movement, deltaTime);

    }


    public override void Exit()
    {
        Debug.Log("Exit");

        //stateMachine.InputReader.JumpEvent -= OnJump;
        stateMachine.InputReader.TargetEvent -= OnTargeting;
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


}
