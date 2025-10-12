using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadState : PlayerBaseState
{

    private readonly int DeathHash = Animator.StringToHash("Death");

    private const float CrossFadeDuration = 0.1f;

    public PlayerDeadState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {

        //stateMachine.Ragdoll.ToggleRagdoll(true);
        stateMachine.Animator.CrossFadeInFixedTime(DeathHash, CrossFadeDuration);
        stateMachine.WeaponDamage.gameObject.SetActive(false);
    }

    public override void Tick(float deltaTime)
    {

    }

    public override void Exit()
    {
        
    }


}
