using UnityEngine;

public class EnemyJumpAwayState : EnemyBaseState
{
	private readonly int JumpAwayHash = Animator.StringToHash("JumpAway_Start");
	private const float CrossFadeDuration = 0.1f;
	private const string JumpLoopTag = "JumpLoop";
	private const string LandTag = "Land";
	private const string StartTag = "JumpStart";
	private const string LandTrigger = "ShouldLand";

	public bool IsFinished { get; private set; }

	private readonly float jumpDistance;
	private readonly float jumpDuration;
	private float movedDistance;
	private Vector3 moveDirXZ;
	private bool requestedLand;

	public EnemyJumpAwayState(EnemyStateMachine stateMachine, float jumpDistance, float jumpDuration) : base(stateMachine)
	{
		this.jumpDistance = Mathf.Max(0f, jumpDistance);
		this.jumpDuration = Mathf.Max(0.01f, jumpDuration);
	}

	public override void Enter()
	{
		IsFinished = false;

		//stateMachine.Animator.applyRootMotion = true; // we drive displacement manually

		// Play jump-away animation
		stateMachine.Animator.CrossFadeInFixedTime(JumpAwayHash, CrossFadeDuration);

		// Prepare movement
		Vector3 fwd = stateMachine.transform.forward;
		fwd.y = 0f;
		moveDirXZ = fwd.sqrMagnitude > 0.0001f ? (-fwd.normalized) : Vector3.back;
		movedDistance = 0f;
		requestedLand = false;
	}

	public override void Tick(float deltaTime)
	{
		if (IsFinished) return;

		// During loop, move backward in XZ until target distance reached
		float landTime = GetNormalizedTime(stateMachine.Animator, LandTag);
		AnimatorStateInfo currentInfo = stateMachine.Animator.GetCurrentAnimatorStateInfo(0);
		//if (!currentInfo.IsTag(LandTag))
        //{

			if (!requestedLand && movedDistance < jumpDistance)
			{
				//if (currentInfo.IsTag(StartTag) && GetNormalizedTime(stateMachine.Animator, StartTag) < 0.3) return;
				float speed = jumpDistance / jumpDuration;
				float remaining = Mathf.Max(0f, jumpDistance - movedDistance);
				float frameVelocityMag = Mathf.Min(speed, remaining / Mathf.Max(0.0001f, deltaTime));
				Vector3 velocity = moveDirXZ * frameVelocityMag;
				Move(velocity, deltaTime);
				movedDistance += frameVelocityMag * deltaTime;

				if (movedDistance >= jumpDistance)
				{
					// Request landing transition
					stateMachine.Animator.SetTrigger(LandTrigger);
					requestedLand = true;
				}
			}
			else
			{
				if (landTime < 0.25f)
				{
					float speed = jumpDistance / jumpDuration;
					Vector3 velocity = moveDirXZ * speed;
					// No further displacement, just progress animation
					Move(velocity, deltaTime);
				}
				else
				{
					Move(deltaTime);
				}


			}

		//}

		// End when landing tag completes
		if (GetNormalizedTime(stateMachine.Animator, LandTag) >= 1f)
		{
			IsFinished = true;
			return;
		}
	}

	public override void Exit()
	{
		//stateMachine.Animator.applyRootMotion = false;
	}
}


