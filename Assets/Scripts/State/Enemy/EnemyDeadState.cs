using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{
    public EnemyDeadState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.Ragdoll.ToggleRagdoll(true);
        stateMachine.WeaponDamage.gameObject.SetActive(false);
        //死亡后需要从target中移除
        GameObject.Destroy(stateMachine.Target);
    }

    public override void Tick(float deltaTime)
    {

    }

    public override void Exit()
    {
        
    }

}
