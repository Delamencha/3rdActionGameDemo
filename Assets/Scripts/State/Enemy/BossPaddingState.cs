using System;
using UnityEngine;

public class BossPaddingState : EnemyBaseState
{
	
	private readonly float moveSpeed;
	private readonly float turnSpeed;
	private readonly float duration;
	private readonly float switchInterval;
	private readonly bool preferLeft;

	private Transform target;
	private float startTime;
	private float nextSwitchTime;
	private int dirSign; // -1 left, +1 right

	public bool IsFinished { get; private set; }

	public BossPaddingState(EnemyStateMachine sm,
		float moveSpeed,
		float turnSpeed,
		float duration,
		float switchInterval,
		bool preferLeft) : base(sm)
	{
		
		this.moveSpeed = Mathf.Max(0f, moveSpeed);
		this.turnSpeed = Mathf.Max(0f, turnSpeed);
		this.duration = duration;
		this.switchInterval = switchInterval;
		this.preferLeft = preferLeft;
	}

	public override void Enter()
	{

		target = stateMachine.Player.gameObject.transform;

		IsFinished = false;
		startTime = Time.time;
		dirSign = preferLeft ? -1 : 1;
		nextSwitchTime = switchInterval > 0f ? Time.time + switchInterval : Mathf.Infinity;

		// TODO: play padding/strafe animation if available
		// stateMachine.Animator.CrossFade("Boss_Strafe_Loop", 0.1f);
	}

	public override void Tick(float deltaTime)
	{
		if (IsFinished) return;

		// End by duration
		if (duration > 0f && Time.time - startTime >= duration)
		{
			IsFinished = true;
			return;
		}

		if (target == null)
		{
			IsFinished = true;
			return;
		}

		// Face target on XZ
		Vector3 to = target.position - stateMachine.transform.position;
		to.y = 0f;
		if (to.sqrMagnitude > 0.0001f)
		{
			Quaternion face = Quaternion.LookRotation(to.normalized, Vector3.up);
			float step = turnSpeed * deltaTime;
			stateMachine.transform.rotation = Quaternion.RotateTowards(stateMachine.transform.rotation, face, step);
		}

		// Tangential strafe with small radial correction to maintain radius
		Vector3 fwd = stateMachine.transform.forward;
		fwd.y = 0f;
		Vector3 moveVec = Vector3.zero;
		if (fwd.sqrMagnitude > 0.0001f)
		{
			fwd.Normalize();
			Vector3 right = Vector3.Cross(Vector3.up, fwd);
			moveVec = right * (dirSign * moveSpeed);
		}

		// Optional simple obstacle avoidance by side ray
		RaycastHit hit;
		if (Physics.Raycast(stateMachine.transform.position + Vector3.up * 0.5f,
			stateMachine.transform.right * dirSign, out hit, 0.75f))
		{
			dirSign = -dirSign;
			nextSwitchTime = switchInterval > 0f ? Time.time + switchInterval : Mathf.Infinity;
		}
		else if (Time.time >= nextSwitchTime)
		{
			dirSign = -dirSign;
			nextSwitchTime = switchInterval > 0f ? Time.time + switchInterval : Mathf.Infinity;
		}

		// Apply movement (CharacterController preferred)
		if (moveVec.sqrMagnitude > 0.0001f)
		{
			Move(moveVec, deltaTime);
		}

		// Note: external interrupts (impact/death) handled by EnemyStateMachine event handlers
	}

	public override void Exit()
	{
		// Reset animator flags if set in Enter
	}
}

