using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("Trigger an enemy jump-away animation state and complete when the state finishes.")]
	[TaskIcon("{SkinColor}JumpIcon.png")]
	public class JumpAway : Action
	{
		private EnemyStateMachine esm;

		public SharedInt taskAllowed;

		public override void OnStart()
		{
			esm = GetComponent<EnemyStateMachine>();
			if (esm == null) return;

			esm.SwitchState(new EnemyJumpAwayState(esm));
		}

		public override TaskStatus OnUpdate()
		{
			if (esm == null) return TaskStatus.Failure;

			if (esm.currentState is EnemyJumpAwayState s)
			{
				return s.IsFinished ? TaskStatus.Success : TaskStatus.Running;
			}

			// State was interrupted by other higher priority transitions
			return TaskStatus.Failure;
		}

		public override void OnEnd()
		{
			if (esm == null) return;

			// Only revert to idle if this task still owns the jump-away state.
			if (esm.currentState is EnemyJumpAwayState)
			{
				esm.SwitchState(new EnemyIdleState(esm));
			}
		}

		public override void OnBehaviorRestart()
		{

			base.OnBehaviorRestart();

			taskAllowed.Value = 100;

		}
	}
}



