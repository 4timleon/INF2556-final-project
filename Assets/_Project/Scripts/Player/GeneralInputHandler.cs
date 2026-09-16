using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GeneralInputHandler : MonoBehaviour
{
    public static GeneralInputHandler Instance;

    private const string PLAYER_PREFS_BINDINGS = "InputBindings";

    [SerializeField] private bool debug = false;

    private InputSystem_Actions inputActions;

    public bool InteractTriggered { get; private set; }
    public bool PauseTriggered { get; private set; }

    public enum Binding
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Sprint,
        Aim,
        Shoot,
        Pause,
        Jump,
        Reload,
        Interact
    }

    //Event
    public event Action OnInteractAction;
    public event Action OnNavigateAction;
    public event Action OnPauseAction;
    public event Action OnCancelAction;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        inputActions = new InputSystem_Actions();

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            inputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }

        inputActions.Enable();

        if (debug)
        {
            Debug.Log(inputActions.Player.Move.bindings[2].ToDisplayString());
            Debug.Log(inputActions.Player.Move.bindings[4].ToDisplayString());
            Debug.Log(inputActions.Player.Move.bindings[6].ToDisplayString());
            Debug.Log(inputActions.Player.Move.bindings[8].ToDisplayString());
            Debug.Log(inputActions.Player.Sprint.bindings[0].ToDisplayString());
            Debug.Log(inputActions.Player.Aim.bindings[0].ToDisplayString());
            Debug.Log(inputActions.Player.Attack.bindings[1].ToDisplayString());
            Debug.Log(inputActions.UI.Cancel.bindings[0].ToDisplayString());
            Debug.Log(inputActions.Player.Jump.bindings[0].ToDisplayString());
            Debug.Log(inputActions.Player.Reload.bindings[0].ToDisplayString());
            Debug.Log(inputActions.Player.MouseWheel.bindings[0].ToDisplayString());
            Debug.Log(inputActions.Player.Interact.bindings[0].ToDisplayString());
        }

        SubscribeToInputActions();
        SubscribeToUIInputActions();
    }
    private void OnDisable()
    {
        UnsubscribeToInputActions();
        UnsubscribeToUIInputActions();
    }
    private void SubscribeToInputActions()
    {
        inputActions.Player.Interact.performed += InteractAction;
    }

    private void UnsubscribeToInputActions()
    {
        inputActions.Player.Interact.canceled -= InteractAction;
    }

    private void SubscribeToUIInputActions()
    {
        inputActions.UI.Navigate.performed += NavigateAction;
        inputActions.UI.Cancel.performed += CancelAction;
    }

    private void UnsubscribeToUIInputActions()
    {
        inputActions.UI.Navigate.performed -= NavigateAction;
        inputActions.UI.Cancel.performed -= CancelAction;
    }

    private void InteractAction(InputAction.CallbackContext context)
    {
        OnInteractAction?.Invoke();
    }

    private void NavigateAction(InputAction.CallbackContext context)
    {
        OnNavigateAction?.Invoke();
    }

    private void CancelAction(InputAction.CallbackContext context)
    {
        OnCancelAction?.Invoke();
    }

    public void PauseAction(InputAction.CallbackContext context)
    {
        OnPauseAction?.Invoke();
    }

    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
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
                return inputActions.Player.Attack.bindings[1].ToDisplayString();
            case Binding.Pause:
                return inputActions.UI.Cancel.bindings[0].ToDisplayString();
            case Binding.Jump:
                return inputActions.Player.Jump.bindings[0].ToDisplayString();
            case Binding.Reload:
                return inputActions.Player.Reload.bindings[0].ToDisplayString();
            case Binding.Interact:
                return inputActions.Player.Interact.bindings[0].ToDisplayString();
        }
    }

    public void RebindBinding(Binding binding, Action onActionRebound)
    {
        inputActions.Player.Disable();

        InputAction inputAction;
        int bindingIndex;

        switch (binding)
        {
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
                inputAction = inputActions.Player.Attack;
                bindingIndex = 1;
                break;
            case Binding.Pause:
                inputAction = inputActions.UI.Cancel;
                bindingIndex = 0;
                break;
            case Binding.Jump:
                inputAction = inputActions.Player.Jump;
                bindingIndex = 0;
                break;
            case Binding.Reload:
                inputAction = inputActions.Player.Reload;
                bindingIndex = 0;
                break;
            case Binding.Interact:
                inputAction = inputActions.Player.Interact;
                bindingIndex = 0;
                break;
        }

        inputAction?.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(callback =>
            {
                callback.Dispose();
                inputActions.Player.Enable();
                onActionRebound();

                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, inputActions.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();
            })
            .OnCancel(callback =>
            {
                callback.Dispose();
                inputActions.Player.Enable();
                onActionRebound();
            })
            .Start();
    }

    public void ResetBindings()
    {
        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(PLAYER_PREFS_BINDINGS);
    }

    public InputSystem_Actions GetInputActions()
    {
        return inputActions;
    }
}
