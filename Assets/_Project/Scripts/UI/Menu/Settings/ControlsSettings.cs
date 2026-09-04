using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsSettings : MonoBehaviour {

    private const string MOUSE_SENS_KEY = "MouseSensitivity";

    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private Transform confirmationScreen;
    [SerializeField] private Button resetButton;
    [SerializeField] private ButtonBinding[] bindings;
    [SerializeField] private MenuManager menuManager;

    [Header("Mouse Sensitivity")]
    [SerializeField] private Scrollbar mouseSensScrollBar;
    [SerializeField] private TextMeshProUGUI mouseSensText;

    public static System.Action<float> OnMouseSensitivityChanged;

    private void Start() {
        mouseSensScrollBar.value = PlayerPrefs.GetFloat(MOUSE_SENS_KEY, 0.1f);
        mouseSensText.text = mouseSensScrollBar.value.ToString("F2");
    }

    private void OnEnable() {
        HidePressToRebindKey();

        mouseSensScrollBar.onValueChanged.AddListener(OnSensitivityChanged);

        RegisterBindings();
        UpdateVisual();
    }

    private void OnDisable() {
        mouseSensScrollBar.onValueChanged.RemoveListener(OnSensitivityChanged);

        UnregisterBindings();
    }

    private void OnSensitivityChanged(float value) {
        value = Mathf.Max(value, 0.01f);
        OnMouseSensitivityChanged?.Invoke(value);
        mouseSensText.text = value.ToString("F2");

        PlayerPrefs.SetFloat(MOUSE_SENS_KEY, value);
        PlayerPrefs.Save();
    }

    private void RegisterBindings() {
        foreach (ButtonBinding binding in bindings) {
            binding.Button.onClick.RemoveAllListeners();
            binding.Button.onClick.AddListener(() => {
                RebindBinding(binding.Binding);
            });
        }

        resetButton.onClick.AddListener(() => {
            playerInputHandler.ResetBindings();
            UpdateVisual();
        });
    }

    private void UnregisterBindings() {
        foreach (ButtonBinding binding in bindings) {
            binding.Button.onClick.RemoveAllListeners();
        }

        resetButton.onClick.RemoveListener(playerInputHandler.ResetBindings);
    }

    private void UpdateVisual() {
        foreach (ButtonBinding binding in bindings) {
            UpdateButtonText(binding.Binding, binding.ButtonText);
        }
    }

    public void UpdateButtonText(PlayerInputHandler.Binding binding, TextMeshProUGUI buttonText) {
        switch (binding) {
            case PlayerInputHandler.Binding.Move_Up:
                buttonText.text = playerInputHandler.GetBindingText(PlayerInputHandler.Binding.Move_Up);
                break;
            case PlayerInputHandler.Binding.Move_Down:
                buttonText.text = playerInputHandler.GetBindingText(PlayerInputHandler.Binding.Move_Down);
                break;
            case PlayerInputHandler.Binding.Move_Left:
                buttonText.text = playerInputHandler.GetBindingText(PlayerInputHandler.Binding.Move_Left);
                break;
            case PlayerInputHandler.Binding.Move_Right:
                buttonText.text = playerInputHandler.GetBindingText(PlayerInputHandler.Binding.Move_Right);
                break;
            case PlayerInputHandler.Binding.Sprint:
                buttonText.text = playerInputHandler.GetBindingText(PlayerInputHandler.Binding.Sprint);
                break;
            case PlayerInputHandler.Binding.Aim:
                buttonText.text = playerInputHandler.GetBindingText(PlayerInputHandler.Binding.Aim);
                break;
            case PlayerInputHandler.Binding.Shoot:
                buttonText.text = playerInputHandler.GetBindingText(PlayerInputHandler.Binding.Shoot);
                break;
            case PlayerInputHandler.Binding.Pause:
                buttonText.text = playerInputHandler.GetBindingText(PlayerInputHandler.Binding.Pause);
                break;
        }
    }

    private void RebindBinding(PlayerInputHandler.Binding binding) {
        ShowPressToRebindKey();
        playerInputHandler.RebindBinding(binding, () => {
            HidePressToRebindKey();
            UpdateVisual();
        });
    }

    private void ShowPressToRebindKey() {
        menuManager.SetMenuState(Menus.KeybindWindow);
        confirmationScreen.gameObject.SetActive(true);
    }

    public void HidePressToRebindKey() {
        if (menuManager.MenuState == Menus.KeybindWindow) {
            menuManager.SetMenuState(Menus.Settings);
        }
        confirmationScreen.gameObject.SetActive(false);
    }
}
