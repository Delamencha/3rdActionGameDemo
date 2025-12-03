using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("Switch to idle locomotion via FSM. Returns success immediately.")]
	[TaskIcon("{SkinColor}MoveTowardsIcon.png")]
	public class Locomotion : Action
	{
		private EnemyStateMachine esm;

		public override void OnStart()
		{
			esm = GetComponent<EnemyStateMachine>();
			if (esm == null) return;

			// Only switch to idle when currently moving to avoid unintended state changes.
			if (esm.currentState is EnemyMovingState)
			{
				esm.SwitchState(new EnemyIdleState(esm));
			}
		}

		public override TaskStatus OnUpdate()
		{
			if (esm == null) return TaskStatus.Failure;
			return TaskStatus.Success;
		}
	}
}


