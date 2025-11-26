using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

public class DistanceMonitor : MonoBehaviour
{
    public BehaviorTree behaviorTree;
    public Transform player;
    public Transform enemy;

    [Header("Blackboard Variables")]
    public string distanceVariableName = "PlayerDis";

    void Update()
    {
        if (behaviorTree != null && player != null && enemy != null)
        {
            float currentDistance = Vector3.Distance(player.position, enemy.position);

            // 直接设置行为树黑板变量
            behaviorTree.SetVariableValue(distanceVariableName, currentDistance);
        }
    }
}
