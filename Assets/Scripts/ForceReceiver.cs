using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ForceReceiver : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float drag = 0.3f;


    private Vector3 dampingVelocity;
    private Vector3 impact;

    private float verticalVelocity;

    public Vector3 Movement => impact + Vector3.up * verticalVelocity ;

    private void Update()
    {
        if (verticalVelocity < 0 && controller.isGrounded)
        {
            verticalVelocity = Physics.gravity.y * Time.deltaTime;
            //verticalVelocity = 0;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        //持续使力衰减
        //impact以及AddForce的设计使得人物从力中恢复的时间与ImpactState的duration无关
        impact = Vector3.SmoothDamp(impact, Vector3.zero, ref dampingVelocity, drag);

        if(agent != null && impact.sqrMagnitude <  0.2f * 0.2f)
        {
            impact = Vector3.zero;
            agent.enabled = true;
        }

    }

    public void AddForce(Vector3 force)
    {
        impact += force;
        if(agent != null)
        {
            agent.enabled = false;
        }
    }

    public void ResetForce()
    {
        impact = Vector3.zero;
        verticalVelocity = 0;
    }

    public void Jump(float jumpForce)
    {
        verticalVelocity += jumpForce;
    }

}
