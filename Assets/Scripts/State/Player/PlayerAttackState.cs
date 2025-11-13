using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    //private float previousFrameTime = -1;

    private AttackData currentAttack;

    private bool hasAddForce;

    private float accumulatedTurnDeg;
    private float totalTurnLimitDeg;

    //需拓展 -> Light + Heavy
    public int NextComboIndex => currentAttack != null ? currentAttack.LightComboStateIndex : -1;
    public int NextHeavyComboIndex => currentAttack != null ? currentAttack.HeavyComboStateIndex : -1;

    public PlayerAttackState(PlayerStateMachine stateMachine, int attackIndex) : base(stateMachine)
    {
        if (stateMachine.ComboSequence != null && stateMachine.ComboSequence.Attack_Dic != null && attackIndex >= 0 && attackIndex < stateMachine.ComboSequence.Attack_Dic.Count)
        {
            //currentAttack = stateMachine.ComboSequence.attacks[attackIndex];
            currentAttack = stateMachine.ComboSequence.Attack_Dic[attackIndex];
        }
    }

    public override void Enter()
    {
        stateMachine.ResetAllTransitions(false);
        //Debug.Log("turing into animation" + currentAttack.AnimationName);
        stateMachine.Animator.applyRootMotion = true;

        //使用CrossFadeInFixedTime导致前一个动画的帧事件仍会执行，且没有相对简易的解决方案，故暂用Play()
        stateMachine.Animator.Play(currentAttack.AnimationName);
        //stateMachine.Animator.CrossFadeInFixedTime(currentAttack.AnimationName, currentAttack.TransitionDuration);
        stateMachine.WeaponDamage.SetAttack(currentAttack.DamageValue, currentAttack.Knockback, currentAttack.knockbackType);

        stateMachine.InputReader.JumpEvent += OnJump;
        stateMachine.InputReader.DogeEvent += OnDodge;
        stateMachine.InputReader.AttackEvent += OnAttack;
        stateMachine.InputReader.HeavyAttackEvent += OnHeavyAttack;

        accumulatedTurnDeg = 0f;
        totalTurnLimitDeg = (currentAttack != null) ? Mathf.Max(0f, currentAttack.TotalTurnLimitDeg) : 0f;

        //进入AttackState即开启预输入的 写入 或是在动画的帧事件中设置开始写入的时间
        //stateMachine.ActivateInputBuffer();

        // Soft lock acquisition by input at the start of attack when no hard lock
        if (stateMachine.Targeter.CurrentTarget == null)
        {
            stateMachine.Targeter.TryAcquireSoftLockByInput(stateMachine.transform, stateMachine.MainCameraTransform.forward);
        }
        // Subscribe to hit events to acquire soft lock by hit
        stateMachine.WeaponDamage.OnTargetHit += OnTargetHit;
    }

    public override void Tick(float deltaTime)
    {
        //Debug.Log("Dodge transition value : " + stateMachine.IsStateTransitionAllowed("PlayerDodgeState"));

        Move(deltaTime);

        // Validate/break soft lock conditions when not hard locked
        if (stateMachine.Targeter.CurrentTarget == null && stateMachine.Targeter.CurrentSoftLockTarget != null)
        {
            if (!stateMachine.Targeter.IsSoftLockValid(stateMachine.transform))
            {
                stateMachine.Targeter.ClearSoftLock();
            }
            else
            {
                Vector3 move = calculateMovement();
                if (move.sqrMagnitude > 0.0001f)
                {
                    Vector3 toSoft = (stateMachine.Targeter.CurrentSoftLockTarget.transform.position - stateMachine.transform.position);
                    toSoft.y = 0f; move.y = 0f;
                    if (Vector3.Angle(move, toSoft) > 90f)
                    {
                        stateMachine.Targeter.ClearSoftLock();
                    }
                }
            }
        }

        if (stateMachine.allowTuring)
        {
            if (stateMachine.Targeter.CurrentTarget != null)
            {

                if (totalTurnLimitDeg > 0f)
                {
                    float remaining = Mathf.Max(0f, totalTurnLimitDeg - accumulatedTurnDeg);
                    if (remaining > 0f)
                    {
                        float beforeYaw = stateMachine.transform.eulerAngles.y;
                        TryFaceTarget(remaining, deltaTime);
                        float afterYaw = stateMachine.transform.eulerAngles.y;
                        accumulatedTurnDeg += Mathf.Abs(Mathf.DeltaAngle(beforeYaw, afterYaw));
                    }
                }
            }
            else
            {
                // Use soft lock turning when present
                if (stateMachine.Targeter.CurrentSoftLockTarget != null && totalTurnLimitDeg > 0f)
                {
                    Vector3 toSoft = stateMachine.Targeter.CurrentSoftLockTarget.transform.position - stateMachine.transform.position;
                    toSoft.y = 0f;
                    if (toSoft.sqrMagnitude > 0.0001f && stateMachine.faceTargetTurnSpeed > 0f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(toSoft);
                        float speedDegPerSec = stateMachine.faceTargetTurnSpeed;
                        float maxStepThisFrame = speedDegPerSec * deltaTime;
                        float remaining = Mathf.Max(0f, totalTurnLimitDeg - accumulatedTurnDeg);
                        if (remaining > 0f)
                        {
                            float step = Mathf.Min(maxStepThisFrame, remaining);
                            float beforeYaw = stateMachine.transform.eulerAngles.y;
                            stateMachine.transform.rotation = Quaternion.RotateTowards(stateMachine.transform.rotation, targetRotation, step);
                            float afterYaw = stateMachine.transform.eulerAngles.y;
                            accumulatedTurnDeg += Mathf.Abs(Mathf.DeltaAngle(beforeYaw, afterYaw));
                        }
                    }
                }
                //重新设计，AttackState中的方向输入应该作为软锁定选择目标，或脱离软锁定的依据
                //TryFaceMovemnetDirection(calculateMovement(), deltaTime);
            }
        }


        float normalizedTime = GetNormalizedTime(stateMachine.Animator, "Attack");

        if(normalizedTime < 1f)
        {
            //经过一定时间，对于Attack而言是在打击帧结束之后，开启预输入的 读取 (Data层面限制)
            // 与stateMachine中ActivateInputBufferRead()有重复，有些冗余 （Animation层面限制）
            if (normalizedTime >= currentAttack.AnimationCancelTime)
            {

                stateMachine.ApplyBufferedInput();
            }

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
                    //tateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
                    ReturnToLocomotion();
                    return;
                }
            }

            //if (stateMachine.InputReader.IsAttacking)
            //{
            //    TryComboAttack(normalizedTime);
            //}


        }
        else
        {


            if(stateMachine.Targeter.CurrentTarget != null)
            {
                Debug.Log("Getting back to targetting state");
                stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
            }
            else
            {
                stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
            }
        }

    }


    public override void Exit()
    {
        stateMachine.InputReader.JumpEvent -= OnJump;
        stateMachine.InputReader.DogeEvent -= OnDodge;
        stateMachine.InputReader.AttackEvent -= OnAttack;
        stateMachine.InputReader.HeavyAttackEvent -= OnHeavyAttack;
        stateMachine.WeaponDamage.OnTargetHit -= OnTargetHit;

        stateMachine.ResetAllTransitions(false);
        stateMachine.AllowTuring();

        stateMachine.DeactivateInputBuffer();
        stateMachine.ResetAllTransitions(false);

        stateMachine.Animator.applyRootMotion = false;

        // Ensure soft lock is cleared when exiting attack
        //stateMachine.Targeter.ClearSoftLock();

        //Debug.Log("accumulatedTurnDeg : " + accumulatedTurnDeg);

    }



    private void TryComboAttack(float normalizedTime)
    {
        if (currentAttack.LightComboStateIndex == -1) return;

        //if (normalizedTime < currentAttack.ComboAttackTime) return;
        if(stateMachine.IsStateTransitionAllowed("PlayerAttackState")){
            stateMachine.SwitchState(new PlayerAttackState(stateMachine, currentAttack.LightComboStateIndex));
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
        if (movement.sqrMagnitude < 0.0001f || stateMachine.faceTargetTurnSpeed <= 0) return;

        Vector3 flatMovement = movement;
        flatMovement.y = 0f;
        if (flatMovement.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(flatMovement);

        float speedDegPerSec = stateMachine.faceTargetTurnSpeed;
        float maxStepThisFrame = speedDegPerSec * deltaTime;

        float step =  maxStepThisFrame;

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
            Debug.Log("Attack State On dodge");
            stateMachine.SwitchState(new PlayerDodgeState(stateMachine, 
                stateMachine.InputReader.MovementValue == Vector2.zero ? new Vector2(0, -1) : stateMachine.InputReader.MovementValue));
        }
        
    }

    private void OnAttack()
    {
        if (currentAttack.LightComboStateIndex == -1) return;

        if (stateMachine.IsStateTransitionAllowed("PlayerAttackState"))
        {
            stateMachine.SwitchState(new PlayerAttackState(stateMachine, currentAttack.LightComboStateIndex));
        }
    }

    private void OnHeavyAttack()
    {
        if (currentAttack.HeavyComboStateIndex == -1) return;

        if (stateMachine.IsStateTransitionAllowed("PlayerAttackState"))
        {
            stateMachine.SwitchState(new PlayerAttackState(stateMachine, currentAttack.HeavyComboStateIndex));
        }
    }



    private void OnTargetHit(Target target)
    {
        if (stateMachine.Targeter.CurrentTarget != null) return;
        stateMachine.Targeter.TryAcquireSoftLockByHit(stateMachine.transform, target);
    }

}
