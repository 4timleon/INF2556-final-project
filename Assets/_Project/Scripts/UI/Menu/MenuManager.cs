using UnityEngine;

public class MenuManager : MonoBehaviour {

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

    private float animationScale = 1.1f;
    private float scaleDuration = 0.25f;
    private Vector3 originalScale;

    private void Start() {
        if (isPauseMenu) pauseMenuContainer.SetActive(true);
        else mainMenuContainer.SetActive(true);

        settingsMenuContainer.SetActive(false);
        graphicsContainer.SetActive(false);
        controlsContainer.SetActive(false);
        soundContainer.SetActive(false);
        quitConfirmationContainer.SetActive(false);

        originalScale = quitConfirmationContainer.transform.localScale;
    }

    #region Main Menu
    public void Play() {
        _ = LoadScene(_mainScene, true);
    }

    public void OpenSettings() {
        if (isPauseMenu) pauseMenuContainer.SetActive(false);
        else mainMenuContainer.SetActive(false);

        settingsMenuContainer.SetActive(true);
        graphicsContainer.SetActive(true);
        controlsContainer.SetActive(false);
        soundContainer.SetActive(false);
    }

    public void OpenCredits() {

    }

    public void SettingsBack() {
        if (isPauseMenu) pauseMenuContainer.SetActive(true);
        else mainMenuContainer.SetActive(true);

        settingsMenuContainer.SetActive(false);
    }

    public void MainMenu() {
        Time.timeScale = 1f;
        _ = LoadScene(_mainMenuScene, false);
    }

    public void ShowQuitConfirmation() {
        quitConfirmationContainer.SetActive(true);

        Vector3 targetScale = originalScale * animationScale; //Cache target Scale
        quitConfirmationContainer.transform.localScale = Vector3.zero; //Set Scale to 0

        quitConfirmationContainer.LeanScale(targetScale, scaleDuration)
            .setIgnoreTimeScale(true); //Tween from current Scale to target Scale
    }

    public void HideQuitConfirmation() {
        quitConfirmationContainer.LeanScale(Vector3.zero, scaleDuration)
            .setIgnoreTimeScale(true)
            .setOnComplete(() => quitConfirmationContainer.SetActive(false));
    }

    public void QuitGame() {
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
}
