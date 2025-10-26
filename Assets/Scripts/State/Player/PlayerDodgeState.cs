using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodgeState : PlayerBaseState
{
    private Vector2 dodgingDirectionInput;
    private float remainingDodgeTime;

    private readonly int DodgeHash = Animator.StringToHash("DodgeBlendTree");

    private readonly int DodgeRightBlendHash = Animator.StringToHash("DodgeRight");
    private readonly int DodgeForwardBlendHash = Animator.StringToHash("DodgeForward");

    private const float CrossFadeDuration = 0.1f;

    private const float InvulnerableStart = 0.125f;
    private const float InvulnerableEnd = 0.5625f;
    private const float PerfectDodgeStart = 0.25f;
    private const float PerfectDodgeEnd = 0.3125f;
    private bool isInvulnerable;
    private bool isPerfectDodge;

    public PlayerDodgeState(PlayerStateMachine stateMachine, Vector2 dogeDirectionInput) : base(stateMachine) 
    {
        this.dodgingDirectionInput = dogeDirectionInput;
    }

    public override void Enter()
    {
        isInvulnerable = false;
        isPerfectDodge = false;

        remainingDodgeTime = stateMachine.DodgeDuration;

        stateMachine.Animator.SetFloat(DodgeForwardBlendHash, dodgingDirectionInput.y);
        stateMachine.Animator.SetFloat(DodgeRightBlendHash, dodgingDirectionInput.x);
        stateMachine.Animator.CrossFadeInFixedTime(DodgeHash, CrossFadeDuration);

        //通过normalizedTime添加无敌窗口


    }

    public override void Tick(float deltaTime)
    {
        //设置无敌帧
        float normalizedTime = GetNormalizedTime(stateMachine.Animator, "Dodge");
        if (!isInvulnerable && normalizedTime >= InvulnerableStart && normalizedTime <= InvulnerableEnd )
        {
            isInvulnerable = true;
            stateMachine.Health.ActiveInvulnerable();
        }else if (isInvulnerable && (normalizedTime < InvulnerableStart || normalizedTime > InvulnerableEnd))
        {
            isInvulnerable = false;
            stateMachine.Health.DeactiveInvulnerable();
        }
        //设置完美闪避
        if (!isPerfectDodge && normalizedTime >= PerfectDodgeStart && normalizedTime <= PerfectDodgeEnd)
        {
            isPerfectDodge = true;
            
        }
        else if (isPerfectDodge && (normalizedTime < PerfectDodgeStart || normalizedTime > PerfectDodgeEnd))
        {
            isPerfectDodge = false;
            
        }


        Vector3 movement = new Vector3();

        movement += stateMachine.transform.right * dodgingDirectionInput.x * stateMachine.DodgeDistance / stateMachine.DodgeDuration;
        movement += stateMachine.transform.forward * dodgingDirectionInput.y * stateMachine.DodgeDistance / stateMachine.DodgeDuration;

        Move(movement, deltaTime);
        FaceTarget();

        remainingDodgeTime -= deltaTime;

        if(remainingDodgeTime <= 0)
        {
            ReturnToLocomotion();
            //stateMachine.SwitchState(new PlayerTargetingState(stateMachine));
        }

        

    }

    public override void Exit()
    {
        
        stateMachine.Health.DeactiveInvulnerable();
    }


}
