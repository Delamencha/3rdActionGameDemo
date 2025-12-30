namespace Combat
{
	/// <summary>
	/// Per-attack hit resolution state (melee-focused).
	/// 0: None (no contact that matters)
	/// 1: Damaged target
	/// 2: Hit target but was blocked
	/// 3: Hit target but was dodged (invulnerable)
	/// </summary>
	public enum AttackHitState
	{
		None = 0,
		Damaged = 1,
		Blocked = 2,
		Dodged = 3
	}
}


