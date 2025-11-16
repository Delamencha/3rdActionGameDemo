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

    protected void Move(float deltaTime)
    {
        Vector3 movement = stateMachine.ForceReceiver.Movement;

        stateMachine.Controller.Move(movement * deltaTime);

    }

    protected void FaceTarget()
    {
        if (stateMachine.Targeter.CurrentTarget == null) return;

        Vector3 lookDirection = stateMachine.Targeter.CurrentTarget.transform.position - stateMachine.transform.position;
        lookDirection.y = 0;

        stateMachine.transform.rotation = Quaternion.LookRotation(lookDirection);

    }

	protected void TryFaceTarget(float degree, float deltaTime)
	{
		if (stateMachine.Targeter.CurrentTarget == null) return;
		if (degree < 0.0001f || stateMachine.FaceTargetTurnSpeed <= 0 ) return;

		float allowedDelta = Mathf.Clamp(degree, 0f, 180f);

		Vector3 toTarget = stateMachine.Targeter.CurrentTarget.transform.position - stateMachine.transform.position;
		toTarget.y = 0f;
		if (toTarget.sqrMagnitude < 0.0001f) return;

		Quaternion targetRotation = Quaternion.LookRotation(toTarget);

        float speedDegPerSec = stateMachine.FaceTargetTurnSpeed;
		float maxStepThisFrame = speedDegPerSec * deltaTime;
		float step = Mathf.Min(allowedDelta, maxStepThisFrame);

		stateMachine.transform.rotation = Quaternion.RotateTowards(stateMachine.transform.rotation, targetRotation, step);
	   
	}

    protected void ReturnToLocomotion()
    {
        if(stateMachine.Targeter.CurrentTarget != null)
        {
            stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
        }
        else
        {
            stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
        }
    }

    protected void ReturnToLocomotion(bool shouldReset)
    {
        if (stateMachine.Targeter.CurrentTarget != null)
        {
            stateMachine.SwitchState(new PlayerTargetingState(stateMachine, shouldReset));
        }
        else
        {
            stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
        }
    }


}
