using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup cineTargetGroup;

    private Camera mainCamera;

    private List<Target> targets = new List<Target>();

    public Target CurrentTarget { get; private set; }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if(other.TryGetComponent<Target>(out Target target))
        {
            targets.Add(target);
            target.OnTargetDestroy += RemoveTarget;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other.name);

        if(other.TryGetComponent<Target>(out Target target))
        {
            //targets.Remove(target);
            RemoveTarget(target);
        }

    }

    public bool SelectTarget()
    {
        if (targets.Count <= 0) return false;

        Target closestTarget = null;
        float closetDistance = Mathf.Infinity;

        foreach(Target target in targets)
        {
            Vector2 screenPos = mainCamera.WorldToViewportPoint(target.transform.position);
            if(screenPos.x < 0 || screenPos.x > 1 ||  screenPos.y < 0 || screenPos.y > 1)
            {
                continue;
            }

            Vector2 toCenter = screenPos - new Vector2(0.5f, 0.5f);
            if(toCenter.sqrMagnitude < closetDistance)
            {
                closestTarget = target;
                closetDistance = toCenter.sqrMagnitude;
            }

        }

        if(closestTarget != null)
        {
            CurrentTarget = closestTarget;
            cineTargetGroup.AddMember(CurrentTarget.transform, 1f, 2f);
            return true;
        }
        else
        {
            return false;
        }

    }

    public void Cancel()
    {   
        if(CurrentTarget != null)
        {
            cineTargetGroup.RemoveMember(CurrentTarget.transform);
        }
 
        CurrentTarget = null;
    }


    //在target destroy 以及 out of range 时均触发
    private void RemoveTarget(Target target)
    {
        if(CurrentTarget == target)
        {
            cineTargetGroup.RemoveMember(CurrentTarget.transform);
            CurrentTarget = null;
        }

        target.OnTargetDestroy -= RemoveTarget;
        targets.Remove(target);

    }

}
