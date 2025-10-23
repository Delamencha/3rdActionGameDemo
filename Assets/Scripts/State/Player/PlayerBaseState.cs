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

	protected void TryFaceTarget(float degree)
	{
		if (stateMachine.Targeter.CurrentTarget == null) return;

		float allowedDelta = Mathf.Clamp(degree, 0f, 180f);

		Vector3 toTarget = stateMachine.Targeter.CurrentTarget.transform.position - stateMachine.transform.position;
		toTarget.y = 0f;
		if (toTarget.sqrMagnitude < 0.0001f) return;

		Quaternion targetRotation = Quaternion.LookRotation(toTarget);
		float currentYaw = stateMachine.transform.eulerAngles.y;
		float targetYaw = targetRotation.eulerAngles.y;

		float deltaYaw = Mathf.DeltaAngle(currentYaw, targetYaw);
		float clampedDelta = Mathf.Clamp(deltaYaw, -allowedDelta, allowedDelta);

		float newYaw = currentYaw + clampedDelta;
		Vector3 euler = stateMachine.transform.eulerAngles;
		euler.y = newYaw;
		stateMachine.transform.rotation = Quaternion.Euler(euler);
       
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
