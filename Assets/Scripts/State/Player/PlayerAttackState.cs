using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private float previousFrameTime = -1;

    private AttackData currentAttack;

    private bool hasAddForce;



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
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);

        if (stateMachine.Targeter.CurrentTarget != null){
            TryFaceTarget(stateMachine.allowedDelta);
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
            //弃用
            // if (stateMachine.InputReader.IsAttacking)
            // {
            //     TryComboAttack(normalizedTime);
            // }
            TryComboAttack(normalizedTime);
        }
        else
        {
            
            //Debug.Log("normalizedTime: " + normalizedTime + "previousFrameTime" + previousFrameTime);
            //Debug.Log("return from Attack State" + currentAttack.AnimationName);
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
        // stateMachine.transform.rotation = Quaternion.Lerp(
        //     stateMachine.transform.rotation,
        //     Quaternion.LookRotation(movement),
        //     deltaTime * stateMachine.RotationDamping
        //     );
        
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
