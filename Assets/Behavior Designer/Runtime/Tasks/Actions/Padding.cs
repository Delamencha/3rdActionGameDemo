using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("Request boss padding via FSM and monitor completion.")]
	[TaskIcon("{SkinColor}MoveTowardsIcon.png")]
	public class Padding : Action
	{
		[Tooltip("The transform to face/strafe around. If null, uses player by tag 'Player'.")]
		public SharedGameObject target;

		[Tooltip("Strafing speed (m/s).")]
		public SharedFloat moveSpeed = 2.5f;

		[Tooltip("How fast to turn towards the target (deg/s).")]
		public SharedFloat turnSpeed = 720f;

		[Tooltip("Padding duration (seconds). <= 0 means let state decide/end.")]
		public SharedFloat duration = 2f;

		[Tooltip("Auto switch strafe direction interval (seconds). <= 0 disables.")]
		public SharedFloat switchDirectionInterval = 0.8f;

		[Tooltip("Prefer strafe left on start (otherwise right).")]
		public SharedBool preferLeft = false;

		[Tooltip("Pause time before switching strafe direction (seconds).")]
		public SharedFloat preTurnPause = 0.2f;

		private EnemyStateMachine esm;

		public override void OnStart()
		{
			esm = GetComponent<EnemyStateMachine>();
			if (esm == null) return;

			if (target == null) return;

			// Randomize initial strafe direction each time padding starts
			bool startLeft = Random.value < 0.5f;

			esm.SwitchState(new EnemyPaddingState(
				esm,
				moveSpeed.Value,
				turnSpeed.Value,
				duration.Value,
				switchDirectionInterval.Value,
				startLeft,
				preTurnPause.Value
			));
		}

		public override TaskStatus OnUpdate()
		{
			if (esm == null) return TaskStatus.Failure;

            if (esm.currentState is EnemyPaddingState padding)
            {
                // if (padding.IsFinished)
                // {
				// 	esm.SwitchState(new EnemyIdleState(esm));

				// }
                return padding.IsFinished ? TaskStatus.Success : TaskStatus.Running;
            }

            // State was interrupted by other higher priority transitions
            return TaskStatus.Failure;
		}

		public override void OnEnd()
		{
			if (esm == null) return;

			// Only revert to idle if this task still owns the padding state.
			if (esm.currentState is EnemyPaddingState)
			{
				esm.SwitchState(new EnemyIdleState(esm));
			}
		}
	}
}

