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
    Forward,        // 单纯向前
    AwayFromAttacker, // 远离攻击者
    TowardsAttacker,  // 拉向攻击者
    Upwards,        // 向上击飞
}
