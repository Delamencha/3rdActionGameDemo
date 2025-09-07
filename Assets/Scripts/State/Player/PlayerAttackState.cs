using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{

    private Attack currentAttack;

    public PlayerAttackState(PlayerStateMachine stateMachine, int attackIndex) : base(stateMachine)
    {
        currentAttack = stateMachine.Attacks[attackIndex];
    }

    public override void Enter()
    {
        // Debug.Log(currentAttack.animationName);
        stateMachine.Animator.CrossFadeInFixedTime(currentAttack.AnimationName, 0.1f);
    }

    public override void Tick(float deltaTime)
    {
        
    }

    public override void Exit()
    {
        
    }


}
