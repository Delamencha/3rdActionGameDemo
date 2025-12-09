using System;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// 攻击事件参数：由攻击状态（例如 PlayerAttackState）在攻击生效时构造并抛出。
    /// </summary>
    public struct AttackEventArgs
    {
        /// <summary>攻击发起者（通常是 Player 或 Enemy 对象）。</summary>
        public GameObject Attacker;

        /// <summary>被命中的目标对象（如果有的话）。</summary>
        public GameObject Target;

        /// <summary>命中点（如果可以获取，通常由 WeaponDamage 或碰撞检测提供）。</summary>
        public Vector3 HitPoint;

        /// <summary>本次攻击所使用的特效/音效配置。</summary>
        public AttackEffectData EffectData;
    }

    /// <summary>
    /// 受击事件参数：由 Health 或类似组件在真正扣血时抛出（目前可暂时不使用，只是预留）。
    /// </summary>
    public struct DamageEventArgs
    {
        /// <summary>受击者。</summary>
        public GameObject Victim;

        /// <summary>伤害来源（攻击者或投射物等）。</summary>
        public GameObject Source;

        /// <summary>受击表现配置，通常是通用的 HitEffectData。</summary>
        public HitEffectData HitEffectData;
    }

    /// <summary>
    /// 统一的战斗事件中枢。
    /// 逻辑层（AttackState / Health 等）通过这里发出“发生了什么”的事件；
    /// 表现层（EffectsManager 等）只订阅这里的事件并播放对应的特效/音效。
    /// </summary>
    public static class CombatEvents
    {
        public static event Action<AttackEventArgs> OnAttackPerformed;
        public static event Action<DamageEventArgs> OnDamaged;

        public static void RaiseAttackPerformed(AttackEventArgs args)
        {
            OnAttackPerformed?.Invoke(args);
        }

        public static void RaiseDamaged(DamageEventArgs args)
        {
            OnDamaged?.Invoke(args);
        }
    }
}


