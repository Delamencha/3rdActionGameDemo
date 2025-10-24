using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private float previousFrameTime = -1;

    private AttackData currentAttack;

    private bool hasAddForce;

    private float accumulatedTurnDeg;
    private float totalTurnLimitDeg;


    public PlayerAttackState(PlayerStateMachine stateMachine, int attackIndex) : base(stateMachine)
    {
        if (stateMachine.ComboSequence != null && stateMachine.ComboSequence.attacks != null && attackIndex >= 0 && attackIndex < stateMachine.ComboSequence.attacks.Count)
        {
            currentAttack = stateMachine.ComboSequence.attacks[attackIndex];
        }
    }

    public override void Enter()
    {

        //Debug.Log("turing into animation" + currentAttack.AnimationName);
        stateMachine.Animator.CrossFadeInFixedTime(currentAttack.AnimationName, currentAttack.TransitionDuration);
        stateMachine.WeaponDamage.SetAttack(currentAttack.DamageValue, currentAttack.Knockback);

        stateMachine.InputReader.JumpEvent += OnJump;
        stateMachine.InputReader.DogeEvent += OnDodge;

        accumulatedTurnDeg = 0f;
        totalTurnLimitDeg = (currentAttack != null) ? Mathf.Max(0f, currentAttack.TotalTurnLimitDeg) : 0f;

        stateMachine.ActivateInputBuffer();
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);

        if (stateMachine.Targeter.CurrentTarget != null){

            if (totalTurnLimitDeg > 0f)
            {
                float remaining = Mathf.Max(0f, totalTurnLimitDeg - accumulatedTurnDeg);
                if (remaining > 0f)
                {
                    float perFrame = Mathf.Min(stateMachine.allowedDelta, remaining);
                    float beforeYaw = stateMachine.transform.eulerAngles.y;
                    TryFaceTarget(perFrame, deltaTime);
                    float afterYaw = stateMachine.transform.eulerAngles.y;
                    accumulatedTurnDeg += Mathf.Abs(Mathf.DeltaAngle(beforeYaw, afterYaw));
                }
            }
        }else{
            TryFaceMovemnetDirection(calculateMovement(), deltaTime);
        }

        float normalizedTime = GetNormalizedTime(stateMachine.Animator, "Attack");

        if(normalizedTime > previousFrameTime && normalizedTime < 1f)
        {
            if(normalizedTime >= currentAttack.ForceTime)
            {
                TryApplyForce();
            }

            if (stateMachine.InputReader.IsBlocking)
            {
                if(stateMachine.IsStateTransitionAllowed("PlayerBlockState")){
                    stateMachine.SwitchState(new PlayerBlockState(stateMachine));
                    return;
                }

            }
            if(stateMachine.InputReader.MovementValue.sqrMagnitude > 0.5f){
                if(stateMachine.IsStateTransitionAllowed("PlayerFreeLookState")){
                    stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
                    return;
                }
            }

            if (stateMachine.InputReader.IsAttacking)
            {
                TryComboAttack(normalizedTime);
            }


        }
        else
        {
            // Leaving attack: first try buffered input; if none, fall back
            if (stateMachine.ApplyBufferedInput())
            {
                previousFrameTime = normalizedTime;
                return;
            }

            if(stateMachine.Targeter.CurrentTarget != null)
            {
                stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
            }
            else
            {
                stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
            }
        }


        previousFrameTime = normalizedTime;

    }


    public override void Exit()
    {
        stateMachine.InputReader.JumpEvent -= OnJump;
        stateMachine.InputReader.DogeEvent -= OnDodge;

        stateMachine.ResetAllTransitions(false);
        stateMachine.ResetAllowedDelta();

        stateMachine.DeactivateInputBuffer(true);

    }



    private void TryComboAttack(float normalizedTime)
    {
        if (currentAttack.ComboStateIndex == -1) return;

        //if (normalizedTime < currentAttack.ComboAttackTime) return;
        if(stateMachine.IsStateTransitionAllowed("PlayerAttackState")){
            stateMachine.SwitchState(new PlayerAttackState(stateMachine, currentAttack.ComboStateIndex));
        }
        

    }

    private void TryApplyForce()
    {
        if (hasAddForce) return;
        stateMachine.ForceReceiver.AddForce(stateMachine.transform.forward * currentAttack.AttackForce);
        hasAddForce = true;
    }

    private Vector3 calculateMovement()
    {

        Vector3 xWeight = stateMachine.MainCameraTransform.right;
        xWeight.y = 0;
        xWeight.Normalize();
        xWeight *= stateMachine.InputReader.MovementValue.x;

        Vector3 zWeight = stateMachine.MainCameraTransform.forward;
        zWeight.y = 0;
        zWeight.Normalize();
        zWeight *= stateMachine.InputReader.MovementValue.y;

        return xWeight + zWeight;
    }

    private void TryFaceMovemnetDirection(Vector3 movement, float deltaTime)
    {
        if (totalTurnLimitDeg <= 0f) return; // No rotation allowed for this attack when limit is 0
        if (movement.sqrMagnitude < 0.0001f) return;

        Vector3 flatMovement = movement;
        flatMovement.y = 0f;
        if (flatMovement.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(flatMovement);

        float allowedDelta = Mathf.Clamp(stateMachine.allowedDelta, 0f, 180f);
        if (allowedDelta < 0.0001f) return;

        float speedDegPerSec = stateMachine.FaceTargetTurnSpeed > 0f ? stateMachine.FaceTargetTurnSpeed : 360f;
        float maxStepThisFrame = speedDegPerSec * deltaTime;

        float step = Mathf.Min(allowedDelta, maxStepThisFrame);

        // Apply total rotation cap across the whole attack state lifecycle
        if (totalTurnLimitDeg > 0f)
        {
            float remaining = Mathf.Max(0f, totalTurnLimitDeg - accumulatedTurnDeg);
            if (remaining <= 0f) return;
            step = Mathf.Min(step, remaining);
        }

        float beforeYaw = stateMachine.transform.eulerAngles.y;
        stateMachine.transform.rotation = Quaternion.RotateTowards(stateMachine.transform.rotation, targetRotation, step);
        float afterYaw = stateMachine.transform.eulerAngles.y;

        if (totalTurnLimitDeg > 0f)
        {
            accumulatedTurnDeg += Mathf.Abs(Mathf.DeltaAngle(beforeYaw, afterYaw));
        }
    }

    private void OnJump()
    {
        if(stateMachine.IsStateTransitionAllowed("PlayerJumpState")){
            stateMachine.SwitchState(new PlayerJumpState(stateMachine));
        }
        
    }

    private void OnDodge()
    {
        if(stateMachine.IsStateTransitionAllowed("PlayerDodgeState")){
            stateMachine.SwitchState(new PlayerDodgeState(stateMachine, 
                stateMachine.InputReader.MovementValue == Vector2.zero ? new Vector2(0, -1) : stateMachine.InputReader.MovementValue));
        }
        
    }

}
