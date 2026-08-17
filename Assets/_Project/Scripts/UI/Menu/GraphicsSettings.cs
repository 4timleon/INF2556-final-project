using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GraphicsSettings : MonoBehaviour {

    private UniversalAdditionalCameraData cameraData;

    private const string GRAPHICS_SCREENMODE = "Graphics_ScreenMode";
    private const string GRAPHICS_VSYNC = "Graphics_VSync";
    private const string GRAPHICS_QUALITY = "Graphics_Quality";
    private const string AA_MODE_KEY = "Graphics_AAMode";
    private const string AA_QUALITY_KEY = "Graphics_AAQuality";
    private const string ANISO_KEY = "Graphics_AnisotropicFiltering";

    [Header("URP Assets")]
    [SerializeField] private UniversalRenderPipelineAsset urpAssetLow;
    [SerializeField] private UniversalRenderPipelineAsset urpAssetMedium;
    [SerializeField] private UniversalRenderPipelineAsset urpAssetHigh;

    [Header("Screen Dropdowns")]
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    [SerializeField] private TMP_Dropdown screenResolutionDropdown;

    [Header("VSync Button")]
    [SerializeField] private Toggle vsyncButton;

    [Header("Graphic Dropdowns")]
    [SerializeField] private TMP_Dropdown graphicsDropdown;
    [SerializeField] private TMP_Dropdown antiAliasingDropdown;
    [SerializeField] private TMP_Dropdown anisotropicFilteringDropdown;

    private Resolution[] resolutions;

    private void Awake() {
        Camera mainCamera = Camera.main;

        if (mainCamera != null) {
            cameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
        }

        LoadAndApplyScreenMode();
        LoadAndApplyVSync();
        LoadAndApplyGraphicsQuality();
        LoadAndApplyAntiAliasing();
        LoadAndApplyAnisotropicFiltering();
    }

    private void Start() {
        // Set Screen settings
        resolutions = Screen.resolutions;
        screenResolutionDropdown.ClearOptions();

        // Preferred resolutions
        (int w, int h)[] preferred = new (int, int)[] {
            (3840,2160),
            (2560,1440),
            (1920,1080),
            (1600,900),
            (1366,768),
            (1280,720)
        };

        List<Resolution> filtered = new();
        List<string> options = new();

        // For each preferred resolution, pick the best matching available refresh rate (highest)
        foreach (var pref in preferred) {
            Resolution? best = null;
            for (int i = 0; i < resolutions.Length; i++) {
                if (resolutions[i].width == pref.w && resolutions[i].height == pref.h) {//Check for matching width/height
                    if (best == null || resolutions[i].refreshRateRatio.value > ((Resolution)best).refreshRateRatio.value) {//Check for best refreshRateRatio
                        best = resolutions[i];
                    }
                }
            }

            if (best != null) {
                filtered.Add((Resolution)best);
                options.Add(((Resolution)best).width + " x " + ((Resolution)best).height);
            }
        }

        // Ensure current resolution is present
        bool hasCurrent = false;
        for (int i = 0; i < filtered.Count; i++) {
            if (filtered[i].width == Screen.currentResolution.width && filtered[i].height == Screen.currentResolution.height) {
                hasCurrent = true; break;
            }
        }

        if (!hasCurrent) {
            filtered.Add(Screen.currentResolution);
            options.Add(Screen.currentResolution.width + " x " + Screen.currentResolution.height);
        }

        // Fallback: if filtered is empty (very rare), show all resolutions
        if (filtered.Count == 0) {
            filtered.AddRange(resolutions);
            options.Clear();
            for (int i = 0; i < resolutions.Length; i++) options.Add(resolutions[i].width + " x " + resolutions[i].height);
        }

        resolutions = filtered.ToArray();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++) {
            if (resolutions[i].width == Screen.currentResolution.width
            && resolutions[i].height == Screen.currentResolution.height
            && (int)resolutions[i].refreshRateRatio.value == (int)Screen.currentResolution.refreshRateRatio.value) {
                currentResolutionIndex = i; break;
            }
        }

        screenResolutionDropdown.AddOptions(options);
        screenResolutionDropdown.value = currentResolutionIndex;
        screenResolutionDropdown.RefreshShownValue();

        //Manual setting listeners
        screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        screenResolutionDropdown.onValueChanged.AddListener(OnScreenResolutionChanged);
        vsyncButton.onValueChanged.AddListener(OnVSyncChanged);

        graphicsDropdown.onValueChanged.AddListener(SetGraphicsQuality);

        antiAliasingDropdown.onValueChanged.AddListener(SetAntiAliasing);
        anisotropicFilteringDropdown.onValueChanged.AddListener(SetAnisotropicFiltering);

        // Sync UI controls with the saved/loaded values.
        ApplySavedUiValues();
    }

    private void OnDisable() {
        screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeChanged);
        screenResolutionDropdown.onValueChanged.RemoveListener(OnScreenResolutionChanged);
        vsyncButton.onValueChanged.RemoveListener(OnVSyncChanged);
        graphicsDropdown.onValueChanged.RemoveListener(SetGraphicsQuality);
        antiAliasingDropdown.onValueChanged.RemoveListener(SetAntiAliasing);
        anisotropicFilteringDropdown.onValueChanged.RemoveListener(SetAnisotropicFiltering);
    }

    #region Screen
    private void OnScreenModeChanged(int index) {
        ApplyScreenMode(index);

        PlayerPrefs.SetInt(GRAPHICS_SCREENMODE, index);
        PlayerPrefs.Save();
    }

    private void OnScreenResolutionChanged(int index) {
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }

    private void OnVSyncChanged(bool state) {
        QualitySettings.vSyncCount = state ? 1 : 0;

        PlayerPrefs.SetInt(GRAPHICS_VSYNC, QualitySettings.vSyncCount);
        PlayerPrefs.Save();
    }

    private void LoadAndApplyScreenMode() {
        int index = PlayerPrefs.GetInt(GRAPHICS_SCREENMODE);

        ApplyScreenMode(index);
    }

    private void LoadAndApplyVSync() {
        bool isOn = PlayerPrefs.GetInt(GRAPHICS_VSYNC, 0) == 1;
        QualitySettings.vSyncCount = isOn ? 1 : 0;

        if (vsyncButton != null) {
            vsyncButton.isOn = isOn;
        }
    }

    private void ApplyScreenMode(int index) {
        switch (index) {
            case 0:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                screenResolutionDropdown.interactable = true;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                screenResolutionDropdown.interactable = false;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                screenResolutionDropdown.interactable = true;
                break;
            default:
                index = 0;
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                screenResolutionDropdown.interactable = true;
                break;
        }

        if (screenModeDropdown != null) {
            screenModeDropdown.value = index;
            screenModeDropdown.RefreshShownValue();
        }
    }

    private void ApplySavedUiValues() {
        int savedScreenMode = PlayerPrefs.GetInt(GRAPHICS_SCREENMODE, 0);
        int savedQuality = PlayerPrefs.GetInt(GRAPHICS_QUALITY, GetCurrentGraphicsIndex());
        bool savedVSync = PlayerPrefs.GetInt(GRAPHICS_VSYNC, 0) == 1;

        if (screenModeDropdown != null) {
            screenModeDropdown.value = Mathf.Clamp(savedScreenMode, 0, screenModeDropdown.options.Count - 1);
            screenModeDropdown.RefreshShownValue();
        }

        if (vsyncButton != null) {
            vsyncButton.isOn = savedVSync;
        }

        if (graphicsDropdown != null) {
            graphicsDropdown.value = Mathf.Clamp(savedQuality, 0, graphicsDropdown.options.Count - 1);
            graphicsDropdown.RefreshShownValue();
        }
    }
    #endregion

    #region Graphics Quality
    private int GetCurrentGraphicsIndex() {
        var current = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;

        if (current == urpAssetLow) return 0;
        if (current == urpAssetMedium) return 1;
        if (current == urpAssetHigh) return 2;

        return 1;
    }

    private void SyncGraphicsDropdownWithCurrentAsset() {
        var current = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        if (current == null) return;

        if (current == urpAssetLow) {
            SetGraphicsDropdown(0);
        } else if (current == urpAssetMedium) {
            SetGraphicsDropdown(1);
        } else if (current == urpAssetHigh) {
            SetGraphicsDropdown(2);
        }
    }

    private void LoadAndApplyGraphicsQuality() {
        int index = PlayerPrefs.GetInt(GRAPHICS_QUALITY, GetCurrentGraphicsIndex());
        if (index < 0 || index > 2) index = GetCurrentGraphicsIndex();

        ApplyGraphicsQuality(index, false);
    }

    public void SetGraphicsQuality(int index) {
        ApplyGraphicsQuality(index, true);
    }

    private void ApplyGraphicsQuality(int index, bool saveToPrefs) {
        if (index < 0 || index > 2) {
            index = GetCurrentGraphicsIndex();
        }

        switch (index) {
            case 0:
                SetQualityLow();
                break;
            case 1:
                SetQualityMedium();
                break;
            case 2:
                SetQualityHigh();
                break;
        }

        if (saveToPrefs) {
            PlayerPrefs.SetInt(GRAPHICS_QUALITY, index);
            PlayerPrefs.Save();
        }

        SetGraphicsDropdown(index);
        SyncGraphicsDropdownWithCurrentAsset();
    }

    private void SetQualityLow() {
        QualitySettings.SetQualityLevel(1, true);

        //Set URP asset for low
        QualitySettings.renderPipeline = urpAssetLow;
    }

    private void SetQualityMedium() {
        QualitySettings.SetQualityLevel(2, true);

        //Set URP asset for medium
        QualitySettings.renderPipeline = urpAssetMedium;
    }

    private void SetQualityHigh() {
        QualitySettings.SetQualityLevel(3, true);

        //Set URP asset for high
        QualitySettings.renderPipeline = urpAssetHigh;
    }
    #endregion

    #region Dropdowns
    private void SetGraphicsDropdown(int index) {
        if (graphicsDropdown != null) {
            graphicsDropdown.value = index;
            graphicsDropdown.RefreshShownValue();
        }
    }

    private void SetAntiAliasingDropdown(int index) {
        if (antiAliasingDropdown != null) {
            antiAliasingDropdown.value = index;
            antiAliasingDropdown.RefreshShownValue();
        }
    }

    private void SetAnisotropicFilteringDropdown(int index) {
        if (anisotropicFilteringDropdown != null) {
            anisotropicFilteringDropdown.value = index;
            anisotropicFilteringDropdown.RefreshShownValue();
        }
    }
    #endregion

    #region Anti-Aliasing
    //0 = None, 1 = FXAA, 2 = SMAA, 3 = TAA
    private void SetAndSaveAntialiasing(int modeIndex) {
        AntialiasingMode mode = AntialiasingMode.None;

        switch (modeIndex) {
            case 0: mode = AntialiasingMode.None; break;
            case 1: mode = AntialiasingMode.FastApproximateAntialiasing; break;
            case 2: mode = AntialiasingMode.SubpixelMorphologicalAntiAliasing; break;
            case 3: mode = AntialiasingMode.TemporalAntiAliasing; break;
        }

        ApplyAntiAliasing(mode, AntialiasingQuality.High);

        PlayerPrefs.SetInt(AA_MODE_KEY, (int)mode);
        PlayerPrefs.SetInt(AA_QUALITY_KEY, (int)AntialiasingQuality.High);
        PlayerPrefs.Save();
    }

    private void LoadAndApplyAntiAliasing() {
        if (cameraData == null) return;

        int savedMode = PlayerPrefs.GetInt(AA_MODE_KEY, (int)AntialiasingMode.TemporalAntiAliasing);
        int savedQuality = PlayerPrefs.GetInt(AA_QUALITY_KEY, (int)AntialiasingQuality.High);

        ApplyAntiAliasing((AntialiasingMode)savedMode, (AntialiasingQuality)savedQuality);
    }

    private void ApplyAntiAliasing(AntialiasingMode mode, AntialiasingQuality quality) {
        if (cameraData != null) {
            cameraData.antialiasing = mode;
            cameraData.antialiasingQuality = quality;
            SetAntiAliasingDropdown((int)mode);
        }
    }
    #endregion

    #region Anti-A. & Aniso. F. Button Listener
    //Anti-Aliasing Button Listener
    public void SetAntiAliasing(int index) {
        SetAndSaveAntialiasing(index);
    }

    //Anisotropic Filtering Button Listener
    public void SetAnisotropicFiltering(int index) {
        ApplyAnisotropicFiltering(index);

        PlayerPrefs.SetInt(ANISO_KEY, index);
        PlayerPrefs.Save();

        SetAnisotropicFilteringDropdown(index);
    }
    #endregion

    #region Anisotropic Filtering
    private void ApplyAnisotropicFiltering(int index) {
        if (index == 0) QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        else if (index == 1) QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        else QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
    }

    private void LoadAndApplyAnisotropicFiltering() {
        int savedIndex = PlayerPrefs.GetInt(ANISO_KEY, 2);

        ApplyAnisotropicFiltering(savedIndex);
        SetAnisotropicFilteringDropdown(savedIndex);
    }
    #endregion
}
