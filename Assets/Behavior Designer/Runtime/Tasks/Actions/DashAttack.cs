using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Combat;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("Perform an attack against the target using the specified attack name.")]
	public class DashAttack : Action
	{
		[Tooltip("Target GameObject to attack.")]
		public SharedGameObject target;

		[Tooltip("Attack name key (matches EnemyAttakData.AnimationName).")]
		public SharedString AttackName;

		private EnemyStateMachine esm;

		public SharedInt taskAllowed;

		public override void OnStart()
		{
			esm = GetComponent<EnemyStateMachine>();
			if (esm == null) return;

			var name = AttackName != null ? AttackName.Value : string.Empty;
			if (string.IsNullOrEmpty(name)) return;

			esm.SwitchState(new EnemyAttackState(esm, name));
		}

		public override TaskStatus OnUpdate()
		{
			if (esm == null) return TaskStatus.Failure;

			if (esm.currentState is EnemyAttackState attack)
			{
				if (!attack.IsFinished) return TaskStatus.Running;

				// Success only when the attack ended having damaged the target at least once.
				return attack.AttackHitState == AttackHitState.Damaged ? TaskStatus.Success : TaskStatus.Failure;
			}

			// State was interrupted by other transitions
			return TaskStatus.Failure;
		}

		public override void OnEnd()
		{
			if (esm == null) return;

			// Only revert to idle if this task still owns the attack state.
			if (esm.currentState is EnemyAttackState)
			{
				esm.SwitchState(new EnemyIdleState(esm));
			}
		}

		public override void OnReset()
		{
			if (target != null) target.Value = null;
			if (AttackName != null) AttackName.Value = string.Empty;
		}

		public override void OnBehaviorRestart()
		{

			base.OnBehaviorRestart();

			taskAllowed.Value = 100;

		}
	}
}


