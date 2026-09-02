using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour {

    private const string SOUND_MASTER = "Sound_Master";
    private const string SOUND_MUSIC = "Sound_Music";
    private const string SOUND_VFX = "Sound_VFX";

    [Header("Mixer Reference")]
    [SerializeField] private SoundMixerManager soundMixerManager;

    [Header("UI Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundEffectSlider;

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI masterText;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private TextMeshProUGUI soundEffectText;

    private void Start() {
        LoadSoundSettings(); //Load the values
    }

    //Loads the values from the PlayerPrefs with 0.5 as the default value if it doesn't exists (Why 0.5? I don't want my ears to be blown ofc.)
    private void LoadSoundSettings() {
        masterSlider.value = PlayerPrefs.GetFloat(SOUND_MASTER, 0.5f);
        musicSlider.value = PlayerPrefs.GetFloat(SOUND_MUSIC, 0.5f);
        soundEffectSlider.value = PlayerPrefs.GetFloat(SOUND_VFX, 0.5f);

        OnMasterChanged(masterSlider.value);
        OnMusicChanged(musicSlider.value);
        OnSoundEffectChanged(soundEffectSlider.value);
    }

    //Saves the Sound Settings when the player lets go of the slider
    public void SaveSoundSettings() {
        PlayerPrefs.SetFloat(SOUND_MASTER, masterSlider.value);
        PlayerPrefs.SetFloat(SOUND_MUSIC, musicSlider.value);
        PlayerPrefs.SetFloat(SOUND_VFX, soundEffectSlider.value);
        PlayerPrefs.Save();
    }

    private void OnMasterChanged(float value) {
        SetSoundNumberText(masterText, value);
        soundMixerManager.SetMasterVolume(value);
    }

    private void OnMusicChanged(float value) {
        SetSoundNumberText(musicText, value);
        soundMixerManager.SetMusicVolume(value);
    }

    private void OnSoundEffectChanged(float value) {
        SetSoundNumberText(soundEffectText, value);
        soundMixerManager.SetSoundFXVolume(value);
    }

    public void SetSoundNumberText(TextMeshProUGUI textMesh, float value) {
        textMesh.text = (value * 100).ToString("F0");
    }

    private void OnEnable() {
        //Set Listeners and Update the Text
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        soundEffectSlider.onValueChanged.AddListener(OnSoundEffectChanged);
    }

    private void OnDisable() {
        masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        soundEffectSlider.onValueChanged.RemoveListener(OnSoundEffectChanged);
    }
}
