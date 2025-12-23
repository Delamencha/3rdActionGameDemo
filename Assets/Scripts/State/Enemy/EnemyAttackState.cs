using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
	private readonly string attackName;
	private EnemyAttakData attackData;
	private int animationHash;

	public bool IsFinished { get; private set; }

	private bool projectileSpawned;
	private bool weaponDamageWasActive;

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
		projectileSpawned = false;

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

		// Configure damage source:
		// - MeleeAttack: WeaponDamage collider deals damage.
		// - RangeAttack: A projectile deals damage (WeaponDamage temporarily disabled to avoid accidental melee hits).
		float damage0 = (attackData.damageValue != null && attackData.damageValue.Count > 0) ? attackData.damageValue[0] : 0f;
		float knock0 = (attackData.knockbackValue != null && attackData.knockbackValue.Count > 0) ? attackData.knockbackValue[0] : 0f;

		if (attackData.EnemyAttackType == EnemyAttackType.RangeAttack)
		{
			if (stateMachine.WeaponDamage != null)
			{
				weaponDamageWasActive = stateMachine.WeaponDamage.gameObject.activeSelf;
				stateMachine.WeaponDamage.gameObject.SetActive(false);
			}
		}
		else
		{
			weaponDamageWasActive = false;
			if (stateMachine.WeaponDamage != null)
			{
				stateMachine.WeaponDamage.SetAttack(damage0, knock0, attackData.knockbackType, null);
			}
		}

		// Start measuring raw root-motion distance (in-session cache, useful if clipRootDistance isn't filled yet)
		if (attackData.applyRootMotion)
		{
			stateMachine.BeginRootMotionDistanceMeasurement(animationHash);
		}

		// Configure root-motion distance matching (compute once on Enter)
		stateMachine.ClearRootMotionTuning();
		if (attackData.applyRootMotion && attackData.enableDistanceMatch && stateMachine.Player != null)
		{
			Vector3 to = stateMachine.Player.transform.position - stateMachine.transform.position;
			to.y = 0f;
			float dist = to.magnitude;

			float stopDist = Mathf.Max(0f, stateMachine.AttackRange);
			float need = Mathf.Max(0f, dist - stopDist);

			float clipDist = attackData.clipRootDistance;
			if (clipDist <= 0.0001f)
			{
				// Fallback to in-session measured cache (only works after at least one play)
				stateMachine.TryGetCachedRootMotionDistance(animationHash, out clipDist);
			}

			if (clipDist > 0.0001f)
			{
				float scale = need / Mathf.Max(0.0001f, clipDist);
			scale = Mathf.Clamp(scale, attackData.minRootScale, attackData.maxRootScale);

			stateMachine.ConfigureRootMotionTuning(stateMachine.Player.transform, stopDist, scale, clampToStop: true);
			}
		}

		// Initialize turning limits for this attack
		accumulatedTurnDeg = 0f;
		totalTurnLimitDeg = Mathf.Max(0f, attackData.TotalTurnLimitDeg);
	}

	public override void Tick(float deltaTime)
	{
		// Face player with per-attack total turning limit
		TryFacePlayerLimited(deltaTime);

		// Spawn projectile for ranged attacks at configured timing.
		if (!projectileSpawned && attackData != null && attackData.EnemyAttackType == EnemyAttackType.RangeAttack)
		{
			float spawnAt = Mathf.Clamp01(attackData.projectileSpawnNormalizedTime);
			if (GetNormalizedTime(stateMachine.Animator, "Attack") >= spawnAt)
			{
				SpawnProjectile();
				projectileSpawned = true;
			}
		}

		// Mark finished when the attack animation completes (uses fixed state name in Animator)
		if (!IsFinished && GetNormalizedTime(stateMachine.Animator, "Attack") >= 1f)
		{
			IsFinished = true;
		}
	}

	public override void Exit()
	{
		//Debug.Log($"[EnemyAttackState] Total turned degrees this attack: {accumulatedTurnDeg:F2}");
		// Reset root motion to default (disabled) after attack
		stateMachine.Animator.applyRootMotion = false;
		stateMachine.ClearRootMotionTuning();

		// End measurement and cache result for this session
		if (attackData != null && attackData.applyRootMotion)
		{
			float measured = stateMachine.EndRootMotionDistanceMeasurement(cacheResult: true);
			if (attackData.enableDistanceMatch && attackData.clipRootDistance <= 0.0001f && measured > 0.0001f)
			{
				Debug.Log($"[EnemyAttackState] Measured clip root distance for '{attackName}' ({attackData.AnimationName}) = {measured:F3}m. Consider copying this into EnemyAttakData.clipRootDistance for stable distance matching.");
			}
		}

		// Restore weapon damage collider state if we disabled it for ranged attacks.
		if (attackData != null && attackData.EnemyAttackType == EnemyAttackType.RangeAttack && stateMachine.WeaponDamage != null)
		{
			stateMachine.WeaponDamage.gameObject.SetActive(weaponDamageWasActive);
		}
	}

	private void SpawnProjectile()
	{
		if (attackData == null) return;
		if (attackData.projectilePrefab == null) return;
		if (stateMachine.Player == null) return;

		Transform spawn = stateMachine.ProjectileSpawnPoint != null
			? stateMachine.ProjectileSpawnPoint
			: (stateMachine.WeaponDamage != null ? stateMachine.WeaponDamage.transform : stateMachine.transform);

		Vector3 spawnPos = spawn.position + spawn.TransformDirection(attackData.projectileSpawnOffset);

		Vector3 to = stateMachine.Player.transform.position - spawnPos;
		if (to.sqrMagnitude < 0.0001f) to = stateMachine.transform.forward;
		Quaternion rot = Quaternion.LookRotation(to.normalized, Vector3.up);

		GameObject go = Object.Instantiate(attackData.projectilePrefab, spawnPos, rot);

		float damage0 = (attackData.damageValue != null && attackData.damageValue.Count > 0) ? attackData.damageValue[0] : 0f;
		float knock0 = (attackData.knockbackValue != null && attackData.knockbackValue.Count > 0) ? attackData.knockbackValue[0] : 0f;

		var proj = go.GetComponentInChildren<Projectile>();
		if (proj != null)
		{
			var mode = attackData.projectileHoming ? Projectile.TrajectoryMode.Homing : Projectile.TrajectoryMode.Straight;
			proj.Initialize(
				attacker: stateMachine.gameObject,
				target: stateMachine.Player.transform,
				damage: damage0,
				knockBack: knock0,
				knockbackType: attackData.knockbackType,
				speed: attackData.projectileSpeed,
				lifetime: attackData.projectileLifetime,
				colliderEnableDelay: attackData.projectileColliderEnableDelay,
				trajectory: mode,
				homingTurnSpeedDeg: attackData.projectileHomingTurnSpeedDeg
			);
		}
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


