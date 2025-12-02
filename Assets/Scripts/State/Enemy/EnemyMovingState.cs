using UnityEngine;

public class EnemyMovingState : EnemyBaseState
{
	private readonly float speed;
	private readonly bool moveAway;

	private readonly int TargetingHash = Animator.StringToHash("TargetingBlendTree");
	private readonly int TargetingRightBlendHash = Animator.StringToHash("TargetingRightSpeed");
	private readonly int TargetingForwardBlendHash = Animator.StringToHash("TargetingForwardSpeed");

	private const float AnimatorDampTime = 0.1f;
	private const float CrossFadeDuration = 0.1f;
	private const float TurnSpeedDeg = 360f;

	public EnemyMovingState(EnemyStateMachine sm, float speed, bool moveAway) : base(sm)
	{
		this.speed = Mathf.Max(0f, speed);
		this.moveAway = moveAway;

		
	}

	public override void Enter()
	{
		stateMachine.Animator.CrossFadeInFixedTime(TargetingHash, CrossFadeDuration);
		stateMachine.Animator.SetFloat(TargetingRightBlendHash, 0f);
		stateMachine.Animator.SetFloat(TargetingForwardBlendHash, 0f);
	}

	public override void Tick(float deltaTime)
	{
		Vector3 moveDir = Vector3.zero;

		if (stateMachine.Player != null)
		{
			Vector3 to = stateMachine.Player.transform.position - stateMachine.transform.position;
			to.y = 0f;
			if (to.sqrMagnitude > 0.0001f)
			{
				Vector3 dir = to.normalized;
				
				// Face movement direction
				Quaternion face = Quaternion.LookRotation(dir, Vector3.up);
				stateMachine.transform.rotation = Quaternion.RotateTowards(
					stateMachine.transform.rotation, face, TurnSpeedDeg * deltaTime);

				if (moveAway) dir = -dir;

				moveDir = dir * speed;
			}
		}

		if (moveDir.sqrMagnitude > 0.0001f)
		{
			Move(moveDir, deltaTime);
		}

		UpdateAnimator(moveDir, deltaTime);
	}

	public override void Exit()
	{
		stateMachine.Animator.SetFloat(TargetingRightBlendHash, 0f, AnimatorDampTime, Time.deltaTime);
		stateMachine.Animator.SetFloat(TargetingForwardBlendHash, 0f, AnimatorDampTime, Time.deltaTime);
	}

	private void UpdateAnimator(Vector3 worldMove, float deltaTime)
	{
		// Convert world move to local space components
		Vector3 fwd = stateMachine.transform.forward;
		Vector3 right = stateMachine.transform.right;

		float forwardVal = 0f;
		float rightVal = 0f;

		if (worldMove.sqrMagnitude > 0.0001f)
		{
			Vector3 dir = worldMove.normalized;
			forwardVal = Mathf.Clamp(Vector3.Dot(fwd, dir), -1f, 1f);
			rightVal = Mathf.Clamp(Vector3.Dot(right, dir), -1f, 1f);
		}

		stateMachine.Animator.SetFloat(TargetingRightBlendHash, rightVal, AnimatorDampTime, deltaTime);
		stateMachine.Animator.SetFloat(TargetingForwardBlendHash, forwardVal, AnimatorDampTime, deltaTime);
	}
}

