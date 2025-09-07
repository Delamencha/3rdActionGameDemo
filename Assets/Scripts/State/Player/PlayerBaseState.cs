using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState : State
{
    protected PlayerStateMachine stateMachine;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    protected void Move(Vector3 motion, float deltaTime)
    {
        Vector3 movement = motion;

        movement += stateMachine.ForceReceiver.Movement;

        stateMachine.Controller.Move(movement * deltaTime);

    }

    protected void FaceTarget()
    {
        if (stateMachine.Targeter.CurrentTarget == null) return;

        Vector3 lookDirection = stateMachine.Targeter.CurrentTarget.transform.position - stateMachine.transform.position;
        lookDirection.y = 0;

        stateMachine.transform.rotation = Quaternion.LookRotation(lookDirection);

    }


}
