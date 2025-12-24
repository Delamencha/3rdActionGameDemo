using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("Switch the enemy into the Dead state and wait for the death animation to finish.")]
	public class Dead : Action
	{


		private EnemyStateMachine esm;

		public override void OnStart()
		{
			esm = GetComponent<EnemyStateMachine>();
			if (esm == null) return;

			esm.SwitchState(new EnemyDeadState(esm));
		}

		public override TaskStatus OnUpdate()
		{
			if (esm == null) return TaskStatus.Failure;

			if (esm.currentState is EnemyDeadState dead)
			{
				return dead.IsFinished ? TaskStatus.Success : TaskStatus.Running;
			}

			// State was interrupted by other transitions
			return TaskStatus.Failure;
		}

		public override void OnEnd()
		{
			// Intentionally do nothing: dead is a terminal state.
		}

		public override void OnReset()
		{
		}
	}
}


