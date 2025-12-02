using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

public class WaitLog : Action
{
    private int counter;
    private float timer;

    public override void OnStart()
    {
        counter = 0;
        timer = 0;
    }

    public override TaskStatus OnUpdate()
    {
        timer += Time.deltaTime;
        if(timer > 1f)
        {
            timer = 0;
            Debug.Log(counter++);
        }


        return TaskStatus.Running;
    }

}
