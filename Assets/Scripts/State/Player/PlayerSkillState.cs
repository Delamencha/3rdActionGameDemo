using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillState : PlayerBaseState
{
    private readonly int SkillHash = Animator.StringToHash("Skill");

    private const float CrossFadeDuration = 0.1f;

    public PlayerSkillState(PlayerStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(SkillHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {
        if (GetNormalizedTime(stateMachine.Animator, "Skill") >= 1f)
        {
            ReturnToLocomotion();
        }
    }

    public override void Exit()
    {
        
    }


}
