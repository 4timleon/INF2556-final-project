using UnityEngine;

public class MenuManager : MonoBehaviour {

    [Header("Menus")]
    [SerializeField] private GameObject mainMenuContainer;
    [SerializeField] private GameObject settingsMenuContainer;

    [Header("Settings")]
    [SerializeField] private GameObject graphicsContainer;
    [SerializeField] private GameObject controlsContainer;
    [SerializeField] private GameObject soundContainer;

    private void Start() {
        mainMenuContainer.SetActive(true);
        settingsMenuContainer.SetActive(false);
        graphicsContainer.SetActive(false);
        controlsContainer.SetActive(false);
        soundContainer.SetActive(false);
    }

    #region Main Menu
    public void Play() {

    }

    public void OpenSettings() {
        mainMenuContainer.SetActive(false);
        settingsMenuContainer.SetActive(true);
        graphicsContainer.SetActive(true);
    }

    public void OpenCredits() {

    }

    public void SettingsBack() {
        mainMenuContainer.SetActive(true);
        settingsMenuContainer.SetActive(false);
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
}
