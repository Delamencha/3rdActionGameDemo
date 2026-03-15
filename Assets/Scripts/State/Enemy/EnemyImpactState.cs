using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyImpactState : EnemyBaseState
{

    // NOTE:
    // These names must match the *Animator State names* in your Enemy Animator Controller.
    // Add/remove entries based on how many impact animations you created.
    private static readonly int[] ImpactHashes =
    {
        Animator.StringToHash("Impact_F"),
        Animator.StringToHash("Impact_L"),
        Animator.StringToHash("Impact_R"),
        Animator.StringToHash("Impact_L_F"),
        Animator.StringToHash("Impact_L_L"),
        Animator.StringToHash("Impact_L_R"),

    };


    private const float CrossFadeDuration = 0.2f;

    private float duration = 0.8f;

    // Impact shake (visual-only): jitter on XZ plane for hit feel.
    // NOTE: We shake the visual transform (usually the Animator child), NOT the root with CharacterController.
    private const float ShakeDuration = 0.28f;
    private const float ShakeAmplitude = 0.6f;   // meters
    private const float ShakeFrequency = 32f;     // Hz-ish (used as noise speed)

    private Transform shakeTarget;
    private Vector3 shakeBaseLocalPos;
    private float shakeTime;
    private float shakeSeedX;
    private float shakeSeedZ;

    public EnemyImpactState(EnemyStateMachine stateMachine) : base(stateMachine){}

    public override void Enter()
    {
        // Pick a safe visual target to shake.
        // Prefer Animator's transform if it's not the same as the root (CharacterController holder).
        if (stateMachine.Animator != null && stateMachine.Animator.transform != stateMachine.transform)
        {
            shakeTarget = stateMachine.Animator.transform;
        }
        else if (stateMachine.transform.childCount > 0)
        {
            // Fallback: first child as "visual" if Animator is on root.
            shakeTarget = stateMachine.transform.GetChild(0);
        }
        else
        {
            shakeTarget = null;
        }

        if (shakeTarget != null)
        {
            shakeBaseLocalPos = shakeTarget.localPosition;
            shakeTime = 0f;
            shakeSeedX = Random.value * 10f;
            shakeSeedZ = Random.value * 10f + 100f;
            Debug.Log("shakeTarget: " + shakeTarget.name);
        }

        // Randomly pick one impact animation each time we enter this state.
        // If you only keep one entry in ImpactHashes, it'll behave exactly like before.
        int chosen = ImpactHashes[Random.Range(0, ImpactHashes.Length)];
        //stateMachine.Animator.CrossFadeInFixedTime(chosen, CrossFadeDuration);

        stateMachine.Animator.Play(chosen);
    }

    public override void Tick(float deltaTime)
    {


        Move(deltaTime);

        TickShake(deltaTime);

        duration -= deltaTime;

        if(duration <= 0)
        {
            stateMachine.SwitchState(new EnemyIdleState(stateMachine));
        }


    }

    private void TickShake(float deltaTime)
    {
        if (shakeTarget == null) return;

        shakeTime += deltaTime;
        if (shakeTime >= ShakeDuration)
        {
            shakeTarget.localPosition = shakeBaseLocalPos;
            return;
        }

        float t = Mathf.Clamp01(shakeTime / ShakeDuration);
        float damper = 1f - t; // linear decay

        float nx = (Mathf.PerlinNoise(shakeSeedX, shakeTime * ShakeFrequency) - 0.5f) * 2f;
        float nz = (Mathf.PerlinNoise(shakeSeedZ, shakeTime * ShakeFrequency) - 0.5f) * 2f;

        Vector3 offset = new Vector3(nx, 0f, nz) * (ShakeAmplitude * damper);
        shakeTarget.localPosition = shakeBaseLocalPos + offset;
    }

    public override void Exit()
    {
        // Ensure we don't leave any visual offset behind.
        if (shakeTarget != null)
        {
            shakeTarget.localPosition = shakeBaseLocalPos;
        }
    }

}
