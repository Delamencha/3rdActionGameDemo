using UnityEngine;


public enum ImpactType
{
    Minor,
    Light,
    Medium,
    Heavy,
    BlockLight,
    BlockHeavy

}
public enum KnockbackType
{
    Forward,        
    AwayFromAttacker, 
    TowardsAttacker, 
    Upwards,        
}

public enum EnemyAttackType
{
    MeleeAttack,
    RangeAttack,
    DashAttack,
    AreaAttack
}