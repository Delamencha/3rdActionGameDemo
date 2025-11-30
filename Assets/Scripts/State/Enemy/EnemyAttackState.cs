using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
	private readonly string attackName;
	private EnemyAttakData attackData;
	private int animationHash;

	public bool IsFinished { get; private set; }

	private float accumulatedTurnDeg;
	private float totalTurnLimitDeg;
	private const float TurnSpeedDeg = 360f;

	public EnemyAttackState(EnemyStateMachine stateMachine, string attackName) : base(stateMachine)
	{
		this.attackName = attackName;

		var library = stateMachine.EnemyAttackLibrary;
		if (library != null && library.Attack_Dic != null && !string.IsNullOrEmpty(attackName))
		{
			if (library.Attack_Dic.TryGetValue(attackName, out var data) && data != null)
			{
				attackData = data;
				if (!string.IsNullOrEmpty(attackData.AnimationName))
				{
					animationHash = Animator.StringToHash(attackData.AnimationName);
				}
			}
		}
	}

	public override void Enter()
	{
		IsFinished = false;

		if (attackData == null || animationHash == 0)
		{
			// No data: mark finished so task can end gracefully
			IsFinished = true;
			return;
		}

		// Apply root motion setting if specified by the attack data
		stateMachine.Animator.applyRootMotion = attackData.applyRootMotion;

		// Cross-fade to the attack animation
		float transition = Mathf.Max(0f, attackData.TransitionDuration);
		stateMachine.Animator.CrossFadeInFixedTime(animationHash, transition);

		stateMachine.WeaponDamage.SetAttack(attackData.damageValue[0], attackData.knockbackValue[0], attackData.knockbackType);

		// Initialize turning limits for this attack
		accumulatedTurnDeg = 0f;
		totalTurnLimitDeg = Mathf.Max(0f, attackData.TotalTurnLimitDeg);
	}

	public override void Tick(float deltaTime)
	{
		// Face player with per-attack total turning limit
		TryFacePlayerLimited(deltaTime);

		// Mark finished when the attack animation completes (uses fixed state name in Animator)
		if (!IsFinished && GetNormalizedTime(stateMachine.Animator, "Attack") >= 1f)
		{
			IsFinished = true;
		}
	}

	public override void Exit()
	{
		Debug.Log($"[EnemyAttackState] Total turned degrees this attack: {accumulatedTurnDeg:F2}");
		// Reset root motion to default (disabled) after attack
		stateMachine.Animator.applyRootMotion = false;
	}

	private void TryFacePlayerLimited(float deltaTime)
	{
		if (stateMachine.Player == null) return;
		if (totalTurnLimitDeg <= 0f) return;

		Vector3 to = stateMachine.Player.transform.position - stateMachine.transform.position;
		to.y = 0f;
		if (to.sqrMagnitude < 0.0001f) return;

		Quaternion targetRotation = Quaternion.LookRotation(to.normalized, Vector3.up);

		float remaining = Mathf.Max(0f, totalTurnLimitDeg - accumulatedTurnDeg);
		if (remaining <= 0f) return;

		float maxStep = Mathf.Min(TurnSpeedDeg * deltaTime, remaining);

		float beforeYaw = stateMachine.transform.eulerAngles.y;
		stateMachine.transform.rotation = Quaternion.RotateTowards(stateMachine.transform.rotation, targetRotation, maxStep);
		float afterYaw = stateMachine.transform.eulerAngles.y;

		accumulatedTurnDeg += Mathf.Abs(Mathf.DeltaAngle(beforeYaw, afterYaw));
	}
}


