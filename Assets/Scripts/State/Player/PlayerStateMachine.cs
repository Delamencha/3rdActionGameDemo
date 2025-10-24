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
    [field: SerializeField] public float FaceTargetTurnSpeed { get; private set; } = 720f;
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

    [field: SerializeField] public InputWeightsSO InputWeights { get; private set; }
    public InputBuffer Buffer { get; private set; } = new InputBuffer();

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

        if (InputReader != null)
        {
            InputReader.AttackPressed += OnAttackPressedBuffered;
            InputReader.DogeEvent += OnDogeBuffered;
            InputReader.JumpEvent += OnJumpBuffered;
            InputReader.TargetEvent += OnTargetBuffered;
            InputReader.RunEvent += OnRunBuffered;
            InputReader.SkillEvent += OnSkillBuffered;
            InputReader.BlockPressed += OnBlockPressedBuffered;
        }
    }



    private void OnDisable()
    {
        Health.OnTakeDamage -= HandleTakeDamage;
        Health.OnDie -= HandleDeath;

        if (InputReader != null)
        {
            InputReader.AttackPressed -= OnAttackPressedBuffered;
            InputReader.DogeEvent -= OnDogeBuffered;
            InputReader.JumpEvent -= OnJumpBuffered;
            InputReader.TargetEvent -= OnTargetBuffered;
            InputReader.RunEvent -= OnRunBuffered;
            InputReader.SkillEvent -= OnSkillBuffered;
            InputReader.BlockPressed -= OnBlockPressedBuffered;
        }
    }

    private void OnAttackPressedBuffered()
    {
        if (!Buffer.IsActive || InputWeights == null) return;
        Buffer.TryAdd(PlayerBufferedInputType.Attack, InputWeights.AttackWeight);
    }

    private void OnDogeBuffered()
    {
        if (!Buffer.IsActive || InputWeights == null) return;
        Buffer.TryAdd(PlayerBufferedInputType.Dodge, InputWeights.DodgeWeight);
    }

    private void OnJumpBuffered()
    {
        if (!Buffer.IsActive || InputWeights == null) return;
        Buffer.TryAdd(PlayerBufferedInputType.Jump, InputWeights.JumpWeight);
    }

    private void OnTargetBuffered()
    {
        if (!Buffer.IsActive || InputWeights == null) return;
        Buffer.TryAdd(PlayerBufferedInputType.Target, InputWeights.TargetWeight);
    }

    private void OnRunBuffered()
    {
        if (!Buffer.IsActive || InputWeights == null) return;
        Buffer.TryAdd(PlayerBufferedInputType.Run, InputWeights.RunWeight);
    }

    private void OnSkillBuffered()
    {
        if (!Buffer.IsActive || InputWeights == null) return;
        Buffer.TryAdd(PlayerBufferedInputType.Skill, InputWeights.SkillWeight);
    }

    private void OnBlockPressedBuffered()
    {
        if (!Buffer.IsActive || InputWeights == null) return;
        Buffer.TryAdd(PlayerBufferedInputType.Block, InputWeights.BlockWeight);
    }

    public void ActivateInputBuffer()
    {
        Buffer.Activate();
    }

    public void DeactivateInputBuffer(bool clear)
    {
        Buffer.Deactivate(clear);
    }

    public bool ApplyBufferedInput()
    {
        if (!Buffer.TryConsumeTop(out var type)) return false;
        switch (type)
        {
            case PlayerBufferedInputType.Attack:
                if (IsStateTransitionAllowed("PlayerAttackState"))
                {
                    SwitchState(new PlayerAttackState(this, 0));
                }
                break;
            case PlayerBufferedInputType.Dodge:
                if (IsStateTransitionAllowed("PlayerDodgeState"))
                {
                    SwitchState(new PlayerDodgeState(this, InputReader.MovementValue == Vector2.zero ? new Vector2(0, -1) : InputReader.MovementValue));
                }
                break;
            case PlayerBufferedInputType.Jump:
                if (IsStateTransitionAllowed("PlayerJumpState"))
                {
                    SwitchState(new PlayerJumpState(this));
                }
                break;
            case PlayerBufferedInputType.Target:
                SwitchState(new PlayerTargetingState(this));
                break;
            case PlayerBufferedInputType.Run:
                SwitchState(new PlayerFreeRunState(this));
                break;
            case PlayerBufferedInputType.Skill:
                SwitchState(new PlayerSkillState(this));
                break;
            case PlayerBufferedInputType.Block:
                SwitchState(new PlayerBlockState(this));
                break;
        }
        return true;
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
