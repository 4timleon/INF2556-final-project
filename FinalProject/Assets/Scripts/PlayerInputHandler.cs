using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour {
    private InputSystem_Actions inputActions;

    public Vector2 MovementInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool SprintTriggered { get; private set; }
    public bool InteractTriggered { get; private set; }
    public bool InventoryTriggered { get; private set; }
    public bool RotateItemTriggered { get; private set; }

    //Event
    //public event Action OnInteractAction;

    private void Start() {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();

        SubscribeToInputActions();
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

        inputActions.Player.Interact.performed += InteractAction;
    }

    private void UnsubscribeToInputActions() {
        inputActions.Player.Interact.canceled -= InteractAction;
    }

    private void InteractAction(InputAction.CallbackContext context) {
        /* if (IsOwner) {
            OnInteractAction?.Invoke();
        } */
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
}
