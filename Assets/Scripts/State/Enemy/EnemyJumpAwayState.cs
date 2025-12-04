using UnityEngine;

public class EnemyJumpAwayState : EnemyBaseState
{
	private readonly int JumpAwayHash = Animator.StringToHash("JumpAway_Start");
	private const float CrossFadeDuration = 0.1f;
	private const string JumpAwayTag = "Land";

	public bool IsFinished { get; private set; }

	public EnemyJumpAwayState(EnemyStateMachine stateMachine) : base(stateMachine)
	{
	}

	public override void Enter()
	{
		IsFinished = false;

		stateMachine.Animator.applyRootMotion = true;

		// Play jump-away animation
		stateMachine.Animator.CrossFadeInFixedTime(JumpAwayHash, CrossFadeDuration);
	}

	public override void Tick(float deltaTime)
	{
		if (IsFinished) return;

		// End by animation normalized time reaching end of the tagged state
		if (GetNormalizedTime(stateMachine.Animator, JumpAwayTag) >= 1f)
		{
			IsFinished = true;
			return;
		}
	}

	public override void Exit()
	{
		stateMachine.Animator.applyRootMotion = false;
	}
}


