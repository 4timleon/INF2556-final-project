using System;
using UnityEngine;

public class PauseMenuController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;

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
        playerInputHandler.OnPauseAction += PlayerInputHandler_OnPauseAction;
    }

    private void OnDisable() {
        playerInputHandler.OnPauseAction -= PlayerInputHandler_OnPauseAction;
    }

    #region Pause Button Action
    public void Pause() {
        isPaused = true;
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        OnGamePaused?.Invoke(true);

        pauseContainer.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void Resume() {
        isPaused = false;
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        OnGamePaused?.Invoke(false);

        pauseContainer.SetActive(false);
        settingsPanel.SetActive(false);
    }
    #endregion

    private void PlayerInputHandler_OnPauseAction() {
        if (isPaused) Resume();
        else Pause();
    }
}