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
        stateMachine.WeaponDamage.SetAttack(currentAttack.DamageValue, currentAttack.Knockback);

        stateMachine.InputReader.JumpEvent += OnJump;
        stateMachine.InputReader.DogeEvent += OnDodge;
        stateMachine.InputReader.AttackEvent += OnAttack;
        stateMachine.InputReader.HeavyAttackEvent += OnHeavyAttack;

        accumulatedTurnDeg = 0f;
        totalTurnLimitDeg = (currentAttack != null) ? Mathf.Max(0f, currentAttack.TotalTurnLimitDeg) : 0f;

        //进入AttackState即开启预输入的 写入 或是在动画的帧事件中设置开始写入的时间
        //stateMachine.ActivateInputBuffer();
    }

    public override void Tick(float deltaTime)
    {
        //Debug.Log("Dodge transition value : " + stateMachine.IsStateTransitionAllowed("PlayerDodgeState"));

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
                    stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));
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

        stateMachine.ResetAllTransitions(false);
        stateMachine.ResetAllowedDelta();

        stateMachine.DeactivateInputBuffer();
        stateMachine.ResetAllTransitions(false);

        stateMachine.Animator.applyRootMotion = false;

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

        float allowedDelta = Mathf.Clamp(stateMachine.allowedDelta, 0f, 180f);
        if (allowedDelta < 0.0001f) return;

        float speedDegPerSec = stateMachine.faceTargetTurnSpeed;
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

}
