using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, Controls.IPlayerActions
{

    public Vector2 MovementValue { get; private set; }

    public bool IsAttacking  { get; private set; }
    public bool IsBlocking { get; private set; }



    public event Action JumpEvent;
    public event Action DogeEvent;
    public event Action TargetEvent;
    public event Action RunEvent;
    public event Action SkillEvent;
    public event Action AttackPressed;
    public event Action BlockPressed;
    public event Action BlockReleased;

    private Controls controls;

    private void Start()
    {
        controls = new Controls();
        controls.Player.SetCallbacks(this);
        controls.Player.Enable();
    }

    private void OnDestroy()
    {
        controls.Player.Disable();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        JumpEvent?.Invoke();
    }

    public void OnDoge(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        DogeEvent?.Invoke();
    }

    public void OnMove(InputAction.CallbackContext context)
    {

        MovementValue = context.ReadValue<Vector2>();
        
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        
    }

    public void OnTargeting(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        TargetEvent?.Invoke();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsAttacking = true;
            AttackPressed?.Invoke();
        }else if (context.canceled)
        {
            IsAttacking = false;
        }
    }

    public void OnBlock(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsBlocking = true;
            BlockPressed?.Invoke();
        }
        else if (context.canceled)
        {
            IsBlocking = false;
            BlockReleased?.Invoke();
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        RunEvent?.Invoke();
    }

    public void OnSkill(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        SkillEvent?.Invoke();
    }
}
