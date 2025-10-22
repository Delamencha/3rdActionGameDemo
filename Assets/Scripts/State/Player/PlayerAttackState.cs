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
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);
        FaceTarget();

        //ͨ��normalizedTime �ж��Ƿ���������ƶ�״̬������һ������״̬ 
        float normalizedTime = GetNormalizedTime(stateMachine.Animator, "Attack");

        if(normalizedTime > previousFrameTime && normalizedTime < 1f)
        {
            if(normalizedTime >= currentAttack.ForceTime)
            {
                TryApplyForce();
            }

            if (stateMachine.InputReader.IsAttacking)
            {
                TryComboAttack(normalizedTime);
            }
        }
        else
        {
            //�������꣬������һ״̬
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
        
    }



    private void TryComboAttack(float normalizedTime)
    {
        if (currentAttack.ComboStateIndex == -1) return;

        if (normalizedTime < currentAttack.ComboAttackTime) return;

        stateMachine.SwitchState(new PlayerAttackState(stateMachine, currentAttack.ComboStateIndex));

    }

    private void TryApplyForce()
    {
        if (hasAddForce) return;
        stateMachine.ForceReceiver.AddForce(stateMachine.transform.forward * currentAttack.AttackForce);
        hasAddForce = true;
    }

}
