using System;
using UnityEngine;

public class PauseMenuController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private MenuManager menuManager;

    [Header("Panels")]
    [SerializeField] private GameObject pauseContainer;
    [SerializeField] private GameObject settingsPanel;

    private bool isPaused;

    //Event
    public static event Action<bool> OnGamePaused;

    private void Start() {
        Resume();
    }

    private void OnEnable() {
        playerInputHandler.OnCancelAction += PlayerInputHandler_OnCancelAction;
    }

    private void OnDisable() {
        playerInputHandler.OnCancelAction -= PlayerInputHandler_OnCancelAction;
    }

    #region Pause Button Action
    public void Pause() {
        menuManager.SetMenuState(Menus.PauseMenu);

        StopTime(true);

        pauseContainer.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void Resume() {
        menuManager.SetMenuState(Menus.InGame);

        StopTime(false);

        pauseContainer.SetActive(false);
        settingsPanel.SetActive(false);
    }
    #endregion

    public void StopTime(bool state) {
        isPaused = state;
        Cursor.visible = state;
        OnGamePaused?.Invoke(state);

        if (state) {
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
        } else {
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void PlayerInputHandler_OnCancelAction() {
        if (menuManager.MenuState == Menus.PauseMenu
            || menuManager.MenuState == Menus.Settings
            || menuManager.MenuState == Menus.KeybindWindow
            || menuManager.MenuState == Menus.QuitConfirmation
            || menuManager.MenuState == Menus.Quit
            || menuManager.MenuState == Menus.MerchantMenu)
            return;

        if (isPaused) Resume();
        else Pause();
    }

}