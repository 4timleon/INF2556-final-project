using System;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private GeneralInputHandler playerInputHandler;
    [SerializeField] private MenuManager menuManager;

    [Header("Panels")]
    [SerializeField] private GameObject pauseContainer;
    [SerializeField] private GameObject settingsPanel;

    [Header("Sounds")]
    [SerializeField] private AudioSource backgroundMusic;

    private bool isPaused;
    private bool playerIsDead;
    private bool playerIsInPreDeath;

    private void Start()
    {
        playerIsDead = false;
        Resume();
    }

    private void OnEnable()
    {
        playerInputHandler.OnCancelAction += PlayerInputHandler_OnCancelAction;
        GameEventSystem.OnPlayerDeath += GameEventSystem_OnPlayerDeath;
    }

    private void OnDisable()
    {
        playerInputHandler.OnCancelAction -= PlayerInputHandler_OnCancelAction;
        GameEventSystem.OnPlayerDeath -= GameEventSystem_OnPlayerDeath;
    }

    #region Pause Button Action
    public void Pause()
    {
        menuManager.SetMenuState(Menus.PauseMenu);

        StopTime(true);

        if (backgroundMusic.isPlaying)
        {
            backgroundMusic.Pause();
        }

        pauseContainer.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void Resume()
    {
        menuManager.SetMenuState(Menus.InGame);
        GameEventSystem.OnGamePause?.Invoke(false);

        StopTime(false);

        backgroundMusic.Play();

        pauseContainer.SetActive(false);
        settingsPanel.SetActive(false);
    }
    #endregion

    public void StopTime(bool state)
    {
        isPaused = state;
        Cursor.visible = state;
        GameEventSystem.OnGamePause?.Invoke(state);

        if (state)
        {
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void PlayerInputHandler_OnCancelAction()
    {
        if (playerIsDead) return;

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

    private void GameEventSystem_OnPlayerDeath(float deathCounter)
    {
        playerIsDead = true;
    }

}