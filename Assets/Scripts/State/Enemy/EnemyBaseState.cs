using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBaseState : State
{
    protected EnemyStateMachine stateMachine;

    public EnemyBaseState(EnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    protected bool isInChaseRange()
    {
        if (stateMachine.Player.IsDead)
        {
            return false;
        }


        float playerDistanceSqr = (stateMachine.Player.transform.position - stateMachine.transform.position).sqrMagnitude;


        return playerDistanceSqr <= Mathf.Pow(stateMachine.PlayerChasingRange, 2)  ;
    }

    protected void Move(float deltaTime)
    {
        Vector3 movement = stateMachine.ForceReceiver.Movement;

        stateMachine.Controller.Move(movement * deltaTime);
    }

    protected void Move(Vector3 motion, float deltaTime)
    {
        Vector3 movement = motion;

        movement += stateMachine.ForceReceiver.Movement;

        stateMachine.Controller.Move(movement * deltaTime);

    }

    protected void FacePlayer()
    {
        if (stateMachine.Player == null) return;

        Vector3 lookDirection = stateMachine.Player.transform.position - stateMachine.transform.position;
        lookDirection.y = 0;

        stateMachine.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    protected void FacePlayer(float turnSpeedDegPerSec, float deltaTime)
    {
        if (stateMachine.Player == null) return;

        Vector3 to = stateMachine.Player.transform.position - stateMachine.transform.position;
        to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(to.normalized, Vector3.up);
        float step = Mathf.Max(0f, turnSpeedDegPerSec) * deltaTime;
        stateMachine.transform.rotation = Quaternion.RotateTowards(stateMachine.transform.rotation, targetRotation, step);
    }

}
