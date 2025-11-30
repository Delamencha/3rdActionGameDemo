using UnityEngine;

public class EnemyDodgeState : EnemyBaseState
{
	private readonly Vector2 dodgeDirection;

	private readonly int DodgeHash = Animator.StringToHash("DodgingBlendTree");
	private readonly int DodgeRightBlendHash = Animator.StringToHash("TargetingRightSpeed");
	private readonly int DodgeForwardBlendHash = Animator.StringToHash("TargetingForwardSpeed");

	private const float CrossFadeDuration = 0.1f;
	private const float TurnSpeedDeg = 720f;

	private bool isInvulnerable;
	private const float InvulnerableStart = 0.125f;
	private const float InvulnerableEnd = 0.5625f;

	public bool IsFinished { get; private set; }

	public EnemyDodgeState(EnemyStateMachine stateMachine, Vector2 dodgeDirection) : base(stateMachine)
	{
		this.dodgeDirection = dodgeDirection;
	}

	public override void Enter()
	{
		IsFinished = false;

		isInvulnerable = false;

		// Dodge uses root motion for displacement
		stateMachine.Animator.applyRootMotion = true;

		// Set blend values for directional dodge and play animation
		stateMachine.Animator.SetFloat(DodgeForwardBlendHash, dodgeDirection.y);
		stateMachine.Animator.SetFloat(DodgeRightBlendHash, dodgeDirection.x);
		stateMachine.Animator.CrossFadeInFixedTime(DodgeHash, CrossFadeDuration);
	}

	public override void Tick(float deltaTime)
	{
		// Face player smoothly while dodging
		FacePlayer(TurnSpeedDeg, deltaTime);

        //设置无敌帧
        float normalizedTime = GetNormalizedTime(stateMachine.Animator, "Dodge");
		if (!isInvulnerable && normalizedTime >= InvulnerableStart && normalizedTime <= InvulnerableEnd)
		{
			isInvulnerable = true;
			stateMachine.Health.ActiveInvulnerable();
		}
		else if (isInvulnerable && (normalizedTime < InvulnerableStart || normalizedTime > InvulnerableEnd))
		{
			isInvulnerable = false;
			stateMachine.Health.DeactiveInvulnerable();
		}

		// End by normalized time reaching the end of "Dodge"
		if (!IsFinished && GetNormalizedTime(stateMachine.Animator, "Dodge") >= 1f)
		{
			IsFinished = true;
		}
	}

	public override void Exit()
	{
		stateMachine.Animator.applyRootMotion = false;
	}
}


