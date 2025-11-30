using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChasingState : EnemyBaseState
{
    private readonly int IdleBlendHash = Animator.StringToHash("Locomotion");
    private readonly int SpeedHash = Animator.StringToHash("Speed");

    private const float AnimatorDampTime = 0.1f;
    private const float CrossFadeDuration = 0.1f;
    private const float TurnSpeedDeg = 720f;

    public EnemyChasingState(EnemyStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(IdleBlendHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {

        if (!isInChaseRange())
        {
            stateMachine.SwitchState(new EnemyIdleState(stateMachine));
            return;
        }else if (IsInAttackRange())
        {
            stateMachine.SwitchState(new EnemyAttackingState(stateMachine));
            return;
        }

        MoveToPlayer(deltaTime);
        FacePlayer(TurnSpeedDeg, deltaTime);

        stateMachine.Animator.SetFloat(SpeedHash, 1f, AnimatorDampTime, deltaTime);

    }

    public override void Exit()
    {
        stateMachine.Agent.ResetPath();
        stateMachine.Agent.velocity = Vector3.zero;
    }

    private void MoveToPlayer(float deltaTime)
    {

        if (stateMachine.Agent.isOnNavMesh)
        {
            stateMachine.Agent.destination = stateMachine.Player.transform.position;

            Move(stateMachine.Agent.desiredVelocity.normalized * stateMachine.ChasingSpeed, deltaTime);
        }

        stateMachine.Agent.velocity = stateMachine.Controller.velocity;

    }

    private bool IsInAttackRange()
    {
        if (stateMachine.Player.IsDead)
        {
            return false;
        }

        float playerDistanceSqr = (stateMachine.Player.transform.position - stateMachine.transform.position).sqrMagnitude;


        return playerDistanceSqr <= Mathf.Pow(stateMachine.AttackRange, 2);
    }

}
