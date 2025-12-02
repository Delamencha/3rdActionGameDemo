using System;
using UnityEngine;

public class EnemyPaddingState : EnemyBaseState
{
	
	private readonly float moveSpeed;
	private readonly float turnSpeed;
	private readonly float duration;
	private readonly float switchInterval;
	private readonly bool preferLeft;
	private readonly float preTurnPause;

	private Transform target;
	private float startTime;
	private float nextSwitchTime;
	private int dirSign; // -1 left, +1 right

	private bool pendingSwitch;
	private int pendingNewDir;
	private float switchReadyTime;

	public bool IsFinished { get; private set; }

	private readonly int TargetingHash = Animator.StringToHash("TargetingBlendTree");

	private readonly int TargetingRightBlendHash = Animator.StringToHash("TargetingRightSpeed");
	private readonly int TargetingForwardBlendHash = Animator.StringToHash("TargetingForwardSpeed");

	private const float AnimatorDampTime = 0.1f;
	private const float CrossFadeDuration = 0.1f;

	public EnemyPaddingState(EnemyStateMachine sm,
		float moveSpeed,
		float turnSpeed,
		float duration,
		float switchInterval,
		bool preferLeft,
		float preTurnPause) : base(sm)
	{
		
		this.moveSpeed = Mathf.Max(0f, moveSpeed);
		this.turnSpeed = Mathf.Max(0f, turnSpeed);
		this.duration = duration;
		this.switchInterval = switchInterval;
		this.preferLeft = preferLeft;
		this.preTurnPause = Mathf.Max(0f, preTurnPause);

		sm.Animator.SetFloat(TargetingRightBlendHash, 0);
		sm.Animator.SetFloat(TargetingForwardBlendHash, 0);

		Debug.Log("Enter new padding State");

	}

	public override void Enter()
	{

		target = stateMachine.Player.gameObject.transform;

		IsFinished = false;
		startTime = Time.time;
		dirSign = preferLeft ? -1 : 1;
		nextSwitchTime = switchInterval > 0f ? Time.time + switchInterval : Mathf.Infinity;
		pendingSwitch = false;

		stateMachine.Animator.CrossFadeInFixedTime(TargetingHash, CrossFadeDuration);

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

		// Tangential strafe with small radial correction to maintain radius (disabled while pending turn pause)
		Vector3 fwd = stateMachine.transform.forward;
		fwd.y = 0f;
		Vector3 moveVec = Vector3.zero;
		if (!pendingSwitch && fwd.sqrMagnitude > 0.0001f)
		{
			fwd.Normalize();
			Vector3 right = Vector3.Cross(Vector3.up, fwd);
			moveVec = right * (dirSign * moveSpeed);
		}

		// Direction switch handling with pre-turn pause
		if (pendingSwitch)
		{
			if (Time.time >= switchReadyTime)
			{
				dirSign = pendingNewDir;
				pendingSwitch = false;
				nextSwitchTime = switchInterval > 0f ? Time.time + switchInterval : Mathf.Infinity;
			}
		}
		else
		{
			// Optional simple obstacle avoidance by side ray
			RaycastHit hit;
			bool shouldSwitch = false;
			if (Physics.Raycast(stateMachine.transform.position + Vector3.up * 0.5f,
				stateMachine.transform.right * dirSign, out hit, 0.75f))
			{
				shouldSwitch = true;
			}
			else if (Time.time >= nextSwitchTime)
			{
				shouldSwitch = true;
			}

			if (shouldSwitch)
			{
				if (preTurnPause > 0f)
				{
					pendingSwitch = true;
					pendingNewDir = -dirSign;
					switchReadyTime = Time.time + preTurnPause;
				}
				else
				{
					dirSign = -dirSign;
					nextSwitchTime = switchInterval > 0f ? Time.time + switchInterval : Mathf.Infinity;
				}
			}
		}

		// Apply movement (CharacterController preferred)
		if (moveVec.sqrMagnitude > 0.0001f)
		{
			Move(moveVec, deltaTime);
		}

		UpdateAnimator(moveVec, deltaTime);

		// Note: external interrupts (impact/death) handled by EnemyStateMachine event handlers
	}

	public override void Exit()
	{
		// Reset animator flags if set in Enter
		stateMachine.Animator.SetFloat(TargetingRightBlendHash, 0);
		stateMachine.Animator.SetFloat(TargetingForwardBlendHash, 0);
	}

	private void UpdateAnimator(Vector3 move, float deltaTime)
	{
		//if (stateMachine.InputReader.MovementValue == Vector2.zero)
		//{
		//    stateMachine.Animator.SetFloat(TargetingRightBlendHash, 0, AnimatorDampTime, deltaTime);
		//    stateMachine.Animator.SetFloat(TargetingForwardBlendHash, 0, AnimatorDampTime, deltaTime);
		//    return;
		//}

		if (move.sqrMagnitude > 0.0001f && !pendingSwitch)
        {
			float value = dirSign < 0 ? -1f : 1f;
			stateMachine.Animator.SetFloat(TargetingRightBlendHash, value, AnimatorDampTime, deltaTime);
		}
        else
        {
			stateMachine.Animator.SetFloat(TargetingRightBlendHash, 0, 0.2f, deltaTime);
			stateMachine.Animator.SetFloat(TargetingForwardBlendHash, 0, 0.2f, deltaTime);
		}


	}
}

