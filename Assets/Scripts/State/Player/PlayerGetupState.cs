using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGetupState : PlayerBaseState
{
    private readonly int GetUpHash = Animator.StringToHash("GetUp");

    private const float CrossFadeDuration = 0.1f;


    public PlayerGetupState(PlayerStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(GetUpHash, CrossFadeDuration);
    }
    public override void Tick(float deltaTime)
    {

        float normalizedTime = GetNormalizedTime(stateMachine.Animator, "GetUp");

        if (normalizedTime > 1f)
        {
            ReturnToLocomotion();
        }
    }
    public override void Exit()
    {
        
    }


}
