using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("Perform a dodge using the specified direction.")]
	public class Dodge : Action
	{
		[Tooltip("Target GameObject to react to (optional).")]
		public SharedGameObject target;

		[Tooltip("Dodge direction in local X/Z (x=right, y=forward).")]
		public SharedVector2 DodgeDirection;

		private EnemyStateMachine esm;

		public override void OnStart()
		{
			esm = GetComponent<EnemyStateMachine>();
			if (esm == null) return;

			Debug.Log("Enter Dodge Task");

			Vector2 dir = DodgeDirection != null ? DodgeDirection.Value : new Vector2(0, -1);
			esm.SwitchState(new EnemyDodgeState(esm, dir));
		}

		public override TaskStatus OnUpdate()
		{
			if (esm == null) return TaskStatus.Failure;

			if (esm.currentState is EnemyDodgeState dodge)
			{
                if (dodge.IsFinished)
                {
					Debug.Log(" Dodge Task Success");
				}
				

				return dodge.IsFinished ? TaskStatus.Success : TaskStatus.Running;
			}

			// State was interrupted
			return TaskStatus.Failure;
		}

		public override void OnReset()
		{
			if (target != null) target.Value = null;
			if (DodgeDirection != null) DodgeDirection.Value = Vector2.zero;
		}

		public override void OnEnd()
		{
			if (esm == null) return;

			// Only revert to idle if this task still owns the dodge state.
			if (esm.currentState is EnemyDodgeState)
			{
				esm.SwitchState(new EnemyIdleState(esm));
			}
		}
	}
}


