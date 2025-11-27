using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("Move towards the target by switching to EnemyMovingState.")]
	[TaskIcon("{SkinColor}MoveTowardsIcon.png")]
	public class MoveTo : Action
	{
		[Tooltip("Target GameObject to move towards.")]
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
				false // moveAway = false
			));
		}

		public override TaskStatus OnUpdate()
		{
			if (esm == null) return TaskStatus.Failure;
			return TaskStatus.Running; // stopping is decided by other tasks (interrupt)
		}
	}
}

