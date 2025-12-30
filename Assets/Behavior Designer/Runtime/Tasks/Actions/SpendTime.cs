using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	[TaskDescription("Wait/spend time. Records start time on enter and returns Success once elapsed time exceeds duration.")]
	public class SpendTime : Action
	{
		[Tooltip("Time to wait (seconds).")]
		public SharedFloat duration = 1f;

		private float startTime;

		public override void OnStart()
		{
			startTime = Time.time;
		}

		public override TaskStatus OnUpdate()
		{
			float d = duration != null ? duration.Value : 0f;
			if (d <= 0f) return TaskStatus.Success;

			return (Time.time - startTime) >= d ? TaskStatus.Success : TaskStatus.Running;
		}

		public override void OnReset()
		{
			if (duration != null) duration.Value = 1f;
		}
	}
}


