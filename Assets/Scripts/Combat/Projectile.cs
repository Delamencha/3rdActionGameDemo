using UnityEngine;

/// <summary>
/// Simple projectile controller: lifetime, optional collider enable delay, straight/homing movement,
/// damage/knockback on hitting a Health target (player), then self-destruct.
/// </summary>
public class Projectile : MonoBehaviour
{
	public enum TrajectoryMode
	{
		Straight = 0,
		Homing = 1,
	}

	[Header("Lifecycle")]
	[SerializeField] private float lifetime = 5f;
	[SerializeField] private float colliderEnableDelay = 0f;

	[Header("Movement")]
	[SerializeField] private float speed = 12f;
	[SerializeField] private TrajectoryMode trajectory = TrajectoryMode.Straight;
	[SerializeField] private float homingTurnSpeedDeg = 720f;

	[Header("Collision")]
	[SerializeField] private Collider hitCollider;

	private float damage;
	private float knockBack;
	private KnockbackType knockbackType;

	private GameObject attacker;
	private Transform target;

	private bool initialized;
	private bool hasHit;
	private float aliveTime;
	private float colliderTimer;

	/// <summary>
	/// Initialize the projectile. Call right after Instantiate.
	/// </summary>
	public void Initialize(GameObject attacker, Transform target, float damage, float knockBack, KnockbackType knockbackType,
		float speed, float lifetime, float colliderEnableDelay, TrajectoryMode trajectory, float homingTurnSpeedDeg)
	{
		this.attacker = attacker;
		this.target = target;
		this.damage = Mathf.Max(0f, damage);
		this.knockBack = Mathf.Max(0f, knockBack);
		this.knockbackType = knockbackType;

		this.speed = Mathf.Max(0f, speed);
		this.lifetime = Mathf.Max(0.01f, lifetime);
		this.colliderEnableDelay = Mathf.Max(0f, colliderEnableDelay);
		this.trajectory = trajectory;
		this.homingTurnSpeedDeg = Mathf.Max(0f, homingTurnSpeedDeg);

		initialized = true;
		aliveTime = 0f;
		hasHit = false;

		colliderTimer = this.colliderEnableDelay;
		SetColliderEnabled(colliderTimer <= 0f);
	}

	private void Awake()
	{
		if (hitCollider == null)
		{
			hitCollider = GetComponentInChildren<Collider>();
		}
	}

	private void OnEnable()
	{
		aliveTime = 0f;
		hasHit = false;
		colliderTimer = colliderEnableDelay;
		SetColliderEnabled(colliderTimer <= 0f);
	}

	private void Update()
	{
		if (hasHit) return;

		aliveTime += Time.deltaTime;
		if (aliveTime >= lifetime)
		{
			Destroy(gameObject);
			return;
		}

		if (colliderTimer > 0f)
		{
			colliderTimer -= Time.deltaTime;
			if (colliderTimer <= 0f)
			{
				SetColliderEnabled(true);
			}
		}

		Move(Time.deltaTime);
	}

	private void Move(float deltaTime)
	{
		if (!initialized)
		{
			// Default behavior if placed in scene without init.
			transform.position += transform.forward * (speed * deltaTime);
			return;
		}

		if (trajectory == TrajectoryMode.Homing && target != null)
		{
			Vector3 to = target.position - transform.position;
			if (to.sqrMagnitude > 0.0001f)
			{
				Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, homingTurnSpeedDeg * deltaTime);
			}
		}

		transform.position += transform.forward * (speed * deltaTime);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (hasHit) return;
		if (other == null) return;
		if (hitCollider != null && other == hitCollider) return;

		// Only hit something that has Health, and prefer "Player" as target.
		var health = other.GetComponentInParent<Health>();
		if (health == null) return;

		// Prevent self-hit.
		if (attacker != null)
		{
			var attackerHealth = attacker.GetComponentInParent<Health>();
			if (attackerHealth != null && attackerHealth == health) return;
		}

		// Optional: only damage player (enemy projectiles).
		if (!health.gameObject.CompareTag("Player")) return;

		// Block logic: if player is blocking and projectile is in front, do no damage.
		if (health.IsBlocking && IsFrontalHit(health.transform, transform))
		{
			ApplyKnockback(other);
			hasHit = true;
			Destroy(gameObject);
			return;
		}

		health.DealDamage(damage, knockBack);
		ApplyKnockback(other);

		hasHit = true;
		Destroy(gameObject);
	}

	private void ApplyKnockback(Collider other)
	{
		var forceReceiver = other.GetComponentInParent<ForceReceiver>();
		if (forceReceiver == null) return;

		Vector3 dir = GetKnockbackDirection(transform, other.transform, knockbackType);
		forceReceiver.AddForce(dir * knockBack);
	}

	private void SetColliderEnabled(bool enabled)
	{
		if (hitCollider != null) hitCollider.enabled = enabled;
	}

	private static bool IsFrontalHit(Transform defender, Transform attacker)
	{
		if (defender == null || attacker == null) return false;

		Vector3 toAttacker = attacker.position - defender.position;
		toAttacker.y = 0f;
		if (toAttacker.sqrMagnitude < 0.0001f) return true;
		toAttacker.Normalize();

		Vector3 defenderForward = defender.forward;
		defenderForward.y = 0f;
		if (defenderForward.sqrMagnitude < 0.0001f) return true;
		defenderForward.Normalize();

		return Vector3.Dot(defenderForward, toAttacker) > 0f;
	}

	private static Vector3 GetKnockbackDirection(Transform attacker, Transform hitTarget, KnockbackType type)
	{
		if (attacker == null || hitTarget == null) return Vector3.forward;

		Vector3 direction = attacker.forward;
		switch (type)
		{
			case KnockbackType.Forward:
				direction = attacker.forward;
				break;
			case KnockbackType.AwayFromAttacker:
				direction = hitTarget.position - attacker.position;
				direction.y = 0f;
				break;
			case KnockbackType.TowardsAttacker:
				direction = attacker.position - hitTarget.position;
				break;
			case KnockbackType.Upwards:
				direction = Vector3.up;
				break;
		}

		return direction.sqrMagnitude > 0.0001f ? direction.normalized : attacker.forward;
	}
}


