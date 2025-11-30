using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("Move away from the target by switching to EnemyMovingState (back off).")]
	[TaskIcon("{SkinColor}MoveTowardsIcon.png")]
	public class MoveAway : Action
	{
		[Tooltip("Target GameObject to move away from.")]
		public SharedGameObject target;

		[Tooltip("Move speed (m/s).")]
		public SharedFloat speed = 3f;

		private EnemyStateMachine esm;

		public override void OnStart()
		{
			esm = GetComponent<EnemyStateMachine>();
			if (esm == null) return;
			if (target == null || target.Value == null) return;

			esm.SwitchState(new EnemyMovingState(
				esm,
				Mathf.Max(0f, speed.Value),
				true // moveAway = true
			));
		}

		public override TaskStatus OnUpdate()
		{
			if (esm == null) return TaskStatus.Failure;
			return TaskStatus.Running; // stopping is decided by other tasks (interrupt)
		}

		public override void OnEnd()
		{
			if (esm == null) return;

			// Only revert to idle if this task still owns the movement state.
			if (esm.currentState is EnemyMovingState)
			{
				esm.SwitchState(new EnemyIdleState(esm));
			}
		}
	}
}

