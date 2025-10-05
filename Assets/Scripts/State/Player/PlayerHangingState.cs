using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHangingState : PlayerBaseState
{
    private readonly int HangingHash = Animator.StringToHash("Hanging");

    private const float CrossFadeDuration = 0.1f;

    private Vector3 closestPoint;
    private Vector3 ledgeForward;

    public PlayerHangingState(PlayerStateMachine stateMachine, Vector3 ledgeForward, Vector3 closestPoint) : base(stateMachine)
    {

        this.closestPoint = closestPoint;
        this.ledgeForward = ledgeForward;

    }

    public override void Enter()
    {

        stateMachine.transform.rotation = Quaternion.LookRotation(ledgeForward, Vector3.up);

        stateMachine.Controller.enabled = false;
        stateMachine.transform.position += closestPoint - stateMachine.LedgeDetector.transform.position;
        stateMachine.Controller.enabled = true;

        stateMachine.Animator.CrossFadeInFixedTime(HangingHash, CrossFadeDuration);

    }

    public override void Tick(float deltaTime)
    {
        if(stateMachine.InputReader.MovementValue.y <= -0.5f)
        {
            //清除起跳时的水平向速度
            stateMachine.Controller.Move(Vector3.zero);
            //退出悬挂状态时清空加速度的累积
            stateMachine.ForceReceiver.ResetForce();

            stateMachine.SwitchState(new PlayerFallState(stateMachine));
        }else if (stateMachine.InputReader.MovementValue.y >= 0.5f)
        {
            //switch到climb状态仍需播放动画，这之间的时间可能会产生力或速度，故在离开climb状态时Reset

            stateMachine.SwitchState(new PlayerPullUpState(stateMachine));
        }
    }

    public override void Exit()
    {
        
    }

}
