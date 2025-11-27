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
		public SharedTransform target;

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

		private EnemyStateMachine esm;

		public override void OnStart()
		{
			esm = GetComponent<EnemyStateMachine>();
			if (esm == null) return;

			if (target == null) return;

			esm.SwitchState(new BossPaddingState(
				esm,
				moveSpeed.Value,
				turnSpeed.Value,
				duration.Value,
				switchDirectionInterval.Value,
				preferLeft.Value
			));
		}

		public override TaskStatus OnUpdate()
		{
			if (esm == null) return TaskStatus.Failure;

            if (esm.currentState is BossPaddingState padding)
            {
                return padding.IsFinished ? TaskStatus.Success : TaskStatus.Running;
            }

            // State was interrupted by other higher priority transitions
            return TaskStatus.Failure;
		}
	}
}

