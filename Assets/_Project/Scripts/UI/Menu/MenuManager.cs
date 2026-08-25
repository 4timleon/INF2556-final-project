using System;
using UnityEngine;

public enum Menus {
    InGame,
    MainMenu,
    Settings,
    KeybindWindow,
    Credits,
    QuitConfirmation,
    PauseMenu,
    Quit
}

public class MenuManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PauseMenuController pauseMenuController;
    [SerializeField] private ControlsSettings controlsSettings;

    [Header("Scenes")]
    [SerializeField] private string _mainMenuScene;
    [SerializeField] private string _mainScene;

    [Header("Menus")]
    [SerializeField] private GameObject mainMenuContainer;
    [SerializeField] private GameObject settingsMenuContainer;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuContainer;
    [SerializeField] private bool isPauseMenu;

    [Header("Settings")]
    [SerializeField] private GameObject graphicsContainer;
    [SerializeField] private GameObject controlsContainer;
    [SerializeField] private GameObject soundContainer;

    [Header("Quit Confirmation")]
    [SerializeField] private GameObject quitConfirmationContainer;

    public Menus MenuState { get; private set; }

    private void Start() {
        if (!isPauseMenu) {
            mainMenuContainer.SetActive(true);
            MenuState = Menus.MainMenu;
        }

        settingsMenuContainer.SetActive(false);
        graphicsContainer.SetActive(false);
        controlsContainer.SetActive(false);
        soundContainer.SetActive(false);
        quitConfirmationContainer.SetActive(false);
    }

    private void OnEnable() {
        playerInputHandler.OnCancelAction += PlayerInputHandler_OnCancelAction;
    }

    private void OnDisable() {
        playerInputHandler.OnCancelAction -= PlayerInputHandler_OnCancelAction;
    }

    private void PlayerInputHandler_OnCancelAction() {
        switch (MenuState) {
            case Menus.InGame:
                break;
            case Menus.MainMenu:
                break;
            case Menus.PauseMenu:
                break;
            case Menus.Settings:
                SettingsBack();
                break;
            case Menus.Credits:
                //Close Credits
                mainMenuContainer.SetActive(true);
                MenuState = Menus.MainMenu;
                break;
            case Menus.KeybindWindow:
                controlsSettings.HidePressToRebindKey();
                break;
            case Menus.QuitConfirmation:
                HideQuitConfirmation();
                break;
            case Menus.Quit:
                break;
        }
    }

    #region Main Menu
    public void Play() {
        _ = LoadScene(_mainScene, true);
    }

    public void OpenSettings() {
        if (isPauseMenu) {
            pauseMenuContainer.SetActive(false);
            MenuState = Menus.Settings;
        } else {
            mainMenuContainer.SetActive(false);
            MenuState = Menus.Settings;
        }

        settingsMenuContainer.SetActive(true);
        graphicsContainer.SetActive(true);
        controlsContainer.SetActive(false);
        soundContainer.SetActive(false);
    }

    public void OpenCredits() {

    }

    public void SettingsBack() {
        if (isPauseMenu) {
            pauseMenuContainer.SetActive(true);
            MenuState = Menus.PauseMenu;
        } else {
            mainMenuContainer.SetActive(true);
            MenuState = Menus.MainMenu;
        }

        settingsMenuContainer.SetActive(false);
    }

    public void MainMenu() {
        Time.timeScale = 1f;
        _ = LoadScene(_mainMenuScene, false);
    }

    public void ShowQuitConfirmation() {
        MenuState = Menus.QuitConfirmation;
        quitConfirmationContainer.SetActive(true);
    }

    public void HideQuitConfirmation() {
        if (isPauseMenu) {
            MenuState = Menus.PauseMenu;
        } else {
            MenuState = Menus.MainMenu;
        }

        quitConfirmationContainer.SetActive(false);
    }

    public void QuitGame() {
        MenuState = Menus.Quit;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    #region Settings
    public void GraphicsOpen() {
        graphicsContainer.SetActive(true);
        controlsContainer.SetActive(false);
        soundContainer.SetActive(false);
    }

    public void ControlsOpen() {
        graphicsContainer.SetActive(false);
        controlsContainer.SetActive(true);
        soundContainer.SetActive(false);
    }

    public void SoundOpen() {
        graphicsContainer.SetActive(false);
        controlsContainer.SetActive(false);
        soundContainer.SetActive(true);
    }
    #endregion

    public async Awaitable LoadScene(string sceneName, bool useOverlay) {
        await LoadingManager.Instance.TransitionToSceneAsync(sceneName, useOverlay);
    }

    public void SetMenuState(Menus state) {
        MenuState = state;
    }
}
