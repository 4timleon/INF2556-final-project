using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsSettings : MonoBehaviour {

    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private Transform confirmationScreen;
    [SerializeField] private Button resetButton;
    [SerializeField] private ButtonBinding[] bindings;

    private void Start() {
        HidePressToRebindKey();

        RegisterBindings();
        UpdateVisual();
    }

    private void OnDisable() {
        UnregisterBindings();
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
        confirmationScreen.gameObject.SetActive(true);
    }

    private void HidePressToRebindKey() {
        confirmationScreen.gameObject.SetActive(false);
    }
}
