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
    [field: SerializeField] public InputWeightsSO InputWeights { get; private set; }

    public float PreviousDodgeTime { get; private set; } = Mathf.NegativeInfinity;
    public InputBuffer Buffer { get; private set; } = new InputBuffer();
    public Transform MainCameraTransform { get; private set; }
    public float allowedDelta { get; private set; } = 30f;
    /// <summary>
    /// 取消跳转白名单：键为状态类名，值为是否允许“当前状态”被该状态打断（取消）。
    /// 注意：该表不用于“普通跳转”的筛选，普通跳转由各状态的 Enter/Update 逻辑决定，
    /// 仅用于动画帧事件打开的取消窗口内，配合输入缓冲解析，决定允许哪些状态打断当前状态。
    /// </summary>
    public Dictionary<string, bool> StateTransitionMap { get; private set; } = new Dictionary<string, bool>();

    // State priority and input mappings for buffered input resolution
    public Dictionary<string, int> StatePriorityMap { get; private set; } = new Dictionary<string, int>();
    public Dictionary<string, PlayerBufferedInputType> StateInputMap { get; private set; } = new Dictionary<string, PlayerBufferedInputType>();

    public bool allowInputBufferRead  { get; set; } = false;

    [field: SerializeField] public float BufferWindowDuration { get; private set; } = 0.3f;
    

    private void Awake()
    {
        // Seed known player states; default to true (allowed)
        InitializeTransitionMap();
        InitializePriorityAndInputMaps();
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

    //通过动画帧事件调用，对于Attack而言，在能被走路取消的时候，就不需要预输入了
    //为防止动画中没有设置的情况，在State的Exit阶段也调用一次
    public void DeactivateInputBuffer()
    {
        Buffer.Deactivate(true);
        allowInputBufferRead = false;
    }

    /// <summary>
    /// 仅在取消窗口内读取并解析预输入，根据 StateTransitionMap 的允许项和优先级进行“取消跳转”。
    /// 不处理“普通跳转”（普通跳转由各状态自身在动画结束后按逻辑触发）。
    /// </summary>
    //方法开始在tick()中被调用时，预输入的 读取 正式开始，Buffer.Deactivate后，在功能层面关闭 读取，
    //在State Exit()后，新的State中还未开始调用时，在方法层面关闭 读取
    public bool ApplyBufferedInput()
    {
        // Only handle during an active buffer window
        if (!Buffer.IsActive || !allowInputBufferRead) return false;

        // Gather allowed states that have input mappings
        var allowedStates = new List<string>();
        foreach (var kv in StateTransitionMap)
        {
            if (kv.Value && StateInputMap.ContainsKey(kv.Key))
            {
                allowedStates.Add(kv.Key);
            }
        }
        if (allowedStates.Count == 0) return false;

        // Sort by priority (lower number means higher priority)
        allowedStates.Sort((a, b) =>
        {
            int pa = StatePriorityMap.ContainsKey(a) ? StatePriorityMap[a] : int.MaxValue;
            int pb = StatePriorityMap.ContainsKey(b) ? StatePriorityMap[b] : int.MaxValue;
            return pa.CompareTo(pb);
        });

        // Try to consume a matching input within the window duration
        foreach (var stateName in allowedStates)
        {
            var type = StateInputMap[stateName];
            //无Debug版
            //if (!Buffer.TryConsumeType(type, BufferWindowDuration)) continue;
            if (!Buffer.TryConsumeType(type, BufferWindowDuration, out var consumedInfo)) continue;

            Debug.Log($"[InputBuffer] consumed for state={stateName}: {consumedInfo}");
            SwitchByStateName(stateName);
            return true;
        }

        return false;
    }

    public void ActivateInputBufferRead(int flag)
    {
        allowInputBufferRead = (flag == 1);
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

    public void SetStateTransitionBanned(string stateName)
    {
        StateTransitionMap[stateName] = false;

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

    private void InitializePriorityAndInputMaps()
    {
        // Confirmed priority order (lower is higher priority)
        StatePriorityMap["PlayerDodgeState"] = 0;
        StatePriorityMap["PlayerBlockState"] = 1;
        StatePriorityMap["PlayerJumpState"] = 2;
        StatePriorityMap["PlayerAttackState"] = 3;
        StatePriorityMap["PlayerTargetingState"] = 4;
        StatePriorityMap["PlayerFreeRunState"] = 5;

        // Input mapping per state
        StateInputMap["PlayerDodgeState"] = PlayerBufferedInputType.Dodge;
        StateInputMap["PlayerBlockState"] = PlayerBufferedInputType.Block;
        StateInputMap["PlayerJumpState"] = PlayerBufferedInputType.Jump;
        StateInputMap["PlayerAttackState"] = PlayerBufferedInputType.Attack;
        StateInputMap["PlayerTargetingState"] = PlayerBufferedInputType.Target;
        StateInputMap["PlayerFreeRunState"] = PlayerBufferedInputType.Run;
        StateInputMap["PlayerSkillState"] = PlayerBufferedInputType.Skill;
    }


    private void SwitchByStateName(string stateName)
    {
        switch (stateName)
        {
            case "PlayerDodgeState":
            {
                Vector2 dir = InputReader.MovementValue == Vector2.zero ? new Vector2(0, -1) : InputReader.MovementValue;
                SwitchState(new PlayerDodgeState(this, dir));
                break;
            }
            case "PlayerBlockState":
                SwitchState(new PlayerBlockState(this));
                break;
            case "PlayerJumpState":
                SwitchState(new PlayerJumpState(this));
                break;
            case "PlayerAttackState":
                {
                    int idx = 0;
                    var currentAttackState = currentState as PlayerAttackState;
                    if (currentAttackState != null)
                    {
                        int next = currentAttackState.NextComboIndex;
                        if (next >= 0) idx = next;
                    }
                    SwitchState(new PlayerAttackState(this, idx));
                break;
            }
            case "PlayerTargetingState":
                SwitchState(new PlayerTargetingState(this));
                break;
            case "PlayerFreeRunState":
                SwitchState(new PlayerFreeRunState(this));
                break;
            case "PlayerSkillState":
                SwitchState(new PlayerSkillState(this));
                break;
        }
    }

}
