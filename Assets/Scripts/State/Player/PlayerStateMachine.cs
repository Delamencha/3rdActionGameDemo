using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{

    [field: SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public Targeter Targeter { get; private set; }
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public Ragdoll Ragdoll { get; private set; }
    [field: SerializeField] public LedgeDetector LedgeDetector { get; private set; }
    [field: SerializeField] public WeaponDamage WeaponDamage { get; private set; }
    [field: SerializeField] public float FreeLookMoveSpeed { get; private set; }
    [field: SerializeField] public float FreeRunSpeed { get; private set; }
    [field: SerializeField] public float BlockWalkSpeed { get; private set; }
    [field: SerializeField] public float TargetingMoveSpeed { get; private set; }
    [field: SerializeField] public float RotationDamping { get; private set; }
    [field: SerializeField] public float DodgeDuration { get; private set; }
    [field: SerializeField] public float DodgeDistance { get; private set; }
    [field: SerializeField] public float JumpForce { get; private set; }
    [field: SerializeField] public ForceReceiver ForceReceiver { get;private set; }
    [field: SerializeField] public Attack[] Attacks { get; private set; }
    [field: SerializeField] public ComboSequenceSO ComboSequence { get; private set; }

    public float PreviousDodgeTime { get; private set; } = Mathf.NegativeInfinity;

    // A map of state name -> whether it is currently allowed to transition into
    public Dictionary<string, bool> StateTransitionMap { get; private set; } = new Dictionary<string, bool>();

    public float allowedDelta { get; private set; } = 30f;

    public Transform MainCameraTransform { get; private set; }


    private void Awake()
    {
        // Seed known player states; default to true (allowed)
        InitializeTransitionMap();
    }

    private void Start()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //��¼�����transform�����ڷ�����״̬���ƶ�
        MainCameraTransform = Camera.main.transform;

        SwitchState(new PlayerFreeLookState(this));

    }

    private void OnEnable()
    {
        Health.OnTakeDamage += HandleTakeDamage;
        Health.OnDie += HandleDeath;
    }



    private void OnDisable()
    {
        Health.OnTakeDamage -= HandleTakeDamage;
        Health.OnDie -= HandleDeath;
    }

    private void HandleTakeDamage(bool isLargeImpact)
    {
        SwitchState(new PlayerImpactState(this, isLargeImpact));
    }

    private void HandleDeath()
    {
        SwitchState(new PlayerDeadState(this));
    }


    public void SetStateTransitionAllowed(string stateName)
    {
        StateTransitionMap[stateName] = true;
    }

    public bool IsStateTransitionAllowed(string stateName)
    {
        bool allowed;
        return StateTransitionMap.TryGetValue(stateName, out allowed) ? allowed : false;
    }

    public void ResetAllTransitions(bool isAllowed)
    {
        // Copy keys to avoid modification during enumeration
        var keys = new List<string>(StateTransitionMap.Keys);
        foreach (var key in keys)
        {
            StateTransitionMap[key] = isAllowed;
        }
    }

    public void SetAllowedDelta(float degree)
    {
        allowedDelta = Mathf.Clamp(degree, 0f, 180f);
    }

    public void ResetAllowedDelta()
    {
        allowedDelta = 30f;
    }

    private void InitializeTransitionMap()
    {
        // List all known player state class names here
        string[] playerStateNames = new string[]
        {
            "PlayerFreeLookState",
            "PlayerFreeRunState",
            "PlayerTargetingState",
            "PlayerTargetRunState",
            "PlayerTargetJumpState",
            "PlayerTargetBlockState",
            "PlayerAttackState",
            "PlayerImpactState",
            "PlayerDodgeState",
            "PlayerJumpState",
            "PlayerFallState",
            "PlayerBlockState",
            "PlayerSkillState"
        };

        foreach (var name in playerStateNames)
        {
            if (!StateTransitionMap.ContainsKey(name))
            {
                StateTransitionMap.Add(name, false);
            }
        }
    }

}
