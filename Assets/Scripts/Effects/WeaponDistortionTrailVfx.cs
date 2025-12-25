using System.Collections;
using Combat;
using UnityEngine;

/// <summary>
/// VFX prefab controller: receives AttackEventArgs from EffectsManager, finds weapon TrailBase/TrailTip,
/// then drives a WeaponRibbonTrail. Intended to be used as AttackEffectData.SwingVfxPrefab.
/// </summary>
[DisallowMultipleComponent]
public class WeaponDistortionTrailVfx : MonoBehaviour, IAttackVfxInitializable
{
    [Header("Trail Points (names on weapon prefab)")]
    [SerializeField] private string trailBaseName = "TrailBase";
    [SerializeField] private string trailTipName = "TrailTip";

    [Header("Timing")]
    [Tooltip("Fallback stop time (seconds) when AttackEffectData.SwingVfxDuration <= 0.")]
    [Min(0.01f)]
    [SerializeField] private float fallbackStopAfterSeconds = 0.25f;

    [Tooltip("Ribbon lifetime (seconds). This defines how long the visible arc remains behind the blade.")]
    [Min(0.01f)]
    [SerializeField] private float ribbonLifetimeSeconds = 0.18f;

    private WeaponRibbonTrail ribbon;
    private Coroutine stopRoutine;

    private void Awake()
    {
        ribbon = GetComponent<WeaponRibbonTrail>();
        if (ribbon == null)
        {
            ribbon = gameObject.AddComponent<WeaponRibbonTrail>();
        }
        ribbon.SetLifetime(ribbonLifetimeSeconds);
    }

    public void Initialize(AttackEventArgs args)
    {
        // Resolve weapon damage from attacker state machine (player/enemy), per your project convention.
        var weaponDamage = ResolveWeaponDamage(args.Attacker);
        if (weaponDamage == null)
        {
            // Fallback: try find in attacker hierarchy (keeps prefab robust during iteration).
            weaponDamage = args.Attacker != null ? args.Attacker.GetComponentInChildren<WeaponDamage>(true) : null;
        }

        Transform baseTr = null;
        Transform tipTr = null;

        if (weaponDamage != null)
        {
            // In this project, WeaponDamage lives on "WeaponLogic", while TrailBase/TrailTip are siblings
            // under the same parent (see user hierarchy screenshot). Prefer searching the parent so we can
            // find sibling points reliably, and still support points placed under WeaponLogic or deeper.
            Transform searchRoot = weaponDamage.transform.parent != null ? weaponDamage.transform.parent : weaponDamage.transform;

            baseTr = FindByNameRecursive(searchRoot, trailBaseName);
            tipTr = FindByNameRecursive(searchRoot, trailTipName);
        }

        if (baseTr == null || tipTr == null)
        {
            // As a last resort, try search within this prefab instance (if user placed points here).
            baseTr = baseTr != null ? baseTr : FindByNameRecursive(transform, trailBaseName);
            tipTr = tipTr != null ? tipTr : FindByNameRecursive(transform, trailTipName);
        }

        if (baseTr != null && tipTr != null)
        {
            ribbon.SetPoints(baseTr, tipTr);
            ribbon.SetLifetime(ribbonLifetimeSeconds);
        }
        else
        {
            Debug.LogWarning($"[WeaponDistortionTrailVfx] Missing trail points '{trailBaseName}'/'{trailTipName}'. " +
                             $"Make sure they exist under the weapon prefab (recommended), or under this VFX prefab.", this);
        }

        float stopAfter = fallbackStopAfterSeconds;
        if (args.EffectData != null && args.EffectData.SwingVfxDuration > 0f)
        {
            stopAfter = args.EffectData.SwingVfxDuration;
        }

        if (stopRoutine != null) StopCoroutine(stopRoutine);
        stopRoutine = StartCoroutine(StopAfter(stopAfter));
    }

    private IEnumerator StopAfter(float seconds)
    {
        seconds = Mathf.Max(0.01f, seconds);
        yield return new WaitForSeconds(seconds);

        if (ribbon != null)
        {
            ribbon.StopEmitting();
        }
    }

    private static WeaponDamage ResolveWeaponDamage(GameObject attacker)
    {
        if (attacker == null) return null;

        var player = attacker.GetComponent<PlayerStateMachine>();
        if (player != null && player.WeaponDamage != null) return player.WeaponDamage;

        var enemy = attacker.GetComponent<EnemyStateMachine>();
        if (enemy != null && enemy.WeaponDamage != null) return enemy.WeaponDamage;

        return null;
    }

    private static Transform FindByNameRecursive(Transform root, string name)
    {
        if (root == null || string.IsNullOrWhiteSpace(name)) return null;
        if (root.name == name) return root;

        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindByNameRecursive(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}


