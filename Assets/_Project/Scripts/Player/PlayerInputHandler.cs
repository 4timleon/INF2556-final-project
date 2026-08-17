using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour {

    private const string PLAYER_PREFS_BINDINGS = "InputBindings";

    private InputSystem_Actions inputActions;

    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool SprintTriggered { get; private set; }
    public bool InteractTriggered { get; private set; }
    public bool InventoryTriggered { get; private set; }
    public bool RotateItemTriggered { get; private set; }

    public enum Binding {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Sprint,
        Aim,
        Shoot,
        Pause
    }

    //Event
    //public event Action OnInteractAction;
    public event Action OnNavigateAction;

    private void OnEnable() {
        inputActions = new InputSystem_Actions();

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS)) {
            inputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }

        inputActions.Enable();

        SubscribeToInputActions();
        SubscribeToUIInputActions();
    }

    private void OnDisable() {
        UnsubscribeToUIInputActions();
    }

    /* public override void OnNetworkSpawn() {
        if (IsOwner) {
            inputActions = new InputSystem_Actions();
            inputActions.Enable();

            SubscribeToInputActions();
        }
    }

    public override void OnNetworkDespawn() {
        if (IsOwner && inputActions != null) {
            UnsubscribeToInputActions();

            inputActions.Disable();
        }
    } */

    private void SubscribeToInputActions() {
        inputActions.Player.Move.performed += context => MovementInput = context.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => MovementInput = Vector2.zero;

        inputActions.Player.Look.performed += context => LookInput = context.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += _ => LookInput = Vector2.zero;

        inputActions.Player.Sprint.performed += _ => SprintTriggered = true;
        inputActions.Player.Sprint.canceled += _ => SprintTriggered = false;

        //inputActions.Player.Interact.performed += InteractAction;
    }

    private void UnsubscribeToInputActions() {
        //inputActions.Player.Interact.canceled -= InteractAction;
    }

    private void SubscribeToUIInputActions() {
        inputActions.UI.Navigate.performed += NavigateAction;
    }

    private void UnsubscribeToUIInputActions() {
        inputActions.UI.Navigate.performed -= NavigateAction;
    }

    private void InteractAction(InputAction.CallbackContext context) {
        /* if (IsOwner) {
            OnInteractAction?.Invoke();
        } */
    }

    private void NavigateAction(InputAction.CallbackContext context) {
        OnNavigateAction?.Invoke();
    }

    public void SetInteractTriggered(bool InteractTriggered) {
        this.InteractTriggered = InteractTriggered;
    }

    public void SetInventoryTriggered(bool InventoryTriggered) {
        this.InventoryTriggered = InventoryTriggered;
    }

    public void SetRotateItemTriggered(bool RotateItemTriggered) {
        this.RotateItemTriggered = RotateItemTriggered;
    }

    public void StopLookInput() {
        LookInput = Vector2.zero;
    }

    public string GetBindingText(Binding binding) {
        switch (binding) {
            default:
            case Binding.Move_Up:
                return inputActions.Player.Move.bindings[2].ToDisplayString();
            case Binding.Move_Down:
                return inputActions.Player.Move.bindings[4].ToDisplayString();
            case Binding.Move_Left:
                return inputActions.Player.Move.bindings[6].ToDisplayString();
            case Binding.Move_Right:
                return inputActions.Player.Move.bindings[8].ToDisplayString();
            case Binding.Sprint:
                return inputActions.Player.Sprint.bindings[0].ToDisplayString();
            case Binding.Aim:
                return inputActions.Player.Aim.bindings[0].ToDisplayString();
            case Binding.Shoot:
                return inputActions.Player.Shoot.bindings[1].ToDisplayString();
            case Binding.Pause:
                return inputActions.Player.Pause.bindings[0].ToDisplayString();
        }
    }

    public void RebindBinding(Binding binding, Action onActionRebound) {
        inputActions.Player.Disable();

        InputAction inputAction;
        int bindingIndex;

        switch (binding) {
            default:
            case Binding.Move_Up:
                inputAction = inputActions.Player.Move;
                bindingIndex = 2;
                break;
            case Binding.Move_Down:
                inputAction = inputActions.Player.Move;
                bindingIndex = 4;
                break;
            case Binding.Move_Left:
                inputAction = inputActions.Player.Move;
                bindingIndex = 6;
                break;
            case Binding.Move_Right:
                inputAction = inputActions.Player.Move;
                bindingIndex = 8;
                break;
            case Binding.Sprint:
                inputAction = inputActions.Player.Sprint;
                bindingIndex = 0;
                break;
            case Binding.Aim:
                inputAction = inputActions.Player.Aim;
                bindingIndex = 0;
                break;
            case Binding.Shoot:
                inputAction = inputActions.Player.Shoot;
                bindingIndex = 1;
                break;
            case Binding.Pause:
                inputAction = inputActions.Player.Pause;
                bindingIndex = 0;
                break;
        }

        inputAction?.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(callback => {
                callback.Dispose();
                inputActions.Player.Enable();
                onActionRebound();

                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, inputActions.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();
            })
            .Start();
    }

    public void ResetBindings() {
        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(PLAYER_PREFS_BINDINGS);
    }
}
