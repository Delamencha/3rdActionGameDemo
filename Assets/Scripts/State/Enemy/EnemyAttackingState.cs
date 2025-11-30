using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackingState : EnemyBaseState
{

    private readonly int AttackHash = Animator.StringToHash("Attack");

    private const float CrossFadeDuration = 0.1f;
    private const float TurnSpeedDeg = 360f;

    public EnemyAttackingState(EnemyStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {
        
        stateMachine.WeaponDamage.SetAttack(stateMachine.AttackDamage, stateMachine.AttackKnockBack, KnockbackType.AwayFromAttacker);

        stateMachine.Animator.CrossFadeInFixedTime(AttackHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {
        if(GetNormalizedTime(stateMachine.Animator, "Attack") >= 1f)
        {
            stateMachine.SwitchState(new EnemyChasingState(stateMachine));
        }

        FacePlayer(TurnSpeedDeg, deltaTime);
    }

    public override void Exit()
    {

    }

}
