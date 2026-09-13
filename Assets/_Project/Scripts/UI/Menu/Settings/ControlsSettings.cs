using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsSettings : MonoBehaviour
{
    private const string MOUSE_SENS_KEY = "MouseSensitivity";

    [SerializeField] private GeneralInputHandler playerInputHandler;
    [SerializeField] private Transform confirmationScreen;
    [SerializeField] private Button resetButton;
    [SerializeField] private ButtonBinding[] bindings;
    [SerializeField] private MenuManager menuManager;

    [Header("Mouse Sensitivity")]
    [SerializeField] private Scrollbar mouseSensScrollBar;
    [SerializeField] private TextMeshProUGUI mouseSensText;

    public static System.Action<float> OnMouseSensitivityChanged;

    private void Start()
    {
        mouseSensScrollBar.value = PlayerPrefs.GetFloat(MOUSE_SENS_KEY, 0.1f);
        mouseSensText.text = mouseSensScrollBar.value.ToString("F2");
    }

    private void OnEnable()
    {
        HidePressToRebindKey();

        mouseSensScrollBar.onValueChanged.AddListener(OnSensitivityChanged);

        RegisterBindings();
        UpdateVisual();
    }

    private void OnDisable()
    {
        mouseSensScrollBar.onValueChanged.RemoveListener(OnSensitivityChanged);

        UnregisterBindings();
    }

    private void OnSensitivityChanged(float value)
    {
        value = Mathf.Max(value, 0.01f);
        OnMouseSensitivityChanged?.Invoke(value);
        mouseSensText.text = value.ToString("F2");

        PlayerPrefs.SetFloat(MOUSE_SENS_KEY, value);
        PlayerPrefs.Save();
    }

    private void RegisterBindings()
    {
        foreach (ButtonBinding binding in bindings)
        {
            binding.Button.onClick.RemoveAllListeners();
            binding.Button.onClick.AddListener(() =>
            {
                RebindBinding(binding.Binding);
            });
        }

        resetButton.onClick.AddListener(() =>
        {
            playerInputHandler.ResetBindings();
            UpdateVisual();
        });
    }

    private void UnregisterBindings()
    {
        foreach (ButtonBinding binding in bindings)
        {
            binding.Button.onClick.RemoveAllListeners();
        }

        resetButton.onClick.RemoveListener(playerInputHandler.ResetBindings);
    }

    private void UpdateVisual()
    {
        foreach (ButtonBinding binding in bindings)
        {
            //Update Text
            binding.ButtonText.text = playerInputHandler.GetBindingText(binding.Binding);
        }
    }

    private void RebindBinding(GeneralInputHandler.Binding binding)
    {
        ShowPressToRebindKey();
        playerInputHandler.RebindBinding(binding, () =>
        {
            HidePressToRebindKey();
            UpdateVisual();
        });
    }

    private void ShowPressToRebindKey()
    {
        menuManager.SetMenuState(Menus.KeybindWindow);
        confirmationScreen.gameObject.SetActive(true);
    }

    public void HidePressToRebindKey()
    {
        if (menuManager.MenuState == Menus.KeybindWindow)
        {
            menuManager.SetMenuState(Menus.Settings);
        }
        confirmationScreen.gameObject.SetActive(false);
    }
}
