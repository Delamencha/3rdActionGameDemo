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

    // Soft lock support
    public Target CurrentSoftLockTarget { get; private set; }
    [SerializeField] private float SoftLockConeAngleDeg = 60f;
    [SerializeField] private float SoftLockAcquireRange = 5f;
    [SerializeField] private float SoftLockBreakDistance = 12f;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.name);
        if(other.TryGetComponent<Target>(out Target target))
        {
            targets.Add(target);
            target.OnTargetDestroy += RemoveTarget;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log(other.name);

        if(other.TryGetComponent<Target>(out Target target))
        {
            //targets.Remove(target);
            RemoveTarget(target);
        }

    }

    public bool SelectTarget()
    {
        if (targets.Count <= 0) return false;

        // Prefer soft lock target when available and valid/visible
        if (CurrentSoftLockTarget != null && targets.Contains(CurrentSoftLockTarget))
        {
            var renderer = CurrentSoftLockTarget.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.isVisible)
            {
                CurrentTarget = CurrentSoftLockTarget;
                cineTargetGroup.AddMember(CurrentTarget.transform, 1f, 2f);
                ClearSoftLock();
                return true;
            }
        }

        Target closestTarget = null;
        float closetDistance = Mathf.Infinity;

        foreach(Target target in targets)
        {
            Vector2 screenPos = mainCamera.WorldToViewportPoint(target.transform.position);
            //if(screenPos.x < 0 || screenPos.x > 1 ||  screenPos.y < 0 || screenPos.y > 1)
            if(!target.GetComponentInChildren<Renderer>().isVisible)
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
            // Hard lock and soft lock are mutually exclusive
            ClearSoftLock();
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
        // Ensure no soft lock lingers when cancelling
        ClearSoftLock();
    }


    //��target destroy �Լ� out of range ʱ������
    private void RemoveTarget(Target target)
    {
        if(CurrentTarget == target)
        {
            cineTargetGroup.RemoveMember(CurrentTarget.transform);
            CurrentTarget = null;
        }

        if (CurrentSoftLockTarget == target)
        {
            ClearSoftLock();
        }

        target.OnTargetDestroy -= RemoveTarget;
        targets.Remove(target);

    }

    // Soft lock API
    public void SetSoftLock(Target target)
    {
        if (target == null) return;
        if (CurrentTarget != null) return; // Do not set soft lock when hard locked

        if (CurrentSoftLockTarget == target) return;

        ClearSoftLock();
        CurrentSoftLockTarget = target;
        CurrentSoftLockTarget.OnTargetDestroy += OnSoftLockTargetDestroyed;
        Debug.Log("Soft Lock target set: " + CurrentSoftLockTarget.name);
    }

    public void ClearSoftLock()
    {
        if (CurrentSoftLockTarget != null)
        {
            var prev = CurrentSoftLockTarget;
            CurrentSoftLockTarget.OnTargetDestroy -= OnSoftLockTargetDestroyed;
            CurrentSoftLockTarget = null;
            Debug.Log("Soft Lock target cleared: " + prev.name);
        }
    }

    private void OnSoftLockTargetDestroyed(Target _)
    {
        ClearSoftLock();
    }

    public bool IsSoftLockValid(Transform player)
    {
        if (CurrentSoftLockTarget == null) return false;
        float dist = Vector3.Distance(player.position, CurrentSoftLockTarget.transform.position);
        return dist <= SoftLockBreakDistance;
    }

    // cameraForward should be world-space forward of camera (flattened internally)
    public bool TryAcquireSoftLockByInput(Transform player, Vector3 cameraForward)
    {
        if (CurrentTarget != null) return false; // no soft lock when hard-locked

        Target best = null;
        float bestDist = Mathf.Infinity;

        Vector3 camFwd = cameraForward;
        camFwd.y = 0f;
        if (camFwd.sqrMagnitude < 0.0001f) camFwd = player.forward;
        camFwd.Normalize();

        Vector3 playerPos = player.position;

        foreach (var t in targets)
        {
            if (t == null) continue;
            Vector3 to = t.transform.position - playerPos;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist > SoftLockAcquireRange) continue;
            if (dist < 0.0001f) continue;
            to /= dist;

            float angle = Vector3.Angle(camFwd, to);
            if (angle <= SoftLockConeAngleDeg * 0.5f) // 60° cone full-angle -> 30° half-angle if intended; requirement states 60° total
            {
                if (dist < bestDist)
                {
                    best = t;
                    bestDist = dist;
                }
            }
        }

        if (best != null)
        {
            SetSoftLock(best);
            return true;
        }

        return false;
    }

    public void TryAcquireSoftLockByHit(Transform player, Target hit)
    {
        if (hit == null) return;
        if (CurrentTarget != null) return; // no soft lock when hard locked

        if (CurrentSoftLockTarget == null)
        {
            SetSoftLock(hit);
            return;
        }

        // Choose the nearer one to player
        float currentDist = Vector3.Distance(player.position, CurrentSoftLockTarget.transform.position);
        float newDist = Vector3.Distance(player.position, hit.transform.position);
        if (newDist < currentDist)
        {
            SetSoftLock(hit);
        }
    }
}
