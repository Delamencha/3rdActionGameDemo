using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class RandomCompareonce : Conditional
{

    public SharedFloat successProbability = 0.5f;

    private float randomValue;

    public override void OnAwake()
    {
        // If specified, use the seed provided.
        randomValue = Random.value;
    }

    public override TaskStatus OnUpdate()
    {

        if (randomValue < successProbability.Value)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }

    public override void OnBehaviorRestart()
    {

        randomValue = Random.value;

        base.OnBehaviorRestart();
    }

}
