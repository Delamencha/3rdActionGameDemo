using System;
using UnityEngine;

public class TeleportOnHitTester : MonoBehaviour
{
	[Header("Teleport Settings")]
	[SerializeField] private float teleportRadiusMeters = 5f;
	[SerializeField] private bool randomInsideCircle = true;

	[Header("Gizmo Settings")]
	[SerializeField] private Color gizmoColor = new Color(0f, 0.9f, 1f, 0.85f);
	[SerializeField, Range(8, 256)] private int gizmoSegments = 64;

	private Health cachedHealth;

	private void OnEnable()
	{
		cachedHealth = GetComponent<Health>();
		if (cachedHealth != null)
		{
			cachedHealth.OnTakeDamage += OnTakeDamage;
		}
	}

	private void OnDisable()
	{
		if (cachedHealth != null)
		{
			cachedHealth.OnTakeDamage -= OnTakeDamage;
		}
	}

	private void OnTakeDamage(ImpactType _)
	{
		TeleportWithinRadius();
	}

	private void TeleportWithinRadius()
	{
		if (teleportRadiusMeters <= 0f) return;

		Vector3 center = transform.position;

		Vector2 offset2D = randomInsideCircle
			? UnityEngine.Random.insideUnitCircle * teleportRadiusMeters
			: UnityEngine.Random.insideUnitCircle.normalized * teleportRadiusMeters;

		Vector3 offset = new Vector3(offset2D.x, 0f, offset2D.y);

		// Directly set position as requested (do not route through CharacterController)
		transform.position = center + offset;
	}

	private void OnDrawGizmosSelected()
	{
		if (teleportRadiusMeters <= 0f || gizmoSegments < 8) return;

		Gizmos.color = gizmoColor;
		DrawCircleXZ(transform.position, teleportRadiusMeters, gizmoSegments);
	}

	private void DrawCircleXZ(Vector3 center, float radius, int segments)
	{
		float angleStep = Mathf.PI * 2f / segments;
		Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

		for (int i = 1; i <= segments; i++)
		{
			float angle = angleStep * i;
			Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
			Gizmos.DrawLine(prevPoint, nextPoint);
			prevPoint = nextPoint;
		}
	}
}

