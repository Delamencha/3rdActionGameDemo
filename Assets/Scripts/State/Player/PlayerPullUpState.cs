using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPullUpState : PlayerBaseState
{
    private readonly int ClimbHash = Animator.StringToHash("Climb");

    private readonly Vector3 offset = new Vector3(0, 2.325f, 0.65f);

    private const float CrossFadeDuration = 0.1f;

    public PlayerPullUpState(PlayerStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {


        stateMachine.Animator.CrossFadeInFixedTime(ClimbHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {
        //if (stateMachine.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) return;
        if (GetNormalizedTime(stateMachine.Animator, "Climbing") < 1f) return;


        stateMachine.Controller.enabled = false;
        stateMachine.transform.Translate(offset, Space.Self);
        stateMachine.Controller.enabled = true;

        stateMachine.SwitchState(new PlayerFreeLookState(stateMachine, false));
    }

    public override void Exit()
    {
        stateMachine.Controller.Move(Vector3.zero);
        stateMachine.ForceReceiver.ResetForce();
    }


}
