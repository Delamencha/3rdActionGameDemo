using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("Perform an attack against the target using the specified attack name.")]
	public class RangeAttack : Action
	{
		[Tooltip("Target GameObject to attack.")]
		public SharedGameObject target;

		[Tooltip("Attack name key (matches EnemyAttakData.AnimationName).")]
		public SharedString AttackName;

		private EnemyStateMachine esm;

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
				return attack.IsFinished ? TaskStatus.Success : TaskStatus.Running;
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
	}
}


