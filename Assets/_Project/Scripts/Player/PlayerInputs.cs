using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    public Vector2 MoveInput => inputActions.Player.Move.ReadValue<Vector2>();
    public Vector2 LookInput => inputActions.Player.Look.ReadValue<Vector2>();
    public Vector2 ScrollInput => inputActions.Player.MouseWheel.ReadValue<Vector2>();

    public bool IsMoving => MoveInput.magnitude >= 0.1f;

    private void Awake()
    {
        inputActions = GameControls.GetInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Sprint.started += Input_SprintStarted;
        inputActions.Player.Sprint.canceled += Input_SprintCanceled;

        inputActions.Player.Attack.started += Input_AttackStarted;
        inputActions.Player.Attack.canceled += Input_AttackCanceled;

        inputActions.Player.Aim.started += Input_AimStarted;
        inputActions.Player.Aim.canceled += Input_AimCanceled;

        inputActions.Player.Reload.started += Input_ReloadStarted;

        inputActions.Player.Inspect.started += Input_Inspect;

        inputActions.Player.Interact.started += Input_InteractStarted;
        inputActions.Player.Interact.canceled += Input_InteractEnded;

        inputActions.Player.Jump.started += Input_Jump;

        inputActions.Player.MouseWheel.performed += Input_WeaponScroll;
    }

    private void OnDisable()
    {
        inputActions.Player.Sprint.started -= Input_SprintStarted;
        inputActions.Player.Sprint.canceled -= Input_SprintCanceled;

        inputActions.Player.Attack.started -= Input_AttackStarted;
        inputActions.Player.Attack.canceled -= Input_AttackCanceled;

        inputActions.Player.Aim.started -= Input_AimStarted;
        inputActions.Player.Aim.canceled -= Input_AimCanceled;

        inputActions.Player.Reload.started -= Input_ReloadStarted;

        inputActions.Player.Jump.started -= Input_Jump;

        inputActions.Player.MouseWheel.performed -= Input_WeaponScroll;
    }

    private void Input_WeaponScroll(InputAction.CallbackContext context) => GameEventSystem.OnPlayerWeaponScroll?.Invoke(context.ReadValue<Vector2>());
    private void Input_Jump(InputAction.CallbackContext context) => GameEventSystem.OnPlayerJump?.Invoke();
    private void Input_SprintStarted(InputAction.CallbackContext context) => GameEventSystem.OnPlayerSprintStarted?.Invoke();
    private void Input_SprintCanceled(InputAction.CallbackContext context) => GameEventSystem.OnPlayerSprintEnded?.Invoke();
    private void Input_AttackStarted(InputAction.CallbackContext context) => GameEventSystem.OnPlayerAttackStarted?.Invoke();
    private void Input_AttackCanceled(InputAction.CallbackContext context) => GameEventSystem.OnPlayerAttackEnded?.Invoke();
    private void Input_AimStarted(InputAction.CallbackContext context) => GameEventSystem.OnPlayerAimStarted?.Invoke();
    private void Input_AimCanceled(InputAction.CallbackContext context) => GameEventSystem.OnPlayerAimEnded?.Invoke();
    private void Input_ReloadStarted(InputAction.CallbackContext context) => GameEventSystem.OnPlayerReloadStarted?.Invoke();
    private void Input_Inspect(InputAction.CallbackContext context) => GameEventSystem.OnPlayerInspect?.Invoke();
    private void Input_InteractStarted(InputAction.CallbackContext context) => GameEventSystem.OnPlayerInteractStarted?.Invoke();
    private void Input_InteractEnded(InputAction.CallbackContext context) => GameEventSystem.OnPlayerInteractEnded?.Invoke();
}
