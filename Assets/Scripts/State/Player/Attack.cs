using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Attack 
{

    [field: SerializeField] public string AnimationName { get; private set; }

    //动画转换的时间
    [field: SerializeField] public float TransitionDuration { get; private set; }

    //下一个连招的index,无对应-1
    [field: SerializeField] public int ComboStateIndex { get; private set; } = -1;

    //允许输入并进行下个攻击的时间
    [field: SerializeField] public float ComboAttackTime { get; private set; }

    [field: SerializeField] public float ForceTime { get; private set; }

    //攻击时自己的位移
    [field: SerializeField] public float AttackForce { get; private set; }

    [field: SerializeField] public float DamageValue { get; private set; }

    //攻击对目标的冲击力
    [field: SerializeField] public float Knockback { get; private set; }
}
